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

        /// <summary>
        /// Whether or not the NodeId is within the bounds of the map
        /// </summary>
        public bool InBounds => X >= 0 && Y >= 0 && X < Manager.Size && Y < Manager.Size;

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
