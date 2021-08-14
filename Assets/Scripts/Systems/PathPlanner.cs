/* This work is released under the MIT license.
    Please see the file LICENSE in this distribution for
    license terms. */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rails.Data;
using Rails.Rendering;
using System.Linq;
using Assets.Scripts.Data;
using System;

namespace Rails.Systems
{
    public static class PathPlanner
    {
        #region Public Properties
        // Build Track
        public delegate void OnCurrentCostChangeHandler();
        public static event OnCurrentCostChangeHandler OnCurrentCostChange;

        public static int CurrentCost { get; private set; }
        public static int Paths
        {
            get { return buildPaths.Count; }
        }
        public static int CurrentPath
        {
            get { return currentPath; }
            set { SetPath(value); }
        }
        public static int CurrentNode
        {
            get { return currentNode; }
            set { SetNode(value); }
        }  
       
        public static Route moveRoute;
        public static List<Route> buildRoutes;
        #endregion

        #region Properties
        private static Manager manager;
        private static List<List<NodeId>> buildPaths;
        private static int currentPath;
        private static int currentNode;
        #endregion

        public static void Initialize()
        {
            manager = Manager.Singleton;
            buildPaths = new List<List<NodeId>>();
            buildPaths.Add(new List<NodeId>());
            moveRoute = null;
            buildRoutes = new List<Route>();
            currentPath = 0;
            currentNode = 0;
        }

        #region Path
        // Adds a new path to the end of the list.
        public static int CreatePath()
        {
            buildPaths.Add(new List<NodeId>());
            currentNode = 0;
            return buildPaths.Count - 1;
        }
        // Sets the index of the current build path.
        public static void SetPath(int path)
        {
            currentPath = CircularIndex(path, 0, Paths);
            return;
        }
        // Returns the list of nodes of the specified path.
        public static List<NodeId> GetPath(int path)
        {
            if (path >= 0 && path < Paths)
                return buildPaths[path];
            return null;
        }
        // Clear move path.
        public static void ClearPath()
        {
            GameGraphics.HighlightRoute(moveRoute, null);
            manager.Player.movePath.Clear();
            manager.Player.movePath.Add(manager.Player.trainPosition);
            currentNode = 1;
        }
        // Clear all build paths.
        public static void ClearBuild()
        {
            buildPaths.Clear();
            buildPaths.Add(new List<NodeId>());
            currentPath = 0;
            currentNode = 0;
        }
        // Clear current build path.
        public static bool ClearPath(int path)
        {
            if (path < 0 || path >= Paths)
                return false;

            GameGraphics.DestroyPotentialTrack(buildRoutes[path]);
            buildPaths[path].Clear();
            if (buildPaths.Count == 0)
                buildPaths.Add(new List<NodeId>());
            currentNode = 0;
            currentPath = 0;
            PlannedTracks();

            return true;
        }
        // Removes a path from the list.
        public static bool RemovePath(int path)
        {
            if (Paths != 0 && path >= 0 && path < Paths)
            {
                buildPaths.RemoveAt(path);

                // If there are no paths left, add one.
                if (Paths == 0)
                {
                    buildPaths.Add(new List<NodeId>());
                    currentPath = 0;
                    currentNode = 0;
                }
                else
                {
                    // Decrement current path and reset if it goes less than 0.
                    --currentPath;
                    if (currentPath < 0)
                        currentPath = 0;

                    // Reset current node to be in bounds.
                    int nodes = buildPaths[currentPath].Count - 1;
                    if (currentNode > nodes)
                        currentNode = nodes;
                }

                PlannedTracks();
                return true;
            }
            return false;
        }
        #endregion

        #region Node
        // Adds a node to move path.
        public static void AddNode(NodeId node)
        {
            manager.Player.movePath.Insert(currentNode, node);
            currentNode += 1;
            PlannedRoute();
            return;
        }
        // Adds a node to build path.
        public static void AddNode(int path, NodeId node)
        {
            buildPaths[path].Insert(currentNode, node);
            currentNode += 1;
            PlannedTracks();
            return;
        }
        // Remose a node from move path.
        public static bool RemoveNode(int node)
        {
            if (node >= 0 && node < manager.Player.movePath.Count)
            {
                if (currentNode > node)
                    --currentNode;
                manager.Player.movePath.RemoveAt(node);
                return true;
            }
            return false;
        }
        // Removes a node from build path
        public static bool RemoveNode(int path, int node)
        {
            if (path < 0 || path >= Paths)
                return false;
            if (node < 0 || node >= buildPaths[path].Count)
                return false;

            if (currentNode > node)
                --currentNode;
            buildPaths[path].RemoveAt(node);
            return true;
        }
        // Sets current node for move path.
        public static void SetNode(int node)
        {
            currentNode = CircularIndex(node, 0, manager.Player.movePath.Count+1);
            return;
        }
        // Sets the current node for build path.
        public static void SetNode(int path, int node)
        {
            currentNode = CircularIndex(node, 0, buildPaths[path].Count+1);
            return;
        }
        #endregion

        #region Other
        public static TrainCityInteraction GetStop(NodeId id)
        {
            var currentCityId = manager.MapData.Nodes[id.GetSingleId()].CityID;
            var node = manager.MapData.Nodes[id.GetSingleId()];

            if (node.Type >= NodeType.SmallCity && node.Type <= NodeType.MajorCity)
            {
                var city = manager.MapData.Cities[node.CityID];
                return new TrainCityInteraction
                {
                    // Select any demand cards that both match the city, and
                    // which the player has the good in their load
                    Cards = manager.Player.demandCards
                            .Where(dc => dc.Any(d =>
                                d.City == city &&
                                manager.Player.goodsCarried.Contains(d.Good)
                            ))
                            .OrderBy(dc => dc.FirstOrDefault(x=>x.City == city).Reward)
                            .ToArray(),

                    // Select any good that is from the city, and that
                    // the player can currently pick up
                    Goods =
                            manager.MapData.GetGoodsAtCity(manager.MapData.Cities[node.CityID])
                                .Select(gi => manager.MapData.Goods[gi])
                                .Where(g => GoodsBank.GetGoodQuantity(g) > 0)
                                .ToArray(),

                    PlayerIndex = manager.CurrentPlayer,
                    TrainPosition = id,
                    City = manager.MapData.Cities[node.CityID]
                };
            }
            return null;
        }
        
        // Show the planned route on the map.
        public static void PlannedRoute()
        {
            if (manager.Player.movePath.Count > 0)
            {
                if (moveRoute != null)
                {
                    GameGraphics.HighlightRoute(moveRoute, null);
                }

                int move = manager.Rules.TrainSpecs[manager.Player.trainType].movePoints;
                // Calculate the Route
                moveRoute = Pathfinding.CheapestMove(
                    manager.CurrentPlayer,
                    move,
                    manager.Player.movePath.ToArray());

                var movePoints = Mathf.Min(manager.Player.movePointsLeft, moveRoute.Distance);

                for (int i = 0; i < moveRoute.Distance; ++i)
                {
                    if (moveRoute.Nodes.Take(i).Contains(moveRoute.Nodes[i]))
                        continue;

                    // Highlight the Route
                    var trackToken = GameGraphics.GetTrackToken(moveRoute.Nodes[i], moveRoute.Nodes[i + 1]);

                    if (i < movePoints)
                        trackToken.Color = trackToken.PrimaryColor * 4.0f;
                    else
                        trackToken.Color = trackToken.PrimaryColor * 2.0f;
                }

                CurrentCost = moveRoute.Cost;
                OnCurrentCostChange?.Invoke();
            }
            return;
        }
        // Show the planned tracks on the map.
        public static void PlannedTracks()
        {
            if (buildPaths.Count > 0)
            {
                if (buildRoutes != null)
                {
                    foreach (var route in buildRoutes)
                        GameGraphics.DestroyPotentialTrack(route);
                    buildRoutes.Clear();
                }
                buildRoutes = Pathfinding.CheapestBuilds(buildPaths);

                foreach (var route in buildRoutes)
                {
                    GameGraphics.GeneratePotentialTrack(route, Color.yellow);
                }

                CurrentCost = buildRoutes.Sum(br => br.Cost);
                OnCurrentCostChange?.Invoke();
            }
            return;
        }

        public static void CommitTracks()
        {
            foreach (Route route in buildRoutes)
            {
                // Check to make sure we're not over the spending limit.
                GameGraphics.CommitPotentialTrack(route, manager.Player.color);
            }
            CurrentCost = 0;
            OnCurrentCostChange?.Invoke();
        }
        #endregion

        #region Private
        // Sets an index to be between two values, inclusive min, exclusive max.
        private static int CircularIndex(int index, int min, int max)
        {
            while (index < min)
                index += max;
            while (index >= max)
                index -= max;
            return index;
        }
        #endregion
    }
}