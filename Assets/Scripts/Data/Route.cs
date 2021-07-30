using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rails.Data
{
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
}
