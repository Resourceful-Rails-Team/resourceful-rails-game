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
    public enum NodeSegmentType
    {
        None,
        River
    }

    [Serializable]
    public class NodeSegment
    {
        [SerializeField]
        public NodeSegmentType Type = NodeSegmentType.None;
    }
}
