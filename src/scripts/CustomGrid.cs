using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomGridGodot.scripts;
public class CustomGrid<TItem>
{
    // Delegate to handle grid item changed event
    public delegate void GridItemChangedDelegate(int coordinateX, int coordinateY, TItem obj);
    /// <summary>
    /// Event triggered when a grid item changes
    /// </summary>
    public event GridItemChangedDelegate GridItemChanged;
    // Properties to define grid dimensions and cell size
    public int Height { get; private set; }
    public int CellSize { get; private set; }
    public int Width { get; private set; }
    // 2D array to hold grid items
    public TItem[,] GridCellCollection { get; private set; }
    // List to store grid positions
    public List<Vector2> GridPositions { get; private set; }
    // Function to get the origin position of the grid
    private readonly Func<Vector2> _originPosition;
    public CustomGrid(int width, int height, int cellSize, Func<Vector2> originPosition = null)
    {
        _originPosition = originPosition;
        Height = height;
        CellSize = cellSize;
        Width = width;
        GridCellCollection = new TItem[width, height];
        GridPositions = new();
        // Initialize grid cells and positions
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {

                GridCellCollection[i, j] = default;
                GridPositions.Add(GetWorldPosition(i, j));
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="cellSize"></param>
    /// <param name="defaultValue"> Set the default value for the grid cells</param>
    /// <param name="originPosition"></param>
    public CustomGrid(int width, int height, int cellSize,Func<TItem> defaultValue, Func<Vector2> originPosition = null)
    {
        _originPosition = originPosition;
        Height = height;
        CellSize = cellSize;
        Width = width;
        GridCellCollection = new TItem[width, height];
        GridPositions = new();
        // Initialize grid cells and positions
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {

                GridCellCollection[i, j] = defaultValue();
                GridPositions.Add(GetWorldPosition(i, j));
            }
        }
    }
    /// <summary>
    // Function to clear the grid (for testing purposes)
    /// </summary>
    public void Clear()
    {
        foreach (var item in GridCellCollection)
        {
            if (item != null && item is Node node)
            {
                node.QueueFree();
            }
        }
    }
    /// <summary>
    /// // Function to get the parent position
    /// </summary>
    /// <returns></returns>
    private Vector2 GetParentPosition() => _originPosition?.Invoke() ?? Vector2.Zero;
    /// <summary>
    /// Function to get the grid coordinate based on world position
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="parentPos"></param>
    /// <param name="coordinate"></param>
    /// <returns></returns>
    private bool TryGetGridCoordinate(Vector2 pos, Vector2 parentPos, out (int x, int y) coordinate)
    {
        var x = Mathf.FloorToInt((pos.X - parentPos.X) / (CellSize));
        var y = Mathf.FloorToInt((pos.Y - parentPos.Y) / (CellSize));
        coordinate = (x, y);
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }
    /// <summary>
    /// Function to check if a cell is empty
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private bool IsCellEmpty(int x, int y)
    {
        var obj = GridCellCollection[x, y];
        return obj == null || EqualityComparer<TItem>.Default.Equals(obj, default);
    }
    /// <summary>
    /// Function to check if a cell is empty and return the item
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    private bool IsCellEmpty(int x, int y, out TItem item)
    {
        item = GridCellCollection[x, y];
        return GridCellCollection[x, y] == null || EqualityComparer<TItem>.Default.Equals(GridCellCollection[x, y], default);
    }
    /// <summary>
    /// Function to check if a cell is empty based on world position
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public bool IsCellEmpty(Vector2 pos)
    {
        return TryGetGridCoordinate(pos, GetParentPosition(), out var coordinate) && IsCellEmpty(coordinate.x, coordinate.y);
    }
    /// <summary>
    /// Function to get the world position based on grid coordinate
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private Vector2 GetWorldPosition(int x, int y) => new Vector2(x * CellSize, y * CellSize) + GetParentPosition();
    /// <summary>
    /// Function to set an item in the grid at a specified position
    /// Currently does not overwrite existing items
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="obj"></param>
    /// <param name="positionToSet"> grid cell position in world coordinates</param>
    /// <returns></returns>
    public bool TrySetItem(Vector2 pos, TItem obj, out Vector2 positionToSet)
    {
        if (obj == null)
        {
            positionToSet = Vector2.Zero;
            return false;
        }
        var parentPos = GetParentPosition();
        if (TryGetGridCoordinate(pos, parentPos, out var coordinate) && IsCellEmpty(coordinate.x, coordinate.y))
        {
            GridCellCollection[coordinate.x, coordinate.y] = obj;
            positionToSet = new Vector2(coordinate.x * CellSize, coordinate.y * CellSize) + parentPos;
            GridItemChanged?.Invoke(coordinate.x, coordinate.y, obj);
            return true;
        }
        positionToSet = Vector2.Zero;
        return false;
    }
    public void TriggerItemChangedEvent(int x, int y)
    {
        GridItemChanged?.Invoke(x, y, GridCellCollection[x, y]);
    }
    /// <summary>
    /// Function to get an item in the grid at a specified position
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public bool TryGetItem(Vector2 pos, out TItem obj)
    {
        if (TryGetGridCoordinate(pos, GetParentPosition(), out var coordinate))
        {
            if (!IsCellEmpty(coordinate.x, coordinate.y))
            {
                obj = GridCellCollection[coordinate.x, coordinate.y];
                return true;
            }
        }
        obj = default;
        return false;
    }
    /// <summary>
    /// Function to remove an item from the grid at a specified position
    /// This only remove the refrence to the actual item(does not queue free the item, you need to do that manually)
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="removedItem"></param>
    /// <returns></returns>
    public bool TryRemoveItem(Vector2 pos, out TItem removedItem)
    {
        if (TryGetGridCoordinate(pos, GetParentPosition(), out var coordinate))
        {
            if (IsCellEmpty(coordinate.x, coordinate.y, out removedItem))
            {
                return false;
            }
            GridCellCollection[coordinate.x, coordinate.y] = default;
            return true;
        }
        removedItem = default;
        return false;
    }
}
