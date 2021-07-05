using System;
using System.Collections.Generic;

using UnityEngine;

namespace Rails
{
    public static class Pathfinding
    {
        /// <summary>
        /// Finds the Least Cost Track, based on given track nodes, start
        /// and end position.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>    
        public static List<Vector2Int> LeastCostTrack(
            Dictionary<Vector2Int, int[]> tracks, 
            int player, Vector2Int start, Vector2Int end
        ) {
            if(start == end)
                return new List<Vector2Int> { start };
            if(!tracks.ContainsKey(start) || !tracks.ContainsKey(end))
                return null;

            // The true distances from start to considered point
            // (without A* algorithm consideration)
            var distMap = new Dictionary<Vector2Int, int>();
            // The list of nodes to return from method
            var list = new List<Vector2Int>(); 
            // The list of all nodes visited, connected to their
            // shortest-path neighbors.
            var previous = new Dictionary<Vector2Int, Vector2Int>();
            // The next series of nodes to check
            var queue = new SortedSet<WeightedNode>();
            var visitedNodes = new HashSet<Vector2Int>();
            // The current considered node
            WeightedNode node;

            queue.Add(new WeightedNode (start, 0));

            // While there are still nodes to visit,
            // Find the lowest-weight one and determine
            // it's connected nodes.
            while((node = queue.Min) != null)
            { 
                queue.Remove(node); 

                if(node.Position == end)
                    break;    

                for(Cardinal c = Cardinal.N; c < Cardinal.MAX_CARDINAL; ++c)
                {
                    var newPoint = Utilities.PointTowards(node.Position, c);
                    if(tracks.ContainsKey(newPoint))
                    {
                        // If a shorter path has already been found, continue
                        if(distMap.ContainsKey(newPoint) && distMap[newPoint] <= node.Weight + 1)
                            continue;

                        distMap.Add(newPoint, node.Weight + 1);
                        previous[newPoint] = node.Position;

                        if(!visitedNodes.Contains(newPoint))
                        {
                            queue.Add(new WeightedNode(newPoint, node.Weight + 1));
                            visitedNodes.Add(newPoint);
                        }
                    }
                } 
            }

            // If the node's position is the target, a path
            // was successfully found. Traverse the previous collection
            // until start is reached, adding each node to list. Then
            // reverse the list, and the shortest path is returned.
            if(node.Position == end)
            {
                var pos = node.Position;
                while(pos != start)
                {
                    list.Add(pos);
                    pos = previous[pos];
                }

                list.Reverse();
                return list;
            }
            else return null;
        }

        public static List<Vector2Int> LeastWeightPath(
            Dictionary<Vector2Int, int[]> tracks,
            MapData mapData, Vector2Int start, Vector2Int end
        ) {
            if(start == end) return new List<Vector2Int> { start };

            // Return null if start or end is outside the map bounds
            var bounds = new BoundsInt(0, 0, -10, Manager.Size, Manager.Size, 20);
            if(!bounds.Contains((Vector3Int)start) || !bounds.Contains((Vector3Int)end))
                return null;            

            return null;
        }
    }    

    /// <summary>
    /// A comparable object representing the position,
    /// and weight to reach a node given a known start
    /// position. Used in pathfinding
    /// </summary>
    public class WeightedNode : IComparable<WeightedNode>
    {
        public Vector2Int Position { get; set; }
        public int Weight { get; set; }
        public WeightedNode(Vector2Int position, int weight)
        {
            Position = position;
            Weight = weight;
        }
        // Compared by Weight
        public int CompareTo(WeightedNode other) => Weight.CompareTo(other.Weight);
    }
}