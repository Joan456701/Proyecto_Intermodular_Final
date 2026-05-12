using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Ajustes del Grid")]
    [SerializeField] private int _width = 10;
    [SerializeField] private int _height = 10;
    [SerializeField] private float _cellSize = 3f;

    private List<Grid<GridObject>> _gridList;
    [SerializeField] private int _gridVerticalCoutn = 1;
    [SerializeField] private float _gridVerticalSize = 2;

    private void Awake()
    {
        Instance = this;
        _gridList = new List<Grid<GridObject>>();

        for (int i = 0; i < _gridVerticalCoutn; i++)
        {
            Vector3 worldOrigin = new Vector3(0, i * _gridVerticalSize, 0);

            Grid<GridObject> newGrid = new Grid<GridObject>(_width, _height, _cellSize, worldOrigin, (Grid<GridObject> g, int x, int z) => new GridObject(g, x, z));

            _gridList.Add(newGrid);
        }
    }

    public Grid<GridObject> GetGrid(Vector3 worldPosition)
    {
        int gridIndex = Mathf.RoundToInt(worldPosition.y / _gridVerticalSize);

        gridIndex = Mathf.Clamp(gridIndex, 0, _gridList.Count - 1);

        return _gridList[gridIndex];
    }

    public bool HasSupportBelow(Vector3 worldPosition)
    {
        int gridIndex = Mathf.RoundToInt(worldPosition.y / _gridVerticalSize);

        if (gridIndex <= 0) return true;

        Grid<GridObject> currentGrid = _gridList[gridIndex];
        Grid<GridObject> gridBelow = _gridList[gridIndex - 1];

        currentGrid.GetXZ(worldPosition, out int x, out int z);

        GridObject objectBelow = gridBelow.GetGridObject(x, z);
        if (objectBelow != null && !objectBelow.CanBuild())
            return true;

        Vector2Int[] offsets = {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(1, 0)
        };

        FloorPlacedObject.Edge[] edgesPointingToUs = {
            FloorPlacedObject.Edge.Down,
            FloorPlacedObject.Edge.Up,
            FloorPlacedObject.Edge.Right,
            FloorPlacedObject.Edge.Left
        };

        // Edge en la misma dirección del offset (para rampas que suben hacia nuestra celda)
        FloorPlacedObject.Edge[] edgesPointingAway = {
            FloorPlacedObject.Edge.Up,
            FloorPlacedObject.Edge.Down,
            FloorPlacedObject.Edge.Left,
            FloorPlacedObject.Edge.Right
        };

        for (int i = 0; i < offsets.Length; i++)
        {
            int nx = x + offsets[i].x;
            int nz = z + offsets[i].y;

            GridObject neighborSamePiso = currentGrid.GetGridObject(nx, nz);
            if (neighborSamePiso != null && !neighborSamePiso.CanBuild())
                return true;

            GridObject neighborBelow = gridBelow.GetGridObject(nx, nz);
            if (neighborBelow != null && !neighborBelow.CanBuild())
            {
                FloorPlacedObject floorBelow = neighborBelow.GetPlacedObject()
                    ?.GetComponent<FloorPlacedObject>();

                if (floorBelow != null)
                {
                    // Pared apuntando hacia nosotros
                    FloorEdgePlacedObject edgeToUs = floorBelow.GetEdgeObject(edgesPointingToUs[i]);
                    if (edgeToUs != null && !edgeToUs.GetSO().isStairs)
                        return true;

                    // Rampa apuntando en nuestra direccion
                    FloorEdgePlacedObject edgeAway = floorBelow.GetEdgeObject(edgesPointingAway[i]);
                    if (edgeAway != null && edgeAway.GetSO().isStairs)
                        return true;
                }
            }
        }

        return false;
    }

    public float GetGridVerticalSize() => _gridVerticalSize;

}
