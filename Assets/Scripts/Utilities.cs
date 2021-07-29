using System;
using System.Collections.Generic;
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
            if (towards < 0)
                towards += (int)Cardinal.MAX_CARDINAL;

            return towards;
        }

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
                case NodeSegmentType.None: return Color.clear;
                case NodeSegmentType.River: return Color.blue;
                default: return Color.white;
            }
        }

        /// Finds the Cardinal between two adjacent NodeIds.
        /// </summary>
        /// <param name="node1">The starting NodeId</param>
        /// <param name="node2">The target NodeId</param>
        /// <returns>The Cardinal represented from n1 to n2.</returns>
        public static Cardinal CardinalBetween(NodeId node1, NodeId node2)
        {
            var exception = new ArgumentException("Cannot find Cardinal between two non-adjacent NodeIds");

            // Find the direction the NodeIds are moving
            var dir = node2 - node1;
            bool isOdd = node1.X % 2 == 1;

            return (dir.X, dir.Y) switch
            {
                (0, 1) => Cardinal.N,
                (1, 1) => isOdd ? Cardinal.NE : throw exception,
                (1, 0) => isOdd ? Cardinal.SE : Cardinal.NE,
                (1, -1) => isOdd ? throw exception : Cardinal.SE,
                (0, -1) => Cardinal.S,
                (-1, -1) => isOdd ? throw exception : Cardinal.SW,
                (-1, 0) => isOdd ? Cardinal.SW : Cardinal.NW,
                (-1, 1) => isOdd ? Cardinal.NW : throw exception,
                _ => throw exception,
            };
        }

        public static Vector3 GetPosition(NodeId id)
        {
            var w = 2 * Manager.Singleton.WSSize;
            var h = Mathf.Sqrt(3) * Manager.Singleton.WSSize;
            var wspace = 0.75f * w;
            var pos = new Vector3(id.X * wspace, 0, id.Y * h);
            int parity = id.X & 1;
            if (parity == 1)
                pos.z += h / 2;

            return pos;
        }

        public static NodeId GetNodeId(Vector3 position)
        {
            var w = 2 * Manager.Singleton.WSSize;
            var h = Mathf.Sqrt(3) * Manager.Singleton.WSSize;
            var wspace = 0.75f * w;

            int posX = Mathf.RoundToInt(position.x / wspace);
            if (posX % 2 == 1)
                position.z -= h / 2;

            return new NodeId(posX, Mathf.RoundToInt(position.z / h));
        }

        /// <summary>
        /// Returns a collection of NodeIds of nodes that lie within the given circle.
        /// </summary>
        /// <param name="position">Position of the circle</param>
        /// <param name="radius">Radius of circle</param>
        public static List<NodeId> GetNodeIdsByPosition(Vector3 position, float radius)
        {
            List<NodeId> nodeIds = new List<NodeId>();
            var w = 2 * Manager.Singleton.WSSize;
            var h = Mathf.Sqrt(3) * Manager.Singleton.WSSize;
            var wspace = 0.75f * w;

            // Algorithm generates a bounding square
            // It then iterates all nodes within that box
            // Checking if the world space position of that node is within the circle

            // get grid-space node position
            Vector2 centerNodeId = new Vector2(position.x / wspace, position.z / h);
            if ((int)centerNodeId.x % 2 == 1)
                centerNodeId.y -= h / 2;

            // determine grid-space size of radius
            int extents = Mathf.CeilToInt(radius / wspace);

            // generate bounds from center and radius
            // clamp min to be no less than 0
            // clamp max to be no more than Size-1
            int minX = Mathf.Max(0, (int)centerNodeId.x - extents);
            int maxX = Mathf.Min(Manager.Size - 1, Mathf.CeilToInt(centerNodeId.x) + extents);
            int minY = Mathf.Max(0, (int)centerNodeId.y - extents);
            int maxY = Mathf.Min(Manager.Size - 1, Mathf.CeilToInt(centerNodeId.y) + extents);

            // iterate bounds
            for (int x = minX; x <= maxX; ++x)
            {
                for (int y = minY; y <= maxY; ++y)
                {
                    // get position from NodeId
                    var nodeId = new NodeId(x, y);
                    var pos = GetPosition(nodeId);

                    // check if position is within circle
                    if (Vector3.Distance(pos, position) < radius)
                        nodeIds.Add(nodeId);
                }
            }

            return nodeIds;
        }
    }
}