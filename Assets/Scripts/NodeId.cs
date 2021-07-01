using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rails
{
    public struct NodeId
    {
        public int X { get; set; }
        public int Y { get; set; }

        public NodeId(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int GetSingleId()
        {
            return (X * Manager.Size) + Y;
        }
    }
}
