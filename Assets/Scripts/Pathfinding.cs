using System;
using System.Collections.Generic;
using System.Linq;
using Rails.ScriptableObjects;
using UnityEngine;

namespace Rails
{
    public static class Pathfinding
    {
        private const int _maxPaths = 4;

        #region Data Structures

        /// <summary>
        /// Represents information related to a path found by the
        /// Pathfinder class.
        /// </summary>
        public class Route
        {
            public int Cost { get; set; }
            public int Distance => Nodes.Count - 1;
            public List<NodeId> Nodes { get; set; }

            public Route(int cost, List<NodeId> nodes)
            {
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
            public bool[] AltTracksPaid { get; set; }

            public int SpacesLeft { get; set; }
            
            // Compared by Weight
            public int CompareTo(WeightedNode other) => Weight.CompareTo(other.Weight);
        }

        public class PriorityQueue<T> where T: IComparable<T>
        {
            private List<T> items;

            public PriorityQueue() => items = new List<T>();

            public T Peek() => items.FirstOrDefault();
            public T Pop()
            {
                var item = items.FirstOrDefault();

                if(items.Count > 0)
                {
                    items[0] = items.Last();

                    int index = 0;
                    int childIndex = 1;

                    bool traversed = true;

                    while(traversed)
                    { 
                        traversed = false;

                        if(childIndex > items.Count - 1) 
                            break;

                        if(childIndex + 1 < items.Count && items[childIndex].CompareTo(items[childIndex + 1]) > 0)
                            childIndex += 1;

                        if(items[index].CompareTo(items[childIndex]) > 0)
                        {
                            T temp = items[index];
                            items[index] = items[childIndex];
                            items[childIndex] = temp;

                            traversed = true;
                        }

                        index = childIndex;
                        childIndex = 2 * childIndex + 1;
                    } 

                    items.RemoveAt(items.Count - 1);
                }

                return item;
            }

            public void Insert(T item)
            {
                int index = items.Count;
                int parent = Mathf.FloorToInt((index - 1) / 2);

                items.Add(item);

                while(items[index].CompareTo(items[parent]) < 0)
                {
                    var temp = items[parent];
                    items[parent] = items[index];
                    items[index] = temp;

                    index = parent;
                    parent = Mathf.FloorToInt((index - 1) / 2);
                }
            }
        }

        #endregion

        #region Public Methods

        public static Route ShortestBuild(
            Dictionary<NodeId, int[]> tracks, MapData mapData,
            params NodeId[] segments
        ) {
            var path = new List<NodeId>(segments.Length - 1);
            for(int i = 0; i < segments.Length - 1; ++i)
            {
                var route = LeastCostPath(
                    tracks, mapData, segments[i], 
                    segments[i + 1], null, false
                );
                path.AddRange(route.Nodes.Take(route.Nodes.Count - 1));
            }
            path.Add(segments.Last());

            return CreatePathRoute(mapData, path);
        }

        public static Route CheapestBuild(
            Dictionary<NodeId, int[]> tracks, MapData mapData,
            params NodeId[] segments
        ) {
            var path = new List<NodeId>(segments.Length - 1);
            for(int i = 0; i < segments.Length - 1; ++i)
            {
                var route = LeastCostPath(
                    tracks, mapData, segments[i], 
                    segments[i + 1], null, true
                );
                path.AddRange(route.Nodes.Take(route.Nodes.Count - 1));
            }
            path.Add(segments.Last());

            return CreatePathRoute(mapData, path);
        }

        public static Route ShortestMove(
            Dictionary<NodeId, int[]> tracks,
            int player, int speed, params NodeId [] segments
        ) {
            var path = new List<NodeId>(segments.Length - 1);
            for(int i = 0; i < segments.Length - 1; ++i)
            {
                var route = LeastCostTrack(
                    tracks, player, speed, 
                    segments[i], segments[i + 1], false
                );
                path.AddRange(route.Nodes.Take(route.Nodes.Count - 1));
            }
            path.Add(segments.Last());

            return CreateTrackRoute(tracks, player, speed, path); 
        }

        public static Route CheapestMove(
            Dictionary<NodeId, int[]> tracks,
            int player, int speed, params NodeId [] segments
        ) {
            var path = new List<NodeId>(segments.Length - 1);
            for(int i = 0; i < segments.Length - 1; ++i)
            {
                var route = LeastCostTrack(
                    tracks, player, speed, 
                    segments[i], segments[i + 1], true
                );
                path.AddRange(route.Nodes.Take(route.Nodes.Count - 1));
            }
            path.Add(segments.Last());

            return CreateTrackRoute(tracks, player, speed, path); 
        }

        #endregion

        #region Private Methods         

        // Finds the lowest distance track available for the given player,
        // from a start point to end point.
        private static Route LeastDistanceTrack(
            Dictionary<NodeId, int[]> tracks, 
            int player, int speed, 
            NodeId start, NodeId end
        ) {
            return LeastCostTrack(tracks, player, speed, start, end, false);
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
        public static Route LeastCostTrack(
            Dictionary<NodeId, int[]> tracks, 
            int player, int speed, 
            NodeId start, NodeId end,
            bool addAltTrackCost
        ) {
            if(start == end || !tracks.ContainsKey(start) || !tracks.ContainsKey(end))
                return null;

            // The list of nodes to return from method
            Route path = null;
            // The true distances from start to considered point
            // (without A* algorithm consideration)
            var distMap = new Dictionary<NodeId, int>();
            // The list of all nodes visited, connected to their
            // shortest-path neighbors.
            var previous = new Dictionary<NodeId, NodeId>();
            // The next series of nodes to check, sorted by minimum weight
            var queue = new PriorityQueue<WeightedNode>();

            // The current considered node
            WeightedNode node;

            distMap.Add(start, 0);

            var startNode = new WeightedNode 
            {
                Position = start,
                Weight = 0,
                SpacesLeft = speed + 1,
                AltTracksPaid = new bool[6],
            };
            startNode.AltTracksPaid[player] = true;
            // To start things off, add the start node to the queue
            queue.Insert(startNode);

            // While there are still nodes to visit,
            // Find the lowest-weight one and determine
            // it's connected nodes.
            while((node = queue.Pop()) != null)
            { 
                // If the node's position is the target, build the path and assign
                // it to the returned PathData
                if(node.Position == end)
                {
                    path = CreateTrackRoute(tracks, previous, player, speed, start, end);
                    break;
                }

                if(addAltTrackCost && node.SpacesLeft == 0)
                {
                    node.SpacesLeft = speed;
                    for(int i = 0; i < 6; ++i)
                        node.AltTracksPaid[i] = false;
                    node.AltTracksPaid[player] = true;
                }

                for(Cardinal c = Cardinal.N; c < Cardinal.MAX_CARDINAL; ++c)
                {
                    var newPoint = Utilities.PointTowards(node.Position, c);
                    if(tracks[node.Position][(int)c] != -1 && tracks.ContainsKey(newPoint))
                    {
                        var newNode = new WeightedNode
                        {
                            Position = newPoint, 
                            SpacesLeft = node.SpacesLeft - 1,
                        };
                        newNode.AltTracksPaid = node.AltTracksPaid.ToArray();

                        var newCost = distMap[node.Position] + 1;
                        int trackOwner = tracks[node.Position][(int)Utilities.CardinalBetween(node.Position, newPoint)];

                        // If the current track is owned by a different player,
                        // one whose track the current player currently is not on
                        // add the Alternative Track Cost to the track's weight.
                        if(addAltTrackCost && !newNode.AltTracksPaid[trackOwner])
                        {
                            newCost += 1000;
                            newNode.AltTracksPaid[trackOwner] = true;
                        }

                        // If a shorter path has already been found, continue
                        if(distMap.TryGetValue(newPoint, out int currentCost) && currentCost <= newCost)
                            continue;

                        distMap[newPoint] = newCost;
                        previous[newPoint] = node.Position;

                        newNode.Weight = newCost + Mathf.RoundToInt(NodeId.Distance(newNode.Position, end));
                        queue.Insert(newNode);
                    }
                } 
            } 
            return path;
        }

        /// <summary>
        /// Finds the lowest cost path available from one point to another,
        /// if it exists
        /// </summary>
        /// <param name="tracks">The track nodes being traversed on</param>
        /// <param name="map">The map being used in the algorithm</param>
        /// <param name="start">The start point of the traversal</param>
        /// <param name="end">The target point of the traversal</param>
        /// <returns>The lowest cost track</returns>
        public static Route LeastCostPath(
            Dictionary<NodeId, int[]> tracks, 
            MapData map, NodeId start, NodeId end,
            IEnumerable<Tuple<NodeId, Cardinal>> removedEdges,
            bool addWeight
        ) {
            if(start == end)
                return null;
            if(tracks.ContainsKey(start) && tracks[start].All(p => p != -1))
                return null;
            if(tracks.ContainsKey(end) && tracks[end].All(p => p != -1))
                return null;

            // The list of nodes to return from method
            Route path = null;
            // The true distances from start to considered point
            // (without A* algorithm consideration)
            var distMap = new Dictionary<NodeId, int>();
            // The list of all nodes visited, connected to their
            // shortest-path neighbors.
            var previous = new Dictionary<NodeId, NodeId>();
            // The next series of nodes to check, sorted by minimum weight
            var queue = new PriorityQueue<WeightedNode>();

            // The current considered node
            WeightedNode node;

            distMap[start] = 0;
            var startNode = new WeightedNode { Position = start, Weight = 0 };

            // To start things off, add the start node to the queue
            queue.Insert(startNode);

            // While there are still nodes to visit,
            // Find the lowest-weight one and determine
            // it's connected nodes.
            while((node = queue.Pop()) != null)
            { 
                // If the node's position is the target, build the path and assign
                // it to the returned PathData
                if(node.Position == end)
                {
                    path = CreatePathRoute(map, previous, start, end);
                    break;
                }

                for(Cardinal c = Cardinal.N; c < Cardinal.MAX_CARDINAL; ++c)
                {
                    if(tracks.ContainsKey(node.Position) && tracks[node.Position][(int)c] != -1)
                        continue;

                    var newPoint = Utilities.PointTowards(node.Position, c);

                    if(removedEdges?.Contains(Tuple.Create(node.Position, c)) ?? false)
                        continue;
                    if(newPoint.X < 0 || newPoint.Y < 0 || newPoint.X >= Manager.Size || newPoint.Y >= Manager.Size)
                        continue;

                    var newNode = new WeightedNode { Position = newPoint };

                    var newCost = distMap[node.Position] + 1;
                    if(map.Nodes[newNode.Position.X * Manager.Size + newNode.Position.Y].Type == NodeType.Mountain && addWeight)
                        newCost += 1;


                    // If a shorter path has already been found, continue
                    if(distMap.TryGetValue(newPoint, out int currentCost) && currentCost <= newCost)
                        continue;

                    distMap[newPoint] = newCost;
                    previous[newPoint] = node.Position;

                    newNode.Weight = newCost + Mathf.RoundToInt(NodeId.Distance(newNode.Position, end));
                    queue.Insert(newNode);
                } 
            } 
            return path;
        }

        private static Route LeastDistancePath(
            Dictionary<NodeId, int[]> tracks,
            MapData map, NodeId start, NodeId end
        ) => LeastCostPath(tracks, map, start, end, null, false);

        /// <summary>
        /// Creates a new PathData using the given
        /// tracks, nodes from start to end and player index
        /// </summary>
        private static Route CreateTrackRoute(
            Dictionary<NodeId, int[]> tracks,
            int player, int speed,
            params List<NodeId>[] paths
        ) {
            int spacesLeft = speed + 1;
            int cost = 0;
            var nodes = new List<NodeId>();

            bool [] tracksPaid = new bool[6] { false, false, false, false, false, false };
            tracksPaid[player] = true; 

            for(int p = 0; p < paths.Length; ++p)
            {
                for(int i = 0; i < paths[p].Count; ++i)
                {
                    nodes.Add(paths[p][i]); 

                    NodeId node1; NodeId ? node2 = null;
                    node1 = paths[p][i];

                    if(i < paths[p].Count - 1)
                        node2 = paths[p][i+1];
                    else if(p < paths.Length - 1)
                        node2 = paths[p+1][0];

                    if(node2.HasValue)
                    {
                        Cardinal c = Utilities.CardinalBetween(node1, node2.Value);
                        if(!tracksPaid[tracks[node1][(int)c]])
                        {
                            cost += Manager.AltTrackCost;
                            tracksPaid[tracks[node1][(int)c]] = true;
                        }
                    }

                    spacesLeft -= 1;

                    if(spacesLeft == 0)
                    {
                        spacesLeft = speed;
                        for(int t = 0; t < 6; ++t)
                            tracksPaid[t] = false;

                        tracksPaid[player] = true;
                    }
                }
            }

            return new Route(cost, nodes);
        }

        /// <summary>
        /// Creates a new PathData using the given tracks,
        /// a map of all tracks' nodes previous nodes for shortest
        /// path, player index and start / end points.
        /// </summary>
        private static Route CreateTrackRoute (
            Dictionary<NodeId, int[]> tracks,
            Dictionary<NodeId, NodeId> previous,
            int player, int speed, NodeId start, NodeId end
         ) {
            int spacesLeft = speed + 1;
            var cost = 0;
            bool [] tracksPaid = new bool[6] { false, false, false, false, false, false };
            tracksPaid[player] = true;

            var nodes = new List<NodeId>();
            var current = end;

            do
            {
                nodes.Add(current);

                Cardinal c = Utilities.CardinalBetween(current, previous[current]);
                if(!tracksPaid[tracks[current][(int)c]])
                {
                    cost += Manager.AltTrackCost;
                    tracksPaid[tracks[current][(int)c]] = true;
                }

                current = previous[current];

                spacesLeft -= 1;

                if(spacesLeft == 0)
                {
                    spacesLeft = speed;
                    for(int t = 0; t < 6; ++t)
                        tracksPaid[t] = false;

                    tracksPaid[player] = true;
                }
            } 
            while(current != start);

            nodes.Reverse();
            return new Route(cost, nodes);
        }

        private static Route CreatePathRoute(
            MapData map,
            Dictionary<NodeId, NodeId> previous,
            NodeId start, NodeId end
        ) {
            var cost = 0;
            var nodes = new List<NodeId>();
            var current = end;

            do
            {
                nodes.Add(current);

                current = previous[current];

                if(current != start)
                    cost += map.Nodes[current.X * Manager.Size + current.Y].Type == NodeType.Clear ?
                        1 : 2;
            } 
            while(current != start);

            nodes.Reverse();
            Debug.Log(nodes.Count);
            return new Route(cost, nodes);
        }

        private static Route CreatePathRoute(
            MapData map,
            params List<NodeId>[] paths
        ) {
            int cost = 0;
            List<NodeId> path = new List<NodeId>();

            for(int p = 0; p < paths.Length; ++p)
            {
                for(int i = 0; i < paths[p].Count; ++i)
                {
                    path.Add(paths[p][i]); 

                    NodeId node1; NodeId ? node2 = null;
                    node1 = paths[p][i];

                    if(i < paths[p].Count - 1)
                        node2 = paths[p][i+1];
                    else if(p < paths.Length - 1)
                        node2 = paths[p+1][0];

                    if(node2.HasValue)
                        cost += map.Nodes[node2.Value.X * Manager.Size + node2.Value.Y].Type == NodeType.Clear ? 1 : 2;
                }
            }
 
            return new Route(cost, path);
        }
        #endregion
    }    
}