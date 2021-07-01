using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rails
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Map/MapData", order = 1)]
    public class MapData : ScriptableObject
    {
        [SerializeField]
        public Node[,] Nodes;
        [SerializeField]
        public NodeSegment[] Segments;

        public NodeSegment[] GetNodeSegments(NodeId id)
        {
            int index = ((id.X * Nodes.Length) + id.Y) * 6;
            return new NodeSegment[]
            {
                Segments[index + 0],
                Segments[index + 1],
                Segments[index + 2],
                Segments[index + 3],
                Segments[index + 4],
                Segments[index + 5],
            };
        }
    }
}
