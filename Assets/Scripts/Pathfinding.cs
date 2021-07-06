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
        public static List<NodeId> LeastCostTrack(
            Dictionary<NodeId, int[]> tracks, 
            int player, NodeId start, NodeId end
        ) {
            if(start == end)
                return new List<NodeId> { start };
            if(!tracks.ContainsKey(start) || !tracks.ContainsKey(end))
                return null;

            // The list of nodes to return from method
            var list = new List<NodeId>(); 

            // The true distances from start to considered point
            // (without A* algorithm consideration)
            var distMap = new Dictionary<NodeId, int>();
            // The list of all nodes visited, connected to their
            // shortest-path neighbors.
            var previous = new Dictionary<NodeId, NodeId>();
            // The next series of nodes to check, sorted by minimum weight
            var queue = new SortedSet<WeightedNode>();
            // All nodes that have been visited - ensures no node
            // is checked twice.
            var visitedNodes = new HashSet<NodeId>();

            // The current considered node
            WeightedNode node;

            // To start things off, add the start node to the queue
            queue.Add(new WeightedNode (start, 0));

            // While there are still nodes to visit,
            // Find the lowest-weight one and determine
            // it's connected nodes.
            while((node = queue.Min) != null)
            { 
                queue.Remove(node); 

                // If the end has been traversed to, break
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

                        // If the node hasn't been visited yet, add it to the queue
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

        public static List<NodeId> LeastWeightPath(
            Dictionary<NodeId, int[]> tracks, int player,
            MapData mapData, NodeId start, NodeId end
        ) {
            return null;
        }
    }    

    /// <summary>
    /// A comparable object representing the position,
    /// and weight to reach a node given a known start
    /// position. Used in pathfinding
    /// </summary>
    class WeightedNode : IComparable<WeightedNode>
    {
        public NodeId Position { get; set; }
        public int Weight { get; set; }
        public WeightedNode(NodeId position, int weight)
        {
            Position = position;
            Weight = weight;
        }
        // Compared by Weight
        public int CompareTo(WeightedNode other) => Weight.CompareTo(other.Weight);
    }
}