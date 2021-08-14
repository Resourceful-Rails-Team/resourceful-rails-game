/* This work is released under the MIT license.
    Please see the file LICENSE in this distribution for
    license terms. */

using Rails.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Data
{
    [Serializable]
    public class NodeCost
    {
        public NodeType NodeType;
        public int Cost;
    }
}
