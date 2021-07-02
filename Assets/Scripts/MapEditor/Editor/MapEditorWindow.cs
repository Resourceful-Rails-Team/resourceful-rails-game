using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rails.MapEditor.Editor
{
    public class MapEditorWindow : EditorWindow
    {
        // 
        bool paintingActive = false;
        float paintingRadius = 1f;

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
            paintingRadius = EditorGUILayout.Slider("Paint Brush Size", paintingRadius, 0.1f, 10f);
            EditorGUI.EndDisabledGroup();
            
        }

        private void Update()
        {
            var cursor = MapEditorCursor.Singleton;
            cursor.enabled = cursor.Visible = paintingActive;
            cursor.Radius = paintingRadius;
        }
    }
}
