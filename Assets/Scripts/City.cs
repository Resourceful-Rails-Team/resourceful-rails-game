using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rails
{
    [Serializable]
    public class City
    {
        [SerializeField]
        public string Name;

        /// <summary>
        /// Each element represents a good id and the amount of that good the city has.
        /// </summary>
        [SerializeField]
        public List<Vector2Int> Goods;
    }
}
