using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rails
{
	public enum NodeType
    {
		Clear,
		Mountain,
		SmallCity,
		MediumCity,
		MajorCity,
		Water
    }

	public class Node
	{
		[SerializeField]
		public NodeType Type { get; set; }
		[SerializeField]
		public NodeId Id { get; set; }
		[SerializeField]
		private bool[] Rivers { get; set; } = new bool[(int)Cardinal.MAX_CARDINAL];
		
		public Node(NodeId id)
		{
			Id = id;
		}

		public bool HasRiver(Cardinal cardinal)
        {
			return Rivers[(int)cardinal];
        }

		public void SetHasRiver(Cardinal cardinal, bool hasRiver)
        {
			Rivers[(int)cardinal] = hasRiver;
        }
	}
}
