using System;
using System.Collections.Generic;
using System.Linq;
using Rails.ScriptableObjects;

namespace Rails
{
    public static class Pathfinding
    {
        private const int _maxPaths = 5;

        #region Public Methods

        /// <summary>
        /// Calculates the best tracks available for the given
        /// player who wishes to move from start to end.
        /// </summary>
        /// <param name="tracks">The tracks on the current map</param>
        /// <param name="player">The player that wants to move</param>
        /// <param name="start">The start point</param>
        /// <param name="end">The target point</param>
        /// <returns>The list of PathDatas representing the shortest tracks</returns>
        public static List<PathData> BestTracks(
            Dictionary<NodeId, int[]> tracks,
            int player, NodeId start, NodeId end
        ) {
            // First determine the least cost track (if it exists)
            var leastCost = DjikstraLeastCostTrack(tracks, player, start, end);
            if(leastCost == null) return null;

            // A copy of the track map - nodes and edges are removed from
            // this map while keeping the original unchanged
            var spurTracks = new Dictionary<NodeId, int[]>(tracks);

            // Create the returned list, starting with the initial least cost path
            var paths = new List<PathData> { leastCost };

            // A list of spur paths to be added to returned list
            var pathSpurs = new List<PathData>();

            for(int k = 0; k < _maxPaths; ++k)
            {
                // Iterate through all nodes in the latest shortest path
                // added to the returned list
                for(int i = 0; i < paths[k - 1].Nodes.Count - 2; ++i)
                {
                    // Select a node to branch off of
                    var spurNode = paths[k - 1].Nodes[i];

                    // Create a sub-track from the root of the start of the
                    // chosen path to spurNode 
                    var rootPath = paths[k - 1].Nodes.GetRange(0, i + 1);

                    // Remove all edges that exist from the path start to the spurNode.
                    // This will ensure that a different path is chosen.
                    foreach(var p in paths.Select(path => path.Nodes))
                    {
                        if(p.GetRange(0, i + 1).SequenceEqual(rootPath))
                        {
                            spurTracks[p[i]][(int)Utilities.CardinalBetween(p[i], p[i+1])] = -1;
                            spurTracks[p[i+1]][(int)Utilities.CardinalBetween(p[i+1], p[i])] = -1;
                        }
                        if(spurTracks[p[i]].All(p => p == -1))
                            spurTracks.Remove(p[i]);
                        if(spurTracks[p[i+1]].All(p => p == -1))
                            spurTracks.Remove(p[i+1]);
                    }

                    rootPath.RemoveAll(p => p != spurNode);

                    // Calculate the shortest path from the spur node to the end node
                    var spurPath = DjikstraLeastCostTrack(spurTracks, player, spurNode, end);
                    // Combine the root path and spur path to create new path
                    var totalPath = CreatePathData(tracks, rootPath, player);
                    totalPath.Combine(spurPath);

                    // If that path doesn't exist in pathSpurs, add it to the pathSpurs list
                    // to be considered as the shortest path
                    if(!pathSpurs.Any(p => p.Nodes.SequenceEqual(totalPath.Nodes)))
                        pathSpurs.Add(totalPath);
                    
                    spurTracks = new Dictionary<NodeId, int[]>(tracks);
                }
                // If there aren't enough shortest paths to continue the
                // loop, break and return what is present
                if(pathSpurs.Count == 0)
                    break;

                // Finally, sort pathSpurs to retrieve the shortest path generated
                // from the iteration of spur nodes. Add that to the returned list
                pathSpurs.Sort((p1, p2) => p1.Cost.CompareTo(p2.Cost));
                paths.Add(pathSpurs.First());
                pathSpurs.RemoveAt(0);
            }

            // Finally, add the least distance path as an added consideration
            // (if it isn't already part of the list)
            var leastDistance = LeastDistanceTrack(tracks, player, start, end);
            if(!paths.Any(p => p.Nodes.SequenceEqual(leastDistance.Nodes)))
                paths.Insert(0, leastDistance);

            return paths;
        }

        public static List<NodeId> LeastWeightPath(
            Dictionary<NodeId, int[]> tracks, 
            MapData mapData, int player,
            NodeId start, NodeId end
        ) {
            return null;
        }

        #endregion

        #region Private Methods        

        // Finds the lowest distance track available for the given player,
        // from a start point to end point.
        private static PathData LeastDistanceTrack(
            Dictionary<NodeId, int[]> tracks, 
            int player, NodeId start, NodeId end
        ) {
            // This method uses the LeastCostTrack available, but first duplicates,
            // and converts all tracks into the given player's tracks, to avoid
            // adding other players' tracks' costs.
            var normTracks = new Dictionary<NodeId, int[]>(tracks);
            foreach(var node in tracks.Keys)
            {
                for(Cardinal c = Cardinal.N; c < Cardinal.MAX_CARDINAL; ++c)
                {
                    if(tracks[node][(int)c] != -1)
                        tracks[node][(int)c] = player;
                }
            }

            return DjikstraLeastCostTrack(normTracks, player, start, end);
        }

        /// <summary>
        /// Finds the lowest cost track available for the given player,
        /// from a start point to end point.
        /// </summary>
        /// <param name="tracks">The track nodes being traversed on</param>
        /// <param name="player">The player enacting the traversal</param>
        /// <param name="start">The start point of the traversal</param>
        /// <param name="end">The target point of the traversal</param>
        /// <returns>The lowest cost track</returns>
        private static PathData DjikstraLeastCostTrack(
            Dictionary<NodeId, int[]> tracks, 
            int player, NodeId start, NodeId end
        ) {
            if(start == end)
                return new PathData (0, 0, new List<NodeId> { start });

            if(!tracks.ContainsKey(start) || !tracks.ContainsKey(end))
                return null;

            // The list of nodes to return from method
            PathData path = null;
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
            bool [] tracksPaid;

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

                tracksPaid = new bool [] { false, false, false, false, false, false };
                tracksPaid[player] = true;

                var currentNode = node.Position;

                while(currentNode != start)
                {
                    int playerTrack = tracks[currentNode][(int)Utilities.CardinalBetween(currentNode, previous[currentNode])];
                    tracksPaid[playerTrack] = true;

                    currentNode = previous[currentNode];
                }

                // If the node's position is the target, or the current node
                // has joined with an already discovered shortest path, a
                // new best path has been found. Traverse the previous collection
                // until start is reached, adding each node to list. Then
                // reverse the list, and the shortest path is returned.
                if(node.Position == end)
                {
                    path = CreatePathData(tracks, previous, player, start, end);
                    break;
                }

                for(Cardinal c = Cardinal.N; c < Cardinal.MAX_CARDINAL; ++c)
                {
                    var newPoint = Utilities.PointTowards(node.Position, c);
                    if(tracks.ContainsKey(newPoint) && tracks[newPoint][(int)c] != -1)
                    {
                        var newCost = node.Weight + 1;
                        int trackOwner = tracks[newPoint][(int)Utilities.CardinalBetween(newPoint, node.Position)];

                        // If the current track is owned by a different player,
                        // one whose track the current player currently is not on
                        // add the Alternative Track Cost to the track's weight.
                        if(!tracksPaid[trackOwner])
                            newCost += Manager.AltTrackCost;

                        // If a shorter path has already been found, continue
                        if(distMap.ContainsKey(newPoint) && distMap[newPoint] <= newCost)
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
            return path;
        }

        /// <summary>
        /// Creates a new PathData using the given
        /// tracks, nodes from start to end and player index
        /// </summary>
        private static PathData CreatePathData(
            Dictionary<NodeId, int[]> tracks,
            List<NodeId> nodes, int player
        ) {
            int distance = nodes.Count;
            int cost = 0;

            bool [] tracksPaid = new bool[6] { false, false, false, false, false, false };
            tracksPaid[player] = true;

            for(int i = 0; i < nodes.Count; ++i)
            {
                cost += 1;
                Cardinal c = Utilities.CardinalBetween(nodes[i], nodes[i + 1]);
                if(!tracksPaid[tracks[nodes[i]][(int)c]])
                {
                    cost += Manager.AltTrackCost;
                    tracksPaid[tracks[nodes[i]][(int)c]] = true;
                }
            }

            return new PathData(distance, cost, nodes);
        }

        /// <summary>
        /// Creates a new PathData using the given tracks,
        /// a map of all tracks' nodes previous nodes for shortest
        /// path, player index and start / end points.
        /// </summary>
        private static PathData CreatePathData (
            Dictionary<NodeId, int[]> tracks,
            Dictionary<NodeId, NodeId> previous,
            int player, NodeId start, NodeId end
         ) {
            var cost = 0;
            var distance = 0;
            var currentTrack = player;

            var nodes = new List<NodeId>();
            var current = end;

            while(current != start)
            {
                nodes.Add(current);
                cost += 1;
                distance += 1;

                Cardinal c = Utilities.CardinalBetween(current, previous[current]);
                if(tracks[current][(int)c] != currentTrack)
                {
                    cost += Manager.AltTrackCost;
                    currentTrack = tracks[current][(int)c];
                }

                current = previous[current];
            }

            nodes.Reverse();
            return new PathData(distance, cost, nodes);
        }
 
        #endregion
    }    

    /// <summary>
    /// Represents information related to a path found by the
    /// Pathfinder class.
    /// </summary>
    public class PathData
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

        public void Combine(PathData data)
        {
            Nodes.AddRange(data.Nodes);
            Cost += data.Cost;
            Distance += data.Distance;
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