using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{
	private const int NUMBER_COLUMNS	= 13;
	private const int NUMBER_ROWS		= 10;
	private const int GRID_WIDTH		= 1;
	private const int GRID_HEIGHT		= 1;

	public GameObject GridPrefab;

	private List<Grid> _grids;
	private List<Grid> _selectedGrids;
	private DisjointSet _disjointSet;

	// Use this for initialization
	void Start()
	{
		_grids = new List<Grid>();
		for (int j = 0; j < NUMBER_ROWS; ++j)
		{
			for (int i = 0; i < NUMBER_COLUMNS; ++i)
			{
				GameObject gridObject = Instantiate(GridPrefab, transform);
				gridObject.transform.localPosition = new Vector3(i * GRID_WIDTH, j * GRID_HEIGHT, 0);
				Grid grid = gridObject.GetComponent<Grid>();
				grid.Coord = new Vector2(i, j);
				gridObject.name = "Grid " + grid.Coord.ToString();
				_grids.Add(grid);
			}
		}
		Init();
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			Init();
		}
		if (Input.GetMouseButtonUp(0))
		{
			ConstructRoad();
			UpdateNewWaypoint();
			UpdateWaypointColor();
		}
	}

	private void FixedUpdate()
	{
		if (Input.GetMouseButton(0))
		{
			Collider2D hitCollider = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
			if (hitCollider)
			{
				Grid grid = hitCollider.GetComponent<Grid>();
				if (_selectedGrids.Count == 0 || _selectedGrids.Last() != grid)
				{
					while (_selectedGrids.Count > 0 && grid.GetManhattanDistance(_selectedGrids.Last()) > 1)
					{
						Vector2 coord = _selectedGrids.Last().Coord;
						if (coord.x > grid.Coord.x)
						{
							--coord.x;
							SelectGrid(coord);
						}
						else if (coord.x < grid.Coord.x)
						{
							++coord.x;
							SelectGrid(coord);
						}
						else if (coord.y > grid.Coord.y)
						{
							--coord.y;
							SelectGrid(coord);
						}
						else if (coord.y < grid.Coord.y)
						{
							++coord.y;
							SelectGrid(coord);
						}
					}
					SelectGrid(grid);
				}
			}
		}
	}

	private void Init()
	{
		foreach (var grid in _grids)
		{
			grid.Init();
		}
		_selectedGrids = new List<Grid>();
		_disjointSet = new DisjointSet();
	}

	private void SelectGrid(Vector2 coord)
	{
		SelectGrid(GetGrid(coord));
	}

	private void SelectGrid(Grid grid)
	{
		_selectedGrids.Add(grid);
		grid.IsSelected = true;
	}

	private void SetWaypoint(Grid grid, Grid unionGrid = null)
	{
		if (grid.IsWaypoint == false)
		{
			grid.IsWaypoint = true;
			_disjointSet.AddWaypoint(grid);
		}
		if (unionGrid != null)
		{
			_disjointSet.Union(grid, unionGrid);
		}
	}

	private void ConstructRoad()
	{
		int lastSelectedGridIndex = _selectedGrids.Count - 1;
		List<int> waypointIndex = new List<int>{ 0 };
		for (int i = 2; i < _selectedGrids.Count; ++i)
		{
			if (_selectedGrids[i - 1].Coord - _selectedGrids[i - 2].Coord != _selectedGrids[i].Coord - _selectedGrids[i - 1].Coord)
			{
				waypointIndex.Add(i - 1);
			}
		}
		waypointIndex.Add(lastSelectedGridIndex);
		
		SetWaypoint(_selectedGrids[0]);
		for (int i = 1; i < waypointIndex.Count; ++i)
		{
			SetWaypoint(_selectedGrids[waypointIndex[i]], _selectedGrids[0]);
		}
		
		_selectedGrids[0].SetAsRoad(_selectedGrids[waypointIndex[1]]);
		for (int i = 1; i < waypointIndex.Count; ++i)
		{
			Grid prevWaypoint = _selectedGrids[waypointIndex[i - 1]];
			Grid nextWaypoint = _selectedGrids[waypointIndex[i]];
			for (int j = waypointIndex[i - 1] + 1; j < waypointIndex[i]; ++j)
			{
				_selectedGrids[j].SetAsRoad(prevWaypoint, nextWaypoint);
			}
			if (i == waypointIndex.Count - 1)
			{
				break;
			}
			_selectedGrids[waypointIndex[i]].SetAsRoad(prevWaypoint, _selectedGrids[waypointIndex[i + 1]]);
		}
		_selectedGrids[lastSelectedGridIndex].SetAsRoad(_selectedGrids[waypointIndex[waypointIndex.Count - 2]]);
		_selectedGrids = new List<Grid>();
	}
	
	private void UpdateNewNeighbor(Grid grid, Grid.NeighborType neighborType)
	{
		Grid neighbor = grid.GetNeighbor(neighborType);
		if (neighbor == null)
		{
			return;
		}
		
		Vector2 coordStep = GetCoordStep(neighborType);
		Vector2 coord = grid.Coord + coordStep;
		Grid roadNode = GetGrid(coord);
		Grid.NeighborType oppositeNeighborType = GetOppositeNeighborType(neighborType);
		if (roadNode.GetNeighbor(oppositeNeighborType) == grid)
		{
			return;
		}
		
		while (true)
		{
			roadNode.SetNeighbor(oppositeNeighborType, grid);
			if (roadNode.IsWaypoint)
			{
				_disjointSet.Union(grid, roadNode);
				return;
			}
			coord += coordStep;
			roadNode = GetGrid(coord);
		}
	}

	private void UpdateNewWaypoint()
	{
		foreach (var grid in _grids)
		{
			if (grid.IsNewWaypoint == false)
			{
				continue;
			}
			if (grid.IsWaypoint == false)
			{
				SetWaypoint(grid, null);
			}
			UpdateNewNeighbor(grid, Grid.NeighborType.UP);
			UpdateNewNeighbor(grid, Grid.NeighborType.DOWN);
			UpdateNewNeighbor(grid, Grid.NeighborType.LEFT);
			UpdateNewNeighbor(grid, Grid.NeighborType.RIGHT);
			grid.IsNewWaypoint = false;
		}
	}

	private void UpdateWaypointColor()
	{
		foreach (var grid in _grids)
		{
			if (grid.IsWaypoint)
			{
				grid.Root = _disjointSet.Find(grid);
			}
		}
	}

	private Grid GetGrid(Vector2 coord)
	{
		return GetGrid((int)coord.x, (int)coord.y);
	}

	private Grid GetGrid(int x, int y)
	{
		Debug.Assert(x >= 0 && x < NUMBER_COLUMNS && y >= 0 && y < NUMBER_ROWS, x.ToString() + " " + y.ToString());
		return _grids[y * NUMBER_COLUMNS + x];
	}

	private Grid.NeighborType GetOppositeNeighborType(Grid.NeighborType neighborType)
	{
		switch (neighborType)
		{
		case Grid.NeighborType.UP:
			return Grid.NeighborType.DOWN;
		case Grid.NeighborType.DOWN:
			return Grid.NeighborType.UP;
		case Grid.NeighborType.LEFT:
			return Grid.NeighborType.RIGHT;
		case Grid.NeighborType.RIGHT:
			return Grid.NeighborType.LEFT;
		default:
			Debug.Assert(false);
			return Grid.NeighborType.COUNT;
		}
	}

	private Vector2 GetCoordStep(Grid.NeighborType neighborType)
	{
		switch (neighborType)
		{
		case Grid.NeighborType.UP:
			return new Vector2(0, 1);
		case Grid.NeighborType.DOWN:
			return new Vector2(0, -1);
		case Grid.NeighborType.LEFT:
			return new Vector2(-1, 0);
		case Grid.NeighborType.RIGHT:
			return new Vector2(1, 0);
		default:
			Debug.Assert(false);
			return new Vector2(0, 0);
		}
	}
}
