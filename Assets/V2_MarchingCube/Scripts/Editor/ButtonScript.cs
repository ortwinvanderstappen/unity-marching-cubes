using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace V2
{
    [CustomEditor(typeof(ChunkManager))]
    public class ButtonScript : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Generate meshes"))
            {
                ChunkManager chunkManager = (ChunkManager)target;
                chunkManager.StartChunkGeneration();
            }
        }
    }
}