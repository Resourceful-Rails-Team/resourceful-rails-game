using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rails.MapEditor.Editor
{
    public class MapEditorWindow : EditorWindow
    {
        public enum MapEditorPaintType
        {
            Node,
            Segment
        }

        // 
        bool paintingActive = false;
        float paintingRadius = 1f;
        MapEditorPaintType paintType = MapEditorPaintType.Node;
        NodeType paintNodeType = NodeType.Clear;
        NodeSegmentType paintSegmentType = NodeSegmentType.None;

        // Add menu named "My Window" to the Window menu
        [MenuItem("Window/Map Editor")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            MapEditorWindow window = (MapEditorWindow)EditorWindow.GetWindow(typeof(MapEditorWindow));
            window.Show();
        }

        // 
        void OnGUI()
        {
            // 
            paintingActive = GUILayout.Toggle(paintingActive, paintingActive ? "Disable Painting" : "Enable Painting", "Button");

            // painting specific 
            EditorGUI.BeginDisabledGroup(!paintingActive);
            paintType = (MapEditorPaintType)EditorGUILayout.EnumPopup("Paint type", paintType);
            paintingRadius = EditorGUILayout.Slider("Paint Brush Size", paintingRadius, 0.1f, 10f);

            if (paintType == MapEditorPaintType.Node)
            {
                paintNodeType = (NodeType)EditorGUILayout.EnumPopup("Node type", paintNodeType);
            }
            else
            {
                paintSegmentType = (NodeSegmentType)EditorGUILayout.EnumPopup("Segment type", paintSegmentType);
            }

            EditorGUI.EndDisabledGroup();
            
        }

        private void Update()
        {
            var cursor = MapEditorCursor.Singleton;
            cursor.Visible = paintingActive;
            cursor.Radius = paintingRadius;
            cursor.Color = paintType == MapEditorPaintType.Node ? MapEditorUtils.GetNodeColor(paintNodeType) : MapEditorUtils.GetSegmentColor(paintSegmentType);
        }
    }
}
