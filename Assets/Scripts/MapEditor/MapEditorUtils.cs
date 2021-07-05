using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rails
{
    public static class MapEditorUtils
    {
        public static Color GetNodeColor(NodeType nodeType)
        {
            switch (nodeType)
            {
                case NodeType.Clear: return Color.gray;
                case NodeType.Water: return Color.blue;
                case NodeType.Mountain: return Color.black;
                case NodeType.SmallCity: return Color.green;
                case NodeType.MediumCity: return Color.yellow;
                case NodeType.MajorCity: return Color.red;
                default: return Color.white;
            }
        }

        public static Color GetSegmentColor(NodeSegmentType segmentType)
        {
            switch (segmentType)
            {
                case NodeSegmentType.None: return Color.white;
                case NodeSegmentType.River: return Color.blue;
                default: return Color.white;
            }
        }
    }
}
