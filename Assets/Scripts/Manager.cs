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

                _singleton = FindObjectOfType<Manager>();
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
        private Dictionary<NodeId, int[]> Tracks = new Dictionary<NodeId, int[]>();

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

            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    // draw node
                    var node = Map.Nodes[(y * Size) + x];
                    var pos = GetPosition(node.Id);
                    Gizmos.color = Utilities.GetNodeColor(node.Type);
                    Gizmos.DrawSphere(pos, WSSize * 0.3f);

                    // draw segments
                    // we iterate only bottom-right half of segments to prevent drawing them twice
                    var segments = Map.GetNodeSegments(node.Id);
                    for (Cardinal c = Cardinal.NE; c <= Cardinal.S; ++c)
                    {
                        // get segment
                        var segment = segments[(int)c];
                        if (segment != null)
                        {
                            // get neighboring nodeid
                            var nextNodeId = Utilities.PointTowards(node.Id, c);
                            if (nextNodeId.InBounds)
                            {
                                // draw line to
                                Gizmos.color = Utilities.GetSegmentColor(segment.Type);
                                Gizmos.DrawLine(pos, GetPosition(nextNodeId));
                            }
                        }
                    }
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

        /// <summary>
        /// Returns a collection of NodeIds of nodes that lie within the given circle.
        /// </summary>
        /// <param name="position">Position of the circle</param>
        /// <param name="radius">Radius of circle</param>
        public List<NodeId> GetNodeIdsByPosition(Vector3 position, float radius)
        {
            List<NodeId> nodeIds = new List<NodeId>();
            var w = 2 * WSSize;
            var h = Mathf.Sqrt(3) * WSSize;
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
            int maxX = Mathf.Min(Size-1, Mathf.CeilToInt(centerNodeId.x) + extents);
            int minY = Mathf.Max(0, (int)centerNodeId.y - extents);
            int maxY = Mathf.Min(Size-1, Mathf.CeilToInt(centerNodeId.y) + extents);

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

        #endregion 
        
        /// <summary>
        /// Inserts a new track onto the Map, based on position and direction.
        /// </summary>
        /// <param name="player">The player who owns the track</param>
        /// <param name="position">The position the track is placed</param>
        /// <param name="towards">The cardinal direction the track moves towards</param>
        private void InsertTrack(int player, NodeId position, Cardinal towards)
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
            InsertTrack(player, Utilities.PointTowards(position, towards), Utilities.ReflectCardinal(towards));
        }
    }
}
