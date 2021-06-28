using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace V2
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class Chunk : MonoBehaviour
    {
        // Chunk properties
        [SerializeField] private Vector3Int _chunkSize; public Vector3Int ChunkSize { get { return _chunkSize; } set { _chunkSize = value; } }
        [SerializeField] private int _pointDensity; public int PointDensity { get { return _pointDensity; } set { _pointDensity = value; } }
        // Debugging
        [SerializeField] private bool _drawGizmos; public bool DrawGizmos { get { return _drawGizmos; } set { _drawGizmos = value; } }
        [SerializeField] private Material _meshMaterial;

        // Dynamic properties
        public float SurfaceLevel { get; set; }
        public float FlattenScale { get; set; }
        public float IsoLevel { get; set; }
        public Vector3 Offset { get; set; }

        // Rendering components
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private Mesh _mesh;

        // Noise points
        [SerializeField] private V2.NoisePoint[] _noisePoints;
        private int _width, _height, _depth;

        // Compute shaders
        [SerializeField] private ComputeShader _noiseGenerateShader;
        [SerializeField] private ComputeShader _marchingCubesShader;

        // CPU Buffers
        private V2.Triangle[] _triangleBuffer;
        // GPU Buffers
        ComputeBuffer _noiseGeneratorNoiseBuffer;
        ComputeBuffer _triangleComputeBuffer;
        ComputeBuffer _marchingCubeNoiseBuffer;

        // Debug buffers
        //ComputeBuffer _noisePositionBuffer;

        //[SerializeField] private List<float> _noiseValues;
        [SerializeField] private Vector3[] _noiseValues;

        void Start()
        {

            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();

            _mesh = new Mesh();
            _meshFilter.sharedMesh = _mesh;

            _meshRenderer.sharedMaterial = _meshMaterial;

            GenerateChunkPoints();

            _noiseValues = new Vector3[_noisePoints.Length];
            _noiseGeneratorNoiseBuffer = new ComputeBuffer(_noisePoints.Length, sizeof(float) * 4);
            _marchingCubeNoiseBuffer = new ComputeBuffer(_noisePoints.Length, sizeof(float) * 4);
            _triangleComputeBuffer = new ComputeBuffer(_noisePoints.Length * 5, sizeof(float) * 3 * 3, ComputeBufferType.Append);

            Offset = transform.position;

            GenerateChunk();

            // Debug
            //_noisePositionBuffer = new ComputeBuffer(_noisePoints.Length, sizeof(float) * 4);
        }

        private void OnDestroy()
        {
            _noiseGeneratorNoiseBuffer.Release();
            _marchingCubeNoiseBuffer.Release();
            _triangleComputeBuffer.Release();
        }

        public void GenerateChunk()
        {
            ComputeNoise();
        }

        private void GenerateChunkPoints()
        {
            _width = ChunkSize.x - 1;
            _height = ChunkSize.y - 1;
            _depth = ChunkSize.z - 1;

            // Determine noise point array size
            Vector3 distanceBetweenPoints = new Vector3(_width, _height, _depth) / (_pointDensity - 1);

            // Initialize point array
            _noisePoints = new V2.NoisePoint[_pointDensity * _pointDensity * _pointDensity];

            for (int x = 0; x < _pointDensity; x++)
            {
                for (int y = 0; y < _pointDensity; y++)
                {
                    for (int z = 0; z < _pointDensity; z++)
                    {
                        CreateNoisePoint(x, y, z, distanceBetweenPoints);
                    }
                }
            }
        }
        private void CreateNoisePoint(int x, int y, int z, Vector3 distanceBetweenPoints)
        {
            // Calculate start position
            Vector3 chunkDimensions = new Vector3(_chunkSize.x - 1, _chunkSize.y - 1, _chunkSize.z - 1);
            Vector3 pointPosition = - (chunkDimensions)* 0.5f;

            // Increment position per point
            pointPosition += new Vector3(x * distanceBetweenPoints.x, y * distanceBetweenPoints.y, z * distanceBetweenPoints.z);

            // Set a default noise value (for debugging)
            float noise = -999.0f;

            // Create noise point
            int index = x + (y * _pointDensity) + (z * _pointDensity * _pointDensity);
            _noisePoints[index] = new V2.NoisePoint(pointPosition, noise);
        }
        private void ComputeNoise()
        {
            // Static properties
            _noiseGenerateShader.SetInt("width", _pointDensity);
            _noiseGenerateShader.SetInt("height", _pointDensity);

            // Dynamic properties
            _noiseGenerateShader.SetVector("offset", transform.position);
            _noiseGenerateShader.SetFloat("surfaceLevel", SurfaceLevel);
            _noiseGenerateShader.SetFloat("flattenScale", FlattenScale);
            _noiseGenerateShader.SetVector("offset", Offset);

            // Buffer data
            _noiseGeneratorNoiseBuffer.SetData(_noisePoints);
            _noiseGenerateShader.SetBuffer(0, "noiseBuffer", _noiseGeneratorNoiseBuffer);
            //_noiseGenerateShader.SetBuffer(0, "noisePositionBuffer", _noisePositionBuffer);

            // Execute the shader
            const int numThreads = 8;
            int xThreads = Mathf.CeilToInt((float)_pointDensity / numThreads);
            int yThreads = Mathf.CeilToInt((float)_pointDensity / numThreads);
            int zThreads = Mathf.CeilToInt((float)_pointDensity / numThreads);
            _noiseGenerateShader.Dispatch(0, xThreads, yThreads, zThreads);

            // Handle buffer callbacks to make sure they get completed
            System.Action<AsyncGPUReadbackRequest> noiseBufferCallback = noiseBufferRequest => OnComputeNoiseBufferComplete(noiseBufferRequest);
            AsyncGPUReadback.Request(_noiseGeneratorNoiseBuffer, noiseBufferCallback);
        }
        private void OnComputeNoiseBufferComplete(AsyncGPUReadbackRequest noiseBufferRequest)
        {
            // Read data from buffer
            _noisePoints = noiseBufferRequest.GetData<V2.NoisePoint>().ToArray();

            // Debug
            for (int i = 0; i < _noisePoints.Length; i++)
            {
                _noiseValues[i] = _noisePoints[i].position;
            }

            GenerateMesh();
        }

        private void GenerateMesh()
        {
            // Setup properties
            _marchingCubesShader.SetFloat("isoLevel", IsoLevel);
            _marchingCubesShader.SetInt("width", PointDensity);
            _marchingCubesShader.SetInt("height", PointDensity);
            _marchingCubesShader.SetInt("depth", PointDensity);

            // Point buffer
            _marchingCubeNoiseBuffer.SetData(_noisePoints);
            _marchingCubesShader.SetBuffer(0, "points", _marchingCubeNoiseBuffer);
            // Vertex buffer
            _marchingCubesShader.SetBuffer(0, "triangles", _triangleComputeBuffer);
            _triangleComputeBuffer.SetCounterValue(0);

            // Execute the shader
            const int numThreads = 8;
            int xThreads = Mathf.CeilToInt((float)_pointDensity / numThreads);
            int yThreads = Mathf.CeilToInt((float)_pointDensity / numThreads);
            int zThreads = Mathf.CeilToInt((float)_pointDensity / numThreads);
            _marchingCubesShader.Dispatch(0, xThreads, yThreads, zThreads);

            // Handle buffer callbacks to make sure they get completed
            System.Action<AsyncGPUReadbackRequest> triangleBufferCallback = triangleBufferRequest => OnTriangleBufferComplete(triangleBufferRequest);
            AsyncGPUReadback.Request(_triangleComputeBuffer, triangleBufferCallback);
        }
        private void OnTriangleBufferComplete(AsyncGPUReadbackRequest triangleBufferRequest)
        {
            // Determine triangle count via a count buffer
            int triangleCount = V2.Helper.GetAppendBufferSize(_triangleComputeBuffer);

            // Make sure we have a correct trianglecount
            if (triangleCount < 0 || triangleCount > _triangleComputeBuffer.count)
            {
                Debug.LogError("Wrong triangles... count: " + triangleCount);
                return;
            };

            if (triangleCount == 0)
            {
                _mesh.Clear();
                return;
            }

            _triangleBuffer = new Triangle[triangleCount];

            // Read data from buffer and put it in the triangle buffer (sub array to only read the used data)
            _triangleBuffer = triangleBufferRequest.GetData<V2.Triangle>().GetSubArray(0, triangleCount).ToArray();

            // Create the mesh
            CreateMesh();
        }

        private void CreateMesh()
        {
            int triangleCount = _triangleBuffer.Length * 3;

            int[] indices = new int[triangleCount];
            Vector3[] vertices = new Vector3[triangleCount];

            int index = 0;
            foreach (V2.Triangle t in _triangleBuffer)
            {
                for (int i = 0; i < 3; i++)
                {
                    // Add the vertex
                    if (i == 0)
                        vertices[index] = t.v1;
                    if (i == 1)
                        vertices[index] = t.v2;
                    if (i == 2)
                        vertices[index] = t.v3;

                    // Add the index
                    indices[index] = index;
                    ++index;
                }
            }

            _mesh.Clear();

            _mesh.vertices = vertices;
            _mesh.triangles = indices;

            _mesh.RecalculateNormals();
        }

        private void OnDrawGizmos()
        {
            if (!DrawGizmos) return;
            DrawNoisePointGizmos();
        }

        private void DrawNoisePointGizmos()
        {
            const float size = 0.05f;

            foreach (V2.NoisePoint noisePoint in _noisePoints)
            {
                // Obtain the noise value
                float noiseValueColor = noisePoint.noiseValue;
                // Map it from [-1, 1] to [0, 1] for color values
                noiseValueColor = (noiseValueColor + 1) * 0.5f;

                if (noisePoint.noiseValue < -900)
                    Gizmos.color = new Color(1.0f, 0.0f, 0.0f);
                else
                    Gizmos.color = new Color(noiseValueColor, noiseValueColor, noiseValueColor);

                Gizmos.DrawCube(transform.position + noisePoint.position, new Vector3(size, size, size));
            }
        }
    }
}
