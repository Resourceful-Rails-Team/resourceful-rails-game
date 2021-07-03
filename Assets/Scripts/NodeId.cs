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
    }
}
