using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rails.Editor
{
    [CustomEditor(typeof(Manager))]
    public class ManagerEditor : UnityEditor.Editor
    {
        private int _mapSize = 64;

        public override void OnInspectorGUI()
        {
            //
            Manager manager = this.target as Manager;

            //
            base.OnInspectorGUI();

            // 
            GUILayout.BeginVertical();
            GUILayout.Label("Grid Generation");

            if (GUILayout.Button("Regenerate Grid"))
            {
                Generate(manager.Map);
                EditorUtility.SetDirty(manager.Map);
            }

            GUILayout.EndVertical();
        }
        
        private void Generate(MapData mapData)
        {
            var size = Manager.Size;
            mapData.Nodes = new Node[size, size];
            mapData.Segments = new NodeSegment[size * size * 6];
            Node node;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    // build node
                    node = new Node(new NodeId(x, y));
                    mapData.Nodes[x, y] = node;

                    // build 6 segments per node
                    var segIndex = node.Id.GetSingleId() * 6;
                    for (Cardinal c = Cardinal.N; c < Cardinal.MAX_CARDINAL; ++c)
                    {
                        mapData.Segments[segIndex + (int)c] = new NodeSegment();
                    }
                }
            }
        }
    }
}
