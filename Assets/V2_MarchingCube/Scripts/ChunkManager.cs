using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace V2
{
    public class ChunkManager : MonoBehaviour
    {
        // Static chunk properties
        [Header("Static properties")]
        [SerializeField] private GameObject _chunkPrefab;
        [SerializeField] private Vector3Int _chunkSize;
        [SerializeField] private int _pointDensity;
        [SerializeField] private int _chunkCount;

        // Dynamic chunk properties
        [Header("Dynamic properties")]
        [SerializeField] private float _generationDelay = 0.5f;
        [SerializeField] private float _surfaceLevel;
        [SerializeField] private bool _drawGizmos;
        [SerializeField] private float _noiseScale;
        [SerializeField] private float _pointScale;

        // Buffers
        [Header("Data properties")]
        [SerializeField] private Chunk[,,] _chunkArray;

        void Start()
        {
            _chunkArray = new Chunk[_chunkCount, _chunkCount, _chunkCount];
            SpawnChunks();
        }

        public void StartChunkGeneration()
        {
            StartCoroutine(GenerateChunks());
        }
        private IEnumerator GenerateChunks()
        {
            for (int i = 0; i < _chunkCount; i++)
            {
                for (int j = 0; j < _chunkCount; j++)
                {
                    for (int k = 0; k < _chunkCount; k++)
                    {
                        Chunk chunk = _chunkArray[i, j, k];
                        Debug.Log($"Generating chunk [{i},{j},{k}]");
            

                        // Setup new chunk properties
                                    chunk.SurfaceLevel = _surfaceLevel;
                        chunk.DrawGizmos = _drawGizmos;
                        chunk.NoiseScale = _noiseScale;
                        chunk.PointScale = _pointScale;

                        chunk.GenerateChunk();
                        yield return new WaitForSeconds(_generationDelay);
                    }
                }
            }
        }

        private void SpawnChunks()
        {
            for (int i = 0; i < _chunkCount; i++)
            {
                for (int j = 0; j < _chunkCount; j++)
                {
                    for (int k = 0; k < _chunkCount; k++)
                    {
                        CreateChunk(i, j, k);
                    }
                }
            }
        }

        private void CreateChunk(int x, int y, int z)
        {
            // 8 * 1 * 0.5 = 4
            Vector3 chunkPosition = new Vector3(_chunkSize.x * x, _chunkSize.y * y, _chunkSize.z * z);
            Debug.Log("ChunkManager, creating chunk at position: " + chunkPosition);

            // Create chunk object
            GameObject chunkObject = Instantiate(_chunkPrefab, chunkPosition, Quaternion.identity);

            Vector3Int alteredChunkSize = _chunkSize;
            // Make sure chunkSize is uneven
            if (alteredChunkSize.x % 2 == 0) ++alteredChunkSize.x;
            if (alteredChunkSize.y % 2 == 0) ++alteredChunkSize.y;
            if (alteredChunkSize.z % 2 == 0) ++alteredChunkSize.z;

            // Set chunk properties
            Chunk chunk = chunkObject.GetComponent<Chunk>();
            chunk.ChunkSize = alteredChunkSize;
            chunk.PointDensity = _pointDensity;
            chunk.DrawGizmos = _drawGizmos;

            _chunkArray[x,y,z] = chunk;
        }
    }
}
