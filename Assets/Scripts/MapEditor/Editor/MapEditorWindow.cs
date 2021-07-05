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
        private MapEditorCursor _cursor;
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
            paintingRadius = EditorGUILayout.Slider("Paint Brush Size", paintingRadius, 0.1f, 30f);

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
            // get cursor
            var cursor = MapEditorCursor.Singleton;
            if (_cursor != cursor)
            {
                _cursor = cursor;

                // resubscribe in case we're aren't already
                cursor.OnPaint -= OnPaint;
                cursor.OnPaint += OnPaint;
            }

            // set properties
            cursor.Visible = paintingActive;
            cursor.Radius = paintingRadius;
            cursor.Color = paintType == MapEditorPaintType.Node ? Utilities.GetNodeColor(paintNodeType) : Utilities.GetSegmentColor(paintSegmentType);
            cursor.HighlightSelectedNodes = paintType == MapEditorPaintType.Node;
            cursor.HighlightSelectedSegments = paintType == MapEditorPaintType.Segment;
        }

        private void OnPaint(object sender, Vector3 position)
        {
            var manager = Manager.Singleton;
            if (!manager)
                return;

            // record undo
            Undo.RecordObject(manager.Map, "Paint");

            // get surrounding nodes
            var nodeIds = manager.GetNodeIdsByPosition(position, paintingRadius);

            switch (paintType)
            {
                case MapEditorPaintType.Node:
                    {
                        // update node types
                        foreach (var nodeId in nodeIds)
                        {
                            var node = manager.Map.GetNodeAt(nodeId);
                            node.Type = paintNodeType;
                        }
                        break;
                    }
                case MapEditorPaintType.Segment:
                    {
                        // update node types
                        foreach (var nodeId in nodeIds)
                        {
                            // iterate segments
                            var segments = manager.Map.GetNodeSegments(nodeId);
                            for (Cardinal c = 0; c < Cardinal.MAX_CARDINAL; ++c)
                            {
                                var segment = segments[(int)c];
                                if (segment != null)
                                {
                                    var neighborId = Utilities.PointTowards(nodeId, c);
                                    if (neighborId.InBounds)
                                    {
                                        var neighborPos = manager.GetPosition(neighborId);
                                        if (Vector3.Distance(neighborPos, position) < paintingRadius)
                                        {
                                            // set segment
                                            segment.Type = paintSegmentType;

                                            // set matching segment
                                            var neighborSegments = manager.Map.GetNodeSegments(neighborId);
                                            var neighborSegment = neighborSegments[(int)Utilities.ReflectCardinal(c)];
                                            if (neighborSegment != null)
                                                neighborSegment.Type = paintSegmentType;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    }
            }
        }
    }
}
