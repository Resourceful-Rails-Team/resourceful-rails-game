using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rails.Data;
using Rails.ScriptableObjects;
using UnityEngine;

namespace Rails.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Map/MapData", order = 1)]
    public class MapData : ScriptableObject
    {
        public MapTokenTemplate DefaultTokenTemplate;
        public PlayerTokenTemplate DefaultPlayerTemplate;
        public GameObject Board;

        public GameRules DefaultRules;
        
        [HideInInspector]
        public Node[] Nodes;
        [HideInInspector]
        public NodeSegment[] Segments;
        [HideInInspector]
        public List<City> Cities = new List<City>();
        [HideInInspector]
        public List<Good> Goods = new List<Good>();


        public Node GetNodeAt(NodeId id)
        {
            var index = id.GetSingleId();
            if (index < 0 || index >= Nodes.Length)
                return null;

            return Nodes[index];
        }

        public Node GetNodeAt(int index)
        {
            if (index < 0 || index >= Nodes.Length)
                return null;

            return Nodes[index];
        }

        public NodeSegment[] GetNodeSegments(NodeId id)
        {
            int index = id.GetSingleId() * 6;
            return new NodeSegment[]
            {
                GetNodeSegmentAt(index + 0),
                GetNodeSegmentAt(index + 1),
                GetNodeSegmentAt(index + 2),
                GetNodeSegmentAt(index + 3),
                GetNodeSegmentAt(index + 4),
                GetNodeSegmentAt(index + 5),
            };
        }

        public NodeSegment GetNodeSegmentAt(int index)
        {
            if (index < 0 || index >= Segments.Length)
                return null;

            return Segments[index];
        }

        public Tuple<NodeId, Node>[] GetNeighborNodes(NodeId nodeId)
        {
            var node = Nodes[nodeId.GetSingleId()];
            var cityId = Nodes[nodeId.GetSingleId()].CityId;
            var cardinalRange = Enumerable.Range((int)Cardinal.N, (int)Cardinal.MAX_CARDINAL);

            return
                 cardinalRange
                .Select(c => Utilities.PointTowards(nodeId, (Cardinal)c))
                .Where(nId => nId.InBounds)
                .Select(nId => Tuple.Create(nId, Nodes[nId.GetSingleId()]))
                .ToArray();
        }
    }
}
