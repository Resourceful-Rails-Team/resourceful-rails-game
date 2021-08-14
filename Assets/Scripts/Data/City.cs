/* This work is released under the MIT license.
    Please see the file LICENSE in this distribution for
    license terms. */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rails.Data
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
