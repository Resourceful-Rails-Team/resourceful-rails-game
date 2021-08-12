using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rails.Data;
using Rails.Rendering;
using System.Linq;

namespace Rails.Systems
{
    public static class PathPlanner
    {
        #region Public Properties
        public static int BuildCost
        {
            get {
                int cost = 0;
                foreach (Route route in buildRoutes)
                {
                    cost += route.Cost;
                }
                return cost;
            }
        }
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

        #region Public
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

        // Adds a new path to the end of the list.
        public static int CreatePath()
        {
            buildPaths.Add(new List<NodeId>());
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
        }
        // Clear all build paths.
        public static void ClearBuild()
        {
            buildPaths.Clear();
            buildPaths.Add(new List<NodeId>());
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
            PlannedTracks();

            return true;
        }
        // Removes a path from the list.
        public static bool RemovePath(int path)
        {
            if (path >= 0 && path < Paths)
            {
                buildPaths[path].RemoveAt(path);
                return true;
            }
            return false;
        }


        // Adds a node to move path.
        public static void AddNode(NodeId node)
        {
            manager.Player.movePath.Add(node);
            PlannedRoute();
            return;
        }
        // Adds a node to build path.
        public static void AddNode(int path, NodeId node)
        {
            buildPaths[path].Add(node);
            PlannedTracks();
            return;
        }
        // Remose a node from move path.
        public static bool RemoveNode(int node)
        {
            if (node >= 0 && node < manager.Player.movePath.Count)
            {
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

            buildPaths[path].RemoveAt(node);
            return true;
        }
        // Sets current node for move path.
        public static void SetNode(int node)
        {
            currentNode = CircularIndex(node, 0, manager.Player.movePath.Count);
            return;
        }
        // Sets the current node for build path.
        public static void SetNode(int path, int node)
        {
            currentNode = CircularIndex(node, 0, Paths);
            return;
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
        // Show the planned tracks on the map.
        private static void PlannedTracks()
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
                    GameGraphics.GeneratePotentialTrack(route, Color.yellow);
            }
            return;
        }
        // Show the planned route on the map.
        private static void PlannedRoute()
        {
            if (Manager.Singleton.Player.movePath.Count > 0)
            {
                if (moveRoute != null)
                {
                    GameGraphics.HighlightRoute(moveRoute, null);
                }

                int move = manager._rules.TrainSpecs[manager.Player.trainType].movePoints;
                // Calculate the Route
                moveRoute = Pathfinding.CheapestMove(
                    manager.CurrentPlayer,
                    move,
                    manager.Player.movePath.ToArray());

                var movePoints = Mathf.Min(move, moveRoute.Distance);

                // Highlight the Route
                Color pco = manager.Player.color;
                GameGraphics.HighlightRoute(moveRoute.Nodes.Take(movePoints + 1).ToList(), pco * 2.0f);
                GameGraphics.HighlightRoute(moveRoute.Nodes.Take(movePoints).ToList(), pco);
            }
            return;
        }
        #endregion
    }
}