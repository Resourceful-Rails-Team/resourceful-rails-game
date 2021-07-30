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
