using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
public class HexGrid : MonoBehaviour
{
	public enum cardinal { N, NE, SE, S, SW, NW }
	static int[,,] offset = new int[2, 6, 2] {
		{ { 1, 0}, { 1,-1}, {0,-1},
		  {-1,-1}, {-1, 0}, {0, 1} },
		{ { 1, 1}, { 1, 0}, {0,-1},
		  {-1, 0}, {-1, 1}, {0, 1} },
	};

	public static float size = 1.0f;
	float w, h, wspace;

	public int x_size, y_size;
	Node[,] nodes;

	private void Start()
	{
		w = 2 * size;
		h = Mathf.Sqrt(3) * size;
		wspace = 0.75f * w;
		CreateNodes();
		SetNeighbors();
	}

	void CreateNodes()
	{
		nodes = new Node[x_size, y_size];
		Node node;
		Vector3 pos;

		for (int x = 0; x < x_size; x++)
		{
			for (int y = 0; y < y_size; y++)
			{
				pos = new Vector3(x * wspace, 0, y * h);
				int parity = x & 1;
				if (parity == 1)
					pos.z += h / 2;
				node = new Node(x, y, pos);
				nodes[x, y] = node;
			}
		}
	}
	void SetNeighbors()
	{
		for (int x = 0; x < x_size; x++)
		{
			for (int y = 0; y < y_size; y++)
			{
				int p = x & 1;
				for (cardinal dir = cardinal.N; dir <= cardinal.NW; dir++)
				{
					int x_offset = x + offset[p, (int)dir, 0];
					int y_offset = y + offset[p, (int)dir, 1];
					if (x_offset >= 0 && x_offset < x_size &&
						y_offset >= 0 && y_offset < y_size)
					{
						nodes[x, y].setNeighbor(dir, nodes[x_offset, y_offset]);
					}
					else
					{
						nodes[x, y].setNeighbor(dir, null);
					}
				}
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (nodes == null)
			return;
		Gizmos.color = Color.black;
		for (int x = 0; x < x_size; x++)
		{
			for (int y = 0; y < y_size; y++)
			{
				Gizmos.DrawSphere(nodes[x, y].position, size * 0.1f);
			}
		}
	}
}
*/