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
        public static NodeId PointTowards(NodeId start, Cardinal towards)
        {
            bool isOdd = start.X % 2 == 1;

            var dir = towards switch
            {
                Cardinal.N => Vector2Int.up,
                Cardinal.NE => isOdd ? new Vector2Int(1, 1) : Vector2Int.right,
                Cardinal.NW => isOdd ? new Vector2Int(-1, 1) : Vector2Int.left, 
                Cardinal.S => Vector2Int.down,
                Cardinal.SW => isOdd ? Vector2Int.left : new Vector2Int(-1, -1),
                _ => isOdd ? Vector2Int.right : new Vector2Int(1, -1),
            };

            return new NodeId(start.X + dir.x, start.Y + dir.y);
        }

        /// <summary>
        /// Reflects the given Cardinal, returning the
        /// exact opposite direction. (ie. N -> S, NE -> SW).
        /// </summary>
        /// <param name="towards">The Cardinal being reflected</param>
        /// <returns>The opposite cardinal direction to towards</returns> 
        public static Cardinal ReflectCardinal(Cardinal towards)
        {
            towards -= 3;
            if(towards < 0) 
                towards += (int)Cardinal.MAX_CARDINAL;

            return towards;
        }
    }
}