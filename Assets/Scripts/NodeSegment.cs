using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rails
{
    public enum NodeSegmentType
    {
        None,
        River
    }

    public class NodeSegment
    {
        public NodeSegmentType Type { get; set; } = NodeSegmentType.None;
    }
}
