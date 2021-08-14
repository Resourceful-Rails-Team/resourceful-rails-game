/* This work is released under the MIT license.
    Please see the file LICENSE in this distribution for
    license terms. */

using Rails.Data;
using Rails.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Rails.Editor
{
    /// <summary>
    /// Custom inspector editor for the Manager component.
    /// </summary>
    [CustomEditor(typeof(Manager))]
    public class ManagerEditor : UnityEditor.Editor
    {
        /// <summary>
        /// Raised when the Unity Editor is building the inspector GUI.
        /// </summary>
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
                Undo.RecordObject(manager.MapData, "Generate map");
                Generate(manager.MapData);
            }

            GUILayout.EndVertical();
        }
        
        /// <summary>
        /// Generates a new map with default data.
        /// </summary>
        private void Generate(MapData mapData)
        {
            var size = Manager.Size;
            mapData.Nodes = new Node[size * size];
            mapData.Segments = new NodeSegment[size * size * 6];
            Node node;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    // build node
                    node = new Node(new NodeId(x, y));
                    mapData.Nodes[node.Id.GetSingleId()] = node;

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
