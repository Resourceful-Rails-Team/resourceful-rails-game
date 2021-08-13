using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Rails.Collections;
using Rails.Data;
using Rails.ScriptableObjects;
using UnityEngine;

namespace Rails.Systems
{
    // These pathfinding methods rely on Edsger W. Dijkstra's algorithm
    // found on Wikipedia (https://en.wikipedia.org/wiki/Dijkstra's_algorithm)
    // combined with Peter Hart, Nils Nilsson and Bertram Raphael's A*
    // heuristic algorithm found on Wikipedia 
    // (https://en.wikipedia.org/wiki/A*_search_algorithm)

 
    /// <summary>
    /// A comparable object representing the position,
    /// and weight to reach a node given a known start
    /// position. Used in pathfinding
    /// </summary>
    class WeightedNode : IComparable<WeightedNode>
    {
        public NodeId Position { get; set; }
        public int Weight { get; set; }
        
        // A running tally of what tracks have been paid for at
        // the current position
        public bool[] AltTracksPaid { get; set; }
        
        // How many spaces the player has left at this WeightedNode, from
        // start.
        public int SpacesLeft { get; set; }
        
        // Represents the number of major Citys left the player
        // can connect with at this instance.
        public int MajorCitiesLeft { get; set; }
        
        // Compared by Weight
        public int CompareTo(WeightedNode other) => Weight.CompareTo(other.Weight);
    } 
    
    public static class Pathfinding
    {
        private static GameRules _rules;
        private static TrackGraph<int> _tracks;
        private static MapData _mapData;

        #region Public Methods
        
        /// <summary>
        /// Initializes the Pathfinder, supplying it with the unique game session data.
        /// </summary>
        /// <param name="rules">The current rules for the game session.</param>
        /// <param name="tracks">The game's tracks.</param>
        /// <param name="mapData">The MapData of the board.</param>
        public static void Initialize(GameRules rules, TrackGraph<int> tracks, MapData mapData)
        {
            _rules = rules;
            _tracks = tracks;
            _mapData = mapData;
        }

        public static List<Route> ShortestBuilds(List<List<NodeId>> segmentGroups) {
            var routes = new List<Route>();
            int majorCitiesLeft = _rules.MajorCityBuildsPerTurn;

            foreach(var segments in segmentGroups)
            { 
                var newRoute = FindPathBetweenPoints(_tracks.Clone(), _tracks.Clone(), ref majorCitiesLeft, false, segments);
                routes.Add(newRoute);
            }

            return routes;
        }

        public static List<Route> CheapestBuilds(List<List<NodeId>> paths)
        { 
            var routes = new List<Route>();
            int majorCount = _rules.MajorCityBuildsPerTurn;

            foreach(var path in paths)
            { 
                var newRoute = FindPathBetweenPoints(_tracks.Clone(), _tracks.Clone(), ref majorCount, true, path);
                routes.Add(newRoute);
            }

            return routes;
        }
        public static Route ShortestMove(
            int player, 
            int speed, 
            params NodeId[] segments
        ) => FindTrackBetweenPoints(player, speed, false, segments);

        /// <summary>
        /// Finds the least-cost traversal on a track,
        /// given a `player`, the player's train's `speed`, and
        /// a series of intermediate segments.
        /// </summary>
        /// <param name="tracks">The map's current player tracks</param>
        /// <param name="player">The index of the player performing the traversal</param>
        /// <param name="speed">The speed of the player's train</param>
        /// <param name="segments">The NodeIds the route must pass through</param>
        /// <returns>
        /// The shortest-distance Route that connects all segments.
        /// Returns the furthest path between segments possible
        /// if a segment cannot be reached. 
        /// </returns>
        public static Route CheapestMove(
            int player, 
            int speed, 
            params NodeId[] segments
        ) => FindTrackBetweenPoints(player, speed, true, segments);
        
        #endregion

        #region Private Methods 

        /// <summary>
        /// Finds the best build path, either shortest or cheapest.
        /// </summary>
        private static Route FindPathBetweenPoints(
            TrackGraph<int> pathfindingTracks,
            TrackGraph<int> pathbuildingTracks,
            ref int majorCitiesLeft,
            bool addWeight, 
            List<NodeId> segments
        ) {
            var path = new List<NodeId>();

            // Create a copy of majorCount, to eventually pass into
            // the build path method
            int majorCountDuplicate = majorCitiesLeft;
            
            // If no segments were given, return an empty Route
            if (segments.Count == 0) 
                return new Route(0, path);

            var routes = new List<List<NodeId>>();

            // For each segment, find the least-cost path between it and the next segment.
            for(int i = 0; i < segments.Count - 1; ++i)
            {
                var route = LeastCostPath(
                    ref majorCountDuplicate,
                    pathfindingTracks, 
                    segments[i],
                    segments[i + 1], 
                    addWeight
                );
                if(route == null) break;

                routes.Add(route);
                
                // Add the new path to newTracks
                // Using the pathfinding tracks
                for(int j = 0; j < route.Count - 1; ++j)
                    pathfindingTracks[route[j], route[j+1]] = Manager.Singleton.CurrentPlayer;

                path.AddRange(route.Take(route.Count - 1));
                if (i == segments.Count - 2) path.Add(segments.Last());
            }
            
            // Use the pathbuilding tracks, which will not have the pathfindingTracks
            // on them, to build the Route, calculating the cost
            return CreatePathRoute(pathbuildingTracks, ref majorCitiesLeft, path);
        }

        /// <summary>
        /// Finds the best move path, either shortest or cheapest.
        /// </summary>
        private static Route FindTrackBetweenPoints(
            int player, 
            int speed, 
            bool addAltTrackCost, 
            params NodeId [] segments
        ) {
            var path = new List<NodeId>();

            // If no segments were given, return an empty Route
            if (segments.Length == 0) 
                return new Route(0, path);
            if (segments.Any(s => !_tracks.ContainsVertex(s)))
                return new Route(0, path);

            path.Add(segments[0]);
    
            // For each segment, find the least-distance path between it and the next segment.
            for(int i = 0; i < segments.Length - 1; ++i)
            {
                var route = LeastCostTrack(
                    _tracks, player, speed, 
                    segments[i], segments[i + 1], addAltTrackCost
                );
                if(route == null) break;

                // Add the new path to newTracks
                path.AddRange(route.Nodes);
            }

            return CreateTrackRoute(_tracks, player, speed, path); 

        }

        /// <summary>
        /// Finds the lowest cost track available for the given player,
        /// from a start point to end point.
        /// </summary>
        private static Route LeastCostTrack(
            TrackGraph<int> tracks, 
            int player, 
            int speed, 
            NodeId start, 
            NodeId end,
            bool addAltTrackCost
        ) {
            if(start == end || !tracks.ContainsVertex(start) || !tracks.ContainsVertex(end))
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
                    path = CreateTrackRoute(_rules, tracks, previous, player, speed, start, end);
                    break;
                }
                
                // If other players' track costs are being considered,
                // and the player has run out of spaces on this turn, reset
                // all AltTracksPaid elements to false (except the traversing player). 
                if(addAltTrackCost && node.SpacesLeft == 0)
                {
                    node.SpacesLeft = speed;
                    for(int i = 0; i < 6; ++i)
                        node.AltTracksPaid[i] = false;
                    node.AltTracksPaid[player] = true;
                }
                
                // With the current NodeId, cycle through all Cardinal directions,
                // determining cost to traverse that direction.
                for(Cardinal c = Cardinal.N; c < Cardinal.MAX_CARDINAL; ++c)
                {
                    var newPoint = Utilities.PointTowards(node.Position, c);
                    if(tracks[node.Position, c] == Manager.MajorCityIndex || tracks[node.Position, c] != -1)
                    {
                        var newNode = new WeightedNode
                        {
                            Position = newPoint, 
                            SpacesLeft = node.SpacesLeft - 1,
                        };
                        newNode.AltTracksPaid = node.AltTracksPaid.ToArray();

                        var newCost = distMap[node.Position] + 1;

                        // Retrieve the track owner of the edge being considered
                        if (tracks[node.Position, c] != Manager.MajorCityIndex)
                        {
                            int trackOwner = tracks[node.Position, c];

                            // If the current track is owned by a different player,
                            // one whose track the current player currently is not on
                            // add the Alternative Track Cost to the track's weight.
                            if (addAltTrackCost && !newNode.AltTracksPaid[trackOwner])
                            {
                                newCost += 1000;
                                newNode.AltTracksPaid[trackOwner] = true;
                            }
                        }
                        else newCost += 1; 

                        // If a shorter path has already been found, continue
                        if(distMap.TryGetValue(newPoint, out int currentCost) && currentCost <= newCost)
                            continue;
                        
                        // Add the true new cost to distMap
                        distMap[newPoint] = newCost;
                        // Establish the previous node for the new node, to connect
                        // the path upon completion
                        previous[newPoint] = node.Position;
                        
                        // Add the new cost, with the A* heuristic, to the node's weight
                        newNode.Weight = newCost + Mathf.RoundToInt(NodeId.Distance(newNode.Position, end));
                        queue.Insert(newNode);
                    }
                } 
            } 
            return path;
        }
           
        /// <summary>
        /// Finds the lowest cost path available from a start point to end point.
        /// </summary>
        private static List<NodeId> LeastCostPath(
            ref int majorCitiesLeft,
            TrackGraph<int> tracks, 
            NodeId start, 
            NodeId end,            
            bool addWeight
        ) {
            if(start == end)
                return null;
            if(tracks.TryGetEdges(start, out var startCardinals) && startCardinals.All(p => p != -1 && p != Manager.MajorCityIndex))
                return null;
            if(tracks.TryGetEdges(end, out var endCardinals) && endCardinals.All(p => p != -1 && p != Manager.MajorCityIndex))
                return null;

            // The list of nodes to return from method
            List<NodeId> path = null;
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
            var startNode = new WeightedNode { Position = start, Weight = 0, MajorCitiesLeft = majorCitiesLeft };

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
                    path = ConstructPath(ref majorCitiesLeft, previous, start, end);
                    break;
                }

                // With the current NodeId, cycle through all Cardinal directions,
                // determining cost to traverse that direction.
                for (Cardinal c = Cardinal.N; c < Cardinal.MAX_CARDINAL; ++c)
                {
                    int citiesLeft = node.MajorCitiesLeft;
                    var newPoint = Utilities.PointTowards(node.Position, c);
                    bool edgeFound;

                    // If there is a track already at the considered edge, continue
                    if ((edgeFound = tracks.TryGetEdgeValue(node.Position, c, out var edge)) 
                        && edge != Manager.Singleton.CurrentPlayer
                        && edge != Manager.MajorCityIndex
                        && edge != -1)
                        continue;

                    if (!newPoint.InBounds || _mapData.Nodes[newPoint.GetSingleId()].Type == NodeType.Water)
                        continue;

                    var newCost = distMap[node.Position];

                    if (addWeight)
                    {
                        if (!edgeFound || edge != Manager.Singleton.CurrentPlayer)
                        {
                            // Retrieve both NodeTypes for the two considered Nodes
                            var nodeType1 = _mapData.Nodes[node.Position.GetSingleId()].Type;
                            var nodeType2 = _mapData.Nodes[newPoint.GetSingleId()].Type;

                            // If the one of the considered nodes is a major city, while the other one isn't
                            if ((nodeType1 ^ nodeType2) == NodeType.MajorCity)
                            {
                                // If the player can still build from major cities this turn, add the cost
                                // of the current node instead of the major city node. Subtract from major city points left.
                                if (citiesLeft > 0)
                                {
                                    newCost += _rules.GetNodeCost(nodeType1 == NodeType.MajorCity ? nodeType2 : nodeType1);
                                    citiesLeft -= 1;
                                }
                                // Otherwise, charge the cost of inserting into a new major city
                                else
                                    newCost += _rules.GetNodeCost(NodeType.MajorCity);
                            }
                            // If both nodes are not a major city, add the NodeType cost to the new point.
                            else if ((nodeType1 & nodeType2) != NodeType.MajorCity)
                                newCost += _rules.GetNodeCost(_mapData.Nodes[newPoint.GetSingleId()].Type);

                            // (A cost is not added if both NodeTypes are MajorCity)

                            // Add the river cost if the node is over a river
                            if (_mapData.Segments[(newPoint.GetSingleId() * 6) + (int)c].Type == NodeSegmentType.River)
                                newCost += _rules.RiverCrossCost;
                        }
                    }
                    else newCost += 1;

                    // If a shorter path has already been found, continue
                    if (distMap.TryGetValue(newPoint, out int currentCost) && currentCost <= newCost)
                        continue;

                    // Add the true new cost to distMap
                    distMap[newPoint] = newCost;
                    // Establish the previous node for the new node, to connect
                    // the path upon completion
                    previous[newPoint] = node.Position;

                    // Add the new cost, to the node's weight
                    var newNode = new WeightedNode
                    {
                        Position = newPoint,
                        Weight = newCost,
                        MajorCitiesLeft = citiesLeft // Add the calculated Major Cities left,
                                                     // should the pathfinder choose that route.
                    };
                    queue.Insert(newNode);
                }
            } 
            return path;
        }
        
        /// <summary>
        /// Creates a new Route using the given tracks,
        /// a map of all tracks' nodes previous nodes for shortest
        /// path.
        /// </summary>
        private static Route CreateTrackRoute (
            GameRules rules,
            TrackGraph<int> tracks,
            Dictionary<NodeId, NodeId> previous,
            int player, 
            int speed, 
            NodeId start, 
            NodeId end
         ) {
            int spacesLeft = speed + 1;
            var cost = 0;
            bool [] tracksPaid = new bool[6] { false, false, false, false, false, false };
            tracksPaid[player] = true;

            var nodes = new List<NodeId>();
            var current = end;

            // While the start node has not been reached, traverse
            // down the previous map.
            do
            {
                nodes.Add(current);

                // If the track has yet to be paid for this turn
                // add the cost.
                if(tracks[current, previous[current]] != Manager.MajorCityIndex && !tracksPaid[tracks[current, previous[current]]])
                {
                    cost += rules.AltTrackCost;
                    tracksPaid[tracks[current, previous[current]]] = true;
                }

                current = previous[current];

                spacesLeft -= 1;

                // If the player has run out of spaces this turn,
                // reset tracksPaid
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

        /// <summary>
        /// Creates a new Route using the given
        /// tracks, nodes from start to end and player index
        /// </summary>
        private static Route CreateTrackRoute(
            TrackGraph<int> tracks,
            int player, 
            int speed,
            List<NodeId> path
        ) {
            int spacesLeft = speed + 1;
            int cost = 0;

            bool [] tracksPaid = new bool[6] { false, false, false, false, false, false };
            tracksPaid[player] = true; 
            
            // Traverse the path, adding the cost of each edge while doing so
            for(int i = 0; i < path.Count - 1; ++i)
            {
                // If the track has yet to be paid for this turn
                // add the cost.
                if(tracks[path[i], path[i+1]] != Manager.MajorCityIndex && !tracksPaid[tracks[path[i], path[i+1]]])
                {
                    cost += _rules.AltTrackCost;
                    tracksPaid[tracks[path[i], path[i+1]]] = true;
                }
            
                spacesLeft -= 1;
                
                // If the player has run out of spaces this turn,
                // reset tracksPaid
                if(spacesLeft == 0)
                {
                    spacesLeft = speed;

                    for(int t = 0; t < 6; ++t)
                        tracksPaid[t] = false;

                    tracksPaid[player] = true;
                }
            }

            return new Route(cost, path);
        }

        /// <summary>
        /// Creates a new Route using the MapData and a map of all tracks' 
        /// nodes previous nodes for shortest path.
        /// </summary>
        private static List<NodeId> ConstructPath(
            ref int majorCitiesLeft,
            Dictionary<NodeId, NodeId> previous,
            NodeId start, 
            NodeId end
        ) {
            var nodes = new List<NodeId>();
            var current = end;

            // While the start node has not been reached, traverse
            // down the previous map.
            while(current != start)
            {
                nodes.Add(current);

                var nodeType1 = _mapData.Nodes[current.GetSingleId()].Type;
                var nodeType2 = _mapData.Nodes[previous[current].GetSingleId()].Type;

                if ((nodeType1 ^ nodeType2) == NodeType.MajorCity)
                    majorCitiesLeft -= 1;

                current = previous[current];
            }
            nodes.Add(start);

            nodes.Reverse();
            return nodes;
        }
        
        /// <summary>
        /// Creates a new Route using the MapData and a list of NodeIds
        /// </summary>
        private static Route CreatePathRoute(
            TrackGraph<int> pathbuildingTracks,
            ref int majorCitiesLeft,
            List<NodeId> path
        ) {
            int cost = 0;

            // Traverse the path, adding the cost of each edge while doing so
            for (int i = 0; i < path.Count - 1; ++i)
            {
                var nodeType1 = _mapData.Nodes[path[i].GetSingleId()].Type;
                var nodeType2 = _mapData.Nodes[path[i+1].GetSingleId()].Type;

                if (!pathbuildingTracks.TryGetEdgeValue(path[i], path[i + 1], out int _))
                {
                    if ((nodeType1 ^ nodeType2) == NodeType.MajorCity)
                    {
                        if (majorCitiesLeft > 0)
                        {
                            cost += _rules.GetNodeCost(nodeType1 == NodeType.MajorCity ? nodeType2 : nodeType1);
                            majorCitiesLeft -= 1;
                        }
                        else
                            cost += _rules.GetNodeCost(NodeType.MajorCity);
                    }
                    else if ((nodeType1 & nodeType2) != NodeType.MajorCity)
                        cost += _rules.GetNodeCost(_mapData.Nodes[path[i + 1].GetSingleId()].Type);

                    if (_mapData.Segments[path[i].GetSingleId() * 6 + (int)Utilities.CardinalBetween(path[i], path[i + 1])].Type == NodeSegmentType.River)
                        cost += _rules.RiverCrossCost;

                    pathbuildingTracks[path[i], path[i + 1]] = Manager.Singleton.CurrentPlayer;
                }
            }
 
            return new Route(cost, path);
        }
        
        #endregion
    }    
}