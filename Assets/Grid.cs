using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
	public enum NeighborType { UP = 0, DOWN, LEFT, RIGHT, COUNT };

	public GameObject WaypointIndicator;
	public GameObject SelectedIndicator;

	[SerializeField]
	private Vector2 _coord;

	[SerializeField]
	private Grid[] _neighbor;

	[SerializeField]
	private Grid _root;

	private Color _rootColor;
	private bool _isSelected;
	private bool _isRoad;
	private bool _isWaypoint;
	private bool _isNewWaypoint;
	
	public Vector2 Coord
	{
		get
		{
			return _coord;
		}
		set
		{
			_coord = value;
		}
	}

	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			_isSelected = value;
		}
	}

	public bool IsWaypoint
	{
		get
		{
			return _isWaypoint;
		}
		set
		{
			_isWaypoint = value;
		}
	}

	public bool IsNewWaypoint
	{
		get
		{
			return _isNewWaypoint;
		}
		set
		{
			_isNewWaypoint = value;
		}
	}
	
	public Color RootColor
	{
		get
		{
			return _rootColor;
		}
	}

	public Grid Root
	{
		get
		{
			return _root;
		}
		set
		{
			_root = value;
		}
	}

	private int NeighborCount
	{
		get
		{
			int count = 0;
			foreach (var neighbor in _neighbor)
			{
				if (neighbor != null)
				{
					++count;
				}
			}
			return count;
		}
	}

	// Use this for initialization
	void Start()
	{
		_neighbor = new Grid[(int)NeighborType.COUNT];
		_rootColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
		Init();
	}

	// Update is called once per frame
	void Update()
	{
		if (_isRoad)
		{
			GetComponent<SpriteRenderer>().color = Color.grey;
		}
		else
		{
			GetComponent<SpriteRenderer>().color = Color.white;
		}
		WaypointIndicator.GetComponent<SpriteRenderer>().color = _root.RootColor;
		WaypointIndicator.SetActive(_isWaypoint);
		SelectedIndicator.SetActive(_isSelected);
	}
	
	private void CheckNeightbor(Grid grid)
	{
		if (Coord.x > grid.Coord.x)
		{
			SetNeighbor(NeighborType.LEFT, grid);
		}
		else if (Coord.x < grid.Coord.x)
		{
			SetNeighbor(NeighborType.RIGHT, grid);
		}
		else if (Coord.y > grid.Coord.y)
		{
			SetNeighbor(NeighborType.DOWN, grid);
		}
		else if (Coord.y < grid.Coord.y)
		{
			SetNeighbor(NeighborType.UP, grid);
		}
	}

	public void Init()
	{
		Array.Clear(_neighbor, 0, _neighbor.Length);
		_isSelected = false;
		_isRoad = false;
		_isWaypoint = false;
		_isNewWaypoint = false;
		_root = this;
	}

	public void SetAsRoad(Grid prevWaypoint, Grid nextWaypoint = null)
	{
		CheckNeightbor(prevWaypoint);
		if (nextWaypoint != null)
		{
			CheckNeightbor(nextWaypoint);
		}
		if (_isWaypoint || NeighborCount > 2)
		{
			_isNewWaypoint = true;
		}
		_isSelected = false;
		_isRoad = true;
	}

	public void SetNeighbor(NeighborType neighborType, Grid waypoint)
	{
		Grid neighbor = GetNeighbor(neighborType);
		if (neighbor == null || GetManhattanDistance(neighbor) > GetManhattanDistance(waypoint))
		{
			_neighbor[(int)neighborType] = waypoint;
		}
	}

	public Grid GetNeighbor(NeighborType neighborType)
	{
		return _neighbor[(int)neighborType];
	}

	public int GetManhattanDistance(Grid grid)
	{
		return (int)(Mathf.Abs(Coord.x - grid.Coord.x) + Mathf.Abs(Coord.y - grid.Coord.y));
	}
}
