/* This work is released under the MIT license.
    Please see the file LICENSE in this distribution for
    license terms. */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public override bool Equals(object obj)
            => obj is Route route && route.Nodes.SequenceEqual(Nodes);
        public override int GetHashCode()
        {
            unchecked { return 37 * Nodes.Sum(n => n.GetHashCode() * 17); }
        }
    }
}
