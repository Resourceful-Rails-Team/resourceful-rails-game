using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rails
{
    [Serializable]
    public struct NodeId
    {
        public int X;
        public int Y;

        public NodeId(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int GetSingleId()
        {
            return (X * Manager.Size) + Y;
        }

        public override bool Equals(object obj)
        {
            if(obj is NodeId node)
                return node.X == X && node.Y == Y;
            return false;
        }

        public static bool operator ==(NodeId n1, NodeId n2) => n1.Equals(n2);
        public static bool operator !=(NodeId n1, NodeId n2) => !n1.Equals(n2);

        public override int GetHashCode()
        {
            unchecked {
                int hash = 17;
                hash = hash * 23 + X.GetHashCode();
                hash = hash * 23 + Y.GetHashCode();
                return hash;
            }
        }
    }
}
