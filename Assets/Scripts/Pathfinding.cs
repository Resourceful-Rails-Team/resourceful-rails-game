using System;
using System.Collections.Generic;
using System.Linq;
using Rails;
using UnityEngine;

namespace Rails
{
    public static class Pathfinding
    {
        private const int _maxPaths = 5;

        /// <summary>
        /// Finds the shortest, least-cost tracks available given a player.
        /// </summary>
        /// <param name="tracks">The tracks being traversed on</param>
        /// <param name="player">The player enacting the traversal</param>
        /// <param name="start">The start point of the traversal</param>
        /// <param name="end">The target point of the traversal</param>
        /// <returns></returns>
        public static List<PathData> BestTracks(
            Dictionary<NodeId, int[]> tracks, 
            int player, NodeId start, NodeId end
        ) {
            if(start == end)
                return new List<PathData> { 
                    new PathData (0, 0, new List<NodeId> { start } )
                };

            if(!tracks.ContainsKey(start) || !tracks.ContainsKey(end))
                return null;

            // The list of nodes to return from method
            List<PathData> paths = null;
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

                // If the node's position is the target, a path
                // was successfully found. Traverse the previous collection
                // until start is reached, adding each node to list. Then
                // reverse the list, and the shortest path is returned.
                if(node.Position == end)
                {
                    var newPath = CreateTrackPath(tracks, previous, player, start, end);
                    if(!paths.Any(p => p.Cost == newPath.Cost && p.Distance == newPath.Distance))
                    {
                        paths.Add(newPath);
                        if(paths.Count == _maxPaths)
                            break;
                    }
                }

                for(Cardinal c = Cardinal.N; c < Cardinal.MAX_CARDINAL; ++c)
                {
                    var newPoint = Utilities.PointTowards(node.Position, c);
                    if(tracks.ContainsKey(newPoint) && tracks[newPoint][(int)c] != -1)
                    {
                        var newCost = node.Weight + 1;

                        // If the current track is of a different 
                        if(tracks[newPoint][(int)c] != player && 
                            tracks[node.Position][(int)c] != tracks[previous[newPoint]][(int)Utilities.CardinalBetween(previous[newPoint], newPoint)]
                        ) {
                            newCost += Manager.AltTrackCost;
                        }

                        // If a shorter path has already been found, continue
                        if(distMap.ContainsKey(newPoint) && distMap[newPoint] <= node.Weight + 1)
                            continue;

                        distMap[newPoint] = newCost;
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
            return paths;
        }

        private static PathData CreateTrackPath (
            Dictionary<NodeId, int[]> tracks,
            Dictionary<NodeId, NodeId> previous,
            int player, NodeId start, NodeId end
         ) {
            var cost = 0;
            var distance = 0;

            var nodes = new List<NodeId>();
            var current = end;

            while(current != start)
            {
                nodes.Add(current);
                current = previous[current];
            }

            nodes.Reverse();
            return new PathData(distance, cost, nodes);
        }

        public static List<NodeId> LeastWeightPath(
            Dictionary<NodeId, int[]> tracks, int player,
            NodeId start, NodeId end
        ) {
            return null;
        }
    }    

    /// <summary>
    /// Represents information related to a path found by the
    /// Pathfinder class.
    /// </summary>
    public struct PathData
    {
        public int Distance { get; set; }
        public int Cost { get; set; }
        public List<NodeId> Nodes { get; set; }

        public PathData(int distance, int cost, List<NodeId> nodes)
        {
            Distance = distance;
            Cost = cost;
            Nodes = nodes;
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