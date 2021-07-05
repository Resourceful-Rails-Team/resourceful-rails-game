using UnityEngine;

namespace Rails
{
    public static class Utilities
    {
        /// <summary>
        /// Returns the point represented by a combined point and cardinal.
        /// The point returned is the one moving by Cardinal direction, one space over
        /// from start.
        /// </summary>
        /// <param name="start">The start point</param>
        /// <param name="towards">The cardinal direction moving towards</param>
        /// <returns>The new point being moved towards.</returns>
        public static Vector2Int PointTowards(Vector2Int start, Cardinal towards)
        {
            bool isOdd = start.x % 2 == 1;

            var dir = towards switch
            {
                Cardinal.N => Vector2Int.up,
                Cardinal.NE => isOdd ? new Vector2Int(1, 1) : Vector2Int.right,
                Cardinal.NW => isOdd ? new Vector2Int(-1, 1) : Vector2Int.left, 
                Cardinal.S => Vector2Int.down,
                Cardinal.SW => isOdd ? Vector2Int.left : new Vector2Int(-1, -1),
                _ => isOdd ? Vector2Int.right : new Vector2Int(1, -1),
            };

            return start + dir;
        }

        /// <summary>
        /// Reflects the given Cardinal, returning the
        /// exact opposite direction. (ie. N -> S, NE -> SW).
        /// </summary>
        /// <param name="towards">The Cardinal being reflected</param>
        /// <returns>The opposite cardinal direction to towards</returns> 
        public static Cardinal ReflectCardinal(Cardinal towards) =>
            (Cardinal)Mathf.Repeat((int)Cardinal.SW, (int)towards - 3);

        /// <summary>
        /// Returns the respective color associated with the given NodeType.
        /// </summary>
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

        /// <summary>
        /// Returns the respective color associated with the given NodeSegmentType.
        /// </summary>
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