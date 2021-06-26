using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace V1
{
    public class Chunk : MonoBehaviour
    {
        public struct CubePointStruct
        {
            public Vector3 position;
            public float noiseValue;
        }

        // Properties
        [SerializeField] private Vector3 _areaSize;
        [SerializeField] private float _pointDensity;
        public float PointDensity { get; private set; }
        public Vector3 Offset { get; private set; }

        // Points
        private CubePointStruct[,,] _cubePoints;
        [SerializeField] private GameObject _cubePointPrefab;

        // Indices
        private int _renderedWidthIndex = 0;
        private int _renderedHeightIndex = 0;
        private int _renderedDepthIndex = -1;
        private int _width, _height, _depth;

        public bool MarchComplete { get; private set; } = false;

        void Start()
        {
            GeneratePoints();
        }

        void Update()
        {
            DrawAreaBounds();
        }

        private void DrawAreaBounds()
        {
            // Vertical lines
            Debug.DrawLine(new Vector3(-_areaSize.x, -_areaSize.y, -_areaSize.z), new Vector3(-_areaSize.x, _areaSize.y, -_areaSize.z));
            Debug.DrawLine(new Vector3(-_areaSize.x, -_areaSize.y, _areaSize.z), new Vector3(-_areaSize.x, _areaSize.y, _areaSize.z));
            Debug.DrawLine(new Vector3(_areaSize.x, -_areaSize.y, -_areaSize.z), new Vector3(_areaSize.x, _areaSize.y, -_areaSize.z));
            Debug.DrawLine(new Vector3(_areaSize.x, -_areaSize.y, _areaSize.z), new Vector3(_areaSize.x, _areaSize.y, _areaSize.z));

            // X horizontal
            Debug.DrawLine(new Vector3(-_areaSize.x, -_areaSize.y, -_areaSize.z), new Vector3(_areaSize.x, -_areaSize.y, -_areaSize.z)); // near bot
            Debug.DrawLine(new Vector3(-_areaSize.x, +_areaSize.y, -_areaSize.z), new Vector3(_areaSize.x, +_areaSize.y, -_areaSize.z)); // near top
            Debug.DrawLine(new Vector3(-_areaSize.x, -_areaSize.y, +_areaSize.z), new Vector3(_areaSize.x, -_areaSize.y, +_areaSize.z)); // far bot
            Debug.DrawLine(new Vector3(-_areaSize.x, +_areaSize.y, +_areaSize.z), new Vector3(_areaSize.x, +_areaSize.y, +_areaSize.z)); // far top

            // Z horitonzal
            Debug.DrawLine(new Vector3(-_areaSize.x, -_areaSize.y, -_areaSize.z), new Vector3(-_areaSize.x, -_areaSize.y, +_areaSize.z)); // bottom left
            Debug.DrawLine(new Vector3(_areaSize.x, -_areaSize.y, -_areaSize.z), new Vector3(_areaSize.x, -_areaSize.y, +_areaSize.z)); // bottom right
            Debug.DrawLine(new Vector3(-_areaSize.x, +_areaSize.y, -_areaSize.z), new Vector3(-_areaSize.x, +_areaSize.y, +_areaSize.z)); // top left
            Debug.DrawLine(new Vector3(_areaSize.x, +_areaSize.y, -_areaSize.z), new Vector3(_areaSize.x, +_areaSize.y, +_areaSize.z)); // top right
        }

        private void GeneratePoints()
        {
            Vector3 fullArenaSize = _areaSize * 2;
            Offset = fullArenaSize / _pointDensity;

            Vector3 startPos = transform.position - _areaSize;
            Vector3 pos = startPos;

            _width = (int)(fullArenaSize.x / Offset.x);
            _height = (int)(fullArenaSize.y / Offset.y);
            _depth = (int)(fullArenaSize.z / Offset.z);

            // Initialize array
            _cubePoints = new CubePointStruct[_width + 1, _height + 1, _depth + 1];

            for (int i = 0; i < _width + 1; i++)
            {
                for (int j = 0; j < _height + 1; j++)
                {
                    for (int k = 0; k < _depth + 1; k++)
                    {
                        CreatePoint(i, j, k, pos);
                        pos.z += Offset.z;
                    }
                    pos.y += Offset.y;
                    pos.z = startPos.z;
                }
                pos.x += Offset.x;
                pos.y = startPos.y;
            }
        }

        private void CreatePoint(int i, int j, int k, Vector3 position)
        {
            // Generate noise
            float generatedNoise = Perlin3D(new Vector3((float)i / _width, (float)j / _height, (float)k / _depth));

            // Create the point
            CubePointStruct cps = new CubePointStruct();
            cps.position = position;
            cps.noiseValue = generatedNoise;

            // Store the point
            _cubePoints[i, j, k] = cps;
        }

        public Vector3 MarchCube()
        {
            Vector3 fullArenaSize = _areaSize * 2;
            Vector3 startPos = -_areaSize + (Offset * 0.5f);

            int width = (int)(fullArenaSize.x / Offset.x);
            int height = (int)(fullArenaSize.y / Offset.y);
            int depth = (int)(fullArenaSize.z / Offset.z);

            ++_renderedDepthIndex;
            if (_renderedDepthIndex >= depth)
            {
                _renderedDepthIndex = 0;
                ++_renderedHeightIndex;
                if (_renderedHeightIndex >= height)
                {
                    _renderedHeightIndex = 0;
                    ++_renderedWidthIndex;
                    if (_renderedWidthIndex >= width - 1)
                    {
                        MarchComplete = true;
                    }
                }
            }

            return new Vector3(
                startPos.x + _renderedWidthIndex * Offset.x,
                startPos.y + _renderedHeightIndex * Offset.y,
                startPos.z + _renderedDepthIndex * Offset.z);
        }

        public CubePointStruct[] GetNearbyPoints()
        {
            CubePointStruct[] cubePoints = new CubePointStruct[8];

            cubePoints[0] = _cubePoints[_renderedWidthIndex, _renderedHeightIndex, _renderedDepthIndex + 1]; // far bottom left
            cubePoints[1] = _cubePoints[_renderedWidthIndex + 1, _renderedHeightIndex, _renderedDepthIndex + 1]; // far bottom right
            cubePoints[2] = _cubePoints[_renderedWidthIndex + 1, _renderedHeightIndex, _renderedDepthIndex]; // near bottom right 
            cubePoints[3] = _cubePoints[_renderedWidthIndex, _renderedHeightIndex, _renderedDepthIndex]; // near bottom left
            cubePoints[4] = _cubePoints[_renderedWidthIndex, _renderedHeightIndex + 1, _renderedDepthIndex + 1]; // far top left
            cubePoints[5] = _cubePoints[_renderedWidthIndex + 1, _renderedHeightIndex + 1, _renderedDepthIndex + 1]; // far top right
            cubePoints[6] = _cubePoints[_renderedWidthIndex + 1, _renderedHeightIndex + 1, _renderedDepthIndex]; // near top right
            cubePoints[7] = _cubePoints[_renderedWidthIndex, _renderedHeightIndex + 1, _renderedDepthIndex]; // near top left

            return cubePoints;
        }

        private float Perlin3D(Vector3 p)
        {
            float AB = Mathf.PerlinNoise(p.x, p.y);
            float BC = Mathf.PerlinNoise(p.y, p.z);
            float AC = Mathf.PerlinNoise(p.x, p.z);

            float BA = Mathf.PerlinNoise(p.y, p.x);
            float CB = Mathf.PerlinNoise(p.z, p.y);
            float CA = Mathf.PerlinNoise(p.z, p.x);

            float ABC = AB + BC + AC + BA + CB + CA;
            return ABC / 6f;
        }
    }
}