using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisjointSet
{
	private Dictionary<Grid, Grid> _parent;
	private Dictionary<Grid, int> _rank;
	
	public DisjointSet()
	{
		_parent = new Dictionary<Grid, Grid>();
		_rank = new Dictionary<Grid, int>();
	}

	public void AddWaypoint(Grid grid)
	{
		_parent.Add(grid, grid);
		_rank.Add(grid, 0);
	}

	public Grid Find(Grid grid)
	{
		if (_parent.ContainsKey(grid) == false)
		{
			return null;
		}
		if (_parent[grid] == grid)
		{
			return grid;
		}
		else
		{
			Grid root = Find(_parent[grid]);
			_parent[grid] = root;
			return root;
		}
	}

	public void Union(Grid grid1, Grid grid2)
	{
		Grid root1 = Find(grid1);
		Grid root2 = Find(grid2);
		if (root1 == null || root2 == null || root1 == root2)
		{
			return;
		}

		if (_rank[root1] < _rank[root2])
		{
			_parent[root1] = root2;
		}
		else if (_rank[root2] < _rank[root1])
		{
			_parent[root2] = root1;
		}
		else
		{
			_parent[root1] = root2;
			++_rank[root2];
		}
	}
}
