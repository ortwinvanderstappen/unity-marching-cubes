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

        // Dynamic chunk properties
        [Header("Dynamic properties")]
        [SerializeField] private float _chunkDistance = 50.0f;
        [SerializeField] private float _chunkDespawnDistanceOffset = 10.0f;

        [SerializeField] private float _isoLevel;
        [SerializeField] private float _surfaceLevel = 0.0f;
        [SerializeField] private float _flattenScale = 1.0f;

        [Header("Debugging")]
        [SerializeField] private bool _drawGizmos;

        // Buffers
        [Header("Data properties")]
        [SerializeField] private List<Chunk> _chunks;
        [SerializeField] Transform _playerTransform;

        [Header("Runtime generation")]
        [SerializeField] private bool _regenerateChunks = false;
        private float _chunkGenerationCheckDelay = .33f;
        private float _chunkGenerationDelay = .01f;

        private void Start()
        {
            const bool useChunkLoadingDelay = false;
            StartCoroutine(GenerateChunksInRenderDistance(useChunkLoadingDelay));
            StartCoroutine(GenerateChunkPositionAfterDelay());
        }

        private IEnumerator GenerateChunkPositionAfterDelay()
        {
            yield return new WaitForSeconds(_chunkGenerationCheckDelay);

            // Check if new chunks need to get generated
            const bool useChunkLoadingDelay = true;
            StartCoroutine(GenerateChunksInRenderDistance(useChunkLoadingDelay));

            // Infinitely loop coroutine
            StartCoroutine(GenerateChunkPositionAfterDelay());
        }

        private void Update()
        {
            RemoveChunksOutOfRenderDistance();

            // Handle forced chunk regeneration via editor boolean
            if (_regenerateChunks)
            {
                _regenerateChunks = false;
                RegenerateChunks();
            }
        }

        private void RemoveChunksOutOfRenderDistance()
        {
            List<Chunk> chunksToRemove = new List<Chunk>();

            // Go through all the current chunks
            int chunkIndex = 0;
            foreach (Chunk c in _chunks)
            {
                // Determine if chunk is out of player range and unload it
                float distanceToPlayer = Vector3.Distance(c.transform.position, _playerTransform.position);
                if (distanceToPlayer >= _chunkSize.x + _chunkDistance + _chunkDespawnDistanceOffset)
                {
                    chunksToRemove.Add(c);
                    Destroy(c.gameObject);
                }

                ++chunkIndex;
            }

            // Remove dead chunks
            foreach (Chunk chunkToRemove in chunksToRemove)
            {
                _chunks.Remove(chunkToRemove);
            }
        }

        private IEnumerator GenerateChunksInRenderDistance(bool useDelay)
        {
            // Load required chunks around current player position taking into account chunk distance
            Vector3 startPosition = _playerTransform.position;
            startPosition -= new Vector3(_chunkDistance * 0.5f, _chunkDistance * 0.5f, _chunkDistance * 0.5f);
            Vector3 currentPosition = startPosition;

            while (currentPosition.x <= _playerTransform.position.x + _chunkDistance)
            {
                while (currentPosition.y <= _playerTransform.position.y + _chunkDistance)
                {
                    while (currentPosition.z <= _playerTransform.position.z + _chunkDistance)
                    {
                        Chunk chunk = PositionToChunk(currentPosition);
                        if (chunk == null)
                        {
                            if (useDelay)
                            {
                                yield return new WaitForSeconds(_chunkGenerationDelay);
                            }

                            CreateChunkAtPosition(currentPosition);
                        }

                        // Increment depth
                        currentPosition.z += _chunkSize.z;
                    }

                    // Increment height
                    currentPosition.y += _chunkSize.y;
                    // Reset depth
                    currentPosition.z = startPosition.z;
                }

                // Increment width
                currentPosition.x += _chunkSize.x;
                // Reset height
                currentPosition.y = startPosition.y;
            }
        }

        private void RegenerateChunks()
        {
            foreach (Chunk chunk in _chunks)
            {
                SetDynamicChunkProperties(chunk);
                chunk.GenerateChunk();
            }
        }

        Chunk PositionToChunk(Vector3 position)
        {
            foreach (Chunk c in _chunks)
            {
                Bounds bounds = new Bounds();
                bounds.center = c.gameObject.transform.position;
                bounds.size = _chunkSize;
                bounds.extents = bounds.size * 0.5f;

                if (bounds.Contains(position))
                {
                    return c;
                }
            }

            return null;
        }

        void CreateChunkAtPosition(Vector3 position)
        {
            Vector3Int chunkIndex3D = new Vector3Int(
                Mathf.RoundToInt(position.x / _chunkSize.x),
                Mathf.RoundToInt(position.y / _chunkSize.y),
                Mathf.RoundToInt(position.z / _chunkSize.z)
            );

            CreateChunk(chunkIndex3D.x, chunkIndex3D.y, chunkIndex3D.z);
        }

        private void CreateChunk(int x, int y, int z)
        {
            Vector3 chunkPosition = new Vector3(_chunkSize.x * x, _chunkSize.y * y, _chunkSize.z * z);

            // Create chunk object
            GameObject chunkObject = Instantiate(_chunkPrefab, chunkPosition, Quaternion.identity);

            Vector3Int alteredChunkSize = _chunkSize;
            // Make sure chunkSize is uneven
            if (alteredChunkSize.x % 2 == 0) ++alteredChunkSize.x;
            if (alteredChunkSize.y % 2 == 0) ++alteredChunkSize.y;
            if (alteredChunkSize.z % 2 == 0) ++alteredChunkSize.z;

            // Set chunk properties
            Chunk chunk = chunkObject.GetComponent<Chunk>();
            // Static properties
            chunk.ChunkSize = alteredChunkSize;
            chunk.PointDensity = _pointDensity;
            chunk.transform.parent = transform;
            // Dynamic properties
            SetDynamicChunkProperties(chunk);

            // Add chunk to list
            _chunks.Add(chunk);
        }

        private void SetDynamicChunkProperties(Chunk chunk)
        {
            chunk.IsoLevel = _isoLevel;
            chunk.DrawGizmos = _drawGizmos;
            chunk.SurfaceLevel = _surfaceLevel;
            chunk.FlattenScale = _flattenScale;
        }

        private void OnDrawGizmos()
        {
            //Gizmos.DrawCube(_playerTransform.position, new Vector3(_chunkDistance, _chunkDistance, _chunkDistance));
        }
    }
}
