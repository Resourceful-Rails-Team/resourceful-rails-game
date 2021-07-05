using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rails
{
    public class Manager : MonoBehaviour
    {
        /// <summary>
        /// Map size.
        /// </summary>
        public const int Size = 64;

        #region Singleton

        private static Manager _singleton = null;

        /// <summary>
        /// Manager singleton
        /// </summary>
        public static Manager Singleton
        {
            get
            {
                if (_singleton)
                    return _singleton;

                GameObject go = new GameObject("Manager");
                return go.AddComponent<Manager>();
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public float WSSize = 1f;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        public MapData Map;

        /// <summary>
        ///
        /// </summary>
        [SerializeField]
        private Dictionary<Vector2Int, int[]> Tracks = new Dictionary<Vector2Int, int[]>();

        #endregion

        #region Unity Events

        private void Awake()
        {
            // set singleton reference on awake
            _singleton = this;
        }

        private void OnDrawGizmos()
        {
            if (Map == null || Map.Nodes == null || Map.Nodes.Length == 0)
                return;

            Gizmos.color = Color.black;
            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    Gizmos.DrawSphere(GetPosition(Map.Nodes[(y * Size) + x].Id), WSSize * 0.1f);
                }
            }
        }

        #endregion

        #region Utilities

        public Vector3 GetPosition(NodeId id)
        {
            var w = 2 * WSSize;
            var h = Mathf.Sqrt(3) * WSSize;
            var wspace = 0.75f * w;
            var pos = new Vector3(id.X * wspace, 0, id.Y * h);
            int parity = id.X & 1;
            if (parity == 1)
                pos.z += h / 2;

            return pos;
        }

        #endregion 
        
        /// <summary>
        /// Inserts a new track onto the Map, based on position and direction.
        /// </summary>
        /// <param name="player">The player who owns the track</param>
        /// <param name="position">The position the track is placed</param>
        /// <param name="towards">The cardinal direction the track moves towards</param>
        private void InsertTrack(int player, Vector2Int position, Cardinal towards)
        {
            // If Cardinal data doesn't exist for the point yet,
            // insert and initialize the data
            if(!Tracks.ContainsKey(position))
            {
                Tracks[position] = new int[(int)Cardinal.MAX_CARDINAL];
                for(int i = 0; i < (int)Cardinal.MAX_CARDINAL; ++i)
                    Tracks[position][i] = -1;
            }

            Tracks[position][(int)towards] = player;

            // As Tracks is undirected, insert a track moving the opposite way from the
            // target node as well.
            InsertTrack(player, PointTowards(position, towards), ReflectCardinal(towards));
        }

        /// <summary>
        /// Returns the point represented by a combined point and cardinal.
        /// The point returned is the one moving by Cardinal direction, one space over
        /// from start.
        /// </summary>
        /// <param name="start">The start point</param>
        /// <param name="towards">The cardinal direction moving towards</param>
        /// <returns>The new point being moved towards.</returns>
        private Vector2Int PointTowards(Vector2Int start, Cardinal towards)
        {
            bool isOdd = start.x % 2 == 1;

            var dir = towards switch
            {
                Cardinal.N => Vector2Int.up,
                Cardinal.NE => isOdd ? new Vector2Int(1, 1) : Vector2Int.right,
                Cardinal.NW => isOdd ? new Vector2Int(-1, 1) : Vector2Int.left, 
                Cardinal.S => Vector2Int.down,
                Cardinal.SW => isOdd ? Vector2Int.left : new Vector2Int(-1, -1),
                _ => isOdd ? Vector2Int.right : new Vector2Int(1, -1),
            };

            return start + dir;
        }

        /// <summary>
        /// Reflects the given Cardinal, returning the
        /// exact opposite direction. (ie. N -> S, NE -> SW).
        /// </summary>
        /// <param name="towards">The Cardinal being reflected</param>
        /// <returns>The opposite cardinal direction to towards</returns> 
        private Cardinal ReflectCardinal(Cardinal towards) =>
            (Cardinal)Mathf.Repeat((int)Cardinal.SW, (int)towards - 3); 
    
        public List<Vector2Int> LeastCostTrack(int player, Vector2Int start, Vector2Int end) 
        {
            if(start == end)
                return new List<Vector2Int> { start };
            if(!Tracks.ContainsKey(start) || !Tracks.ContainsKey(end))
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
                    var newPoint = PointTowards(node.Position, c);
                    if(Tracks.ContainsKey(newPoint))
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
    }

    public class WeightedNode : IComparable<WeightedNode>
    {
        public Vector2Int Position { get; set; }
        public int Weight { get; set; }
        public WeightedNode(Vector2Int position, int weight)
        {
            Position = position;
            Weight = weight;
        }
        public int CompareTo(WeightedNode other) => Weight.CompareTo(other.Weight);
    }
}
