using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rails
{
	[Serializable]
	public enum NodeType
    {
		Clear,
		Mountain,
		SmallCity,
		MediumCity,
		MajorCity,
		Water
    }

	[Serializable]
	public class Node
	{
		[SerializeField]
		public NodeType Type;
		[SerializeField]
		public NodeId Id;
		[SerializeField]
		private bool[] Rivers = new bool[(int)Cardinal.MAX_CARDINAL];

		/// <summary>
		/// Index in Cities array that this node is associated with.
		/// Only applies to a city NodeType
		/// </summary>
		[SerializeField]
		public int CityId = -1;
		
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
