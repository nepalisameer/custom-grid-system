using CustomGridGodot.scripts;
using Godot;
namespace CustomGridGodot;
public partial class GridMapDemo : Node2D
{
    [Export] public int CellSize = 128;
    [Export] public int Width = 5;
    [Export] public int Height = 4;
    [Export] public PackedScene packedScene;
    [Export] public Vector2 Offset = Vector2.Zero;
    [Export] public Color Color = Colors.GreenYellow;
    CustomGrid<Node2D> customGrid;
    public override void _Ready()
    {
        customGrid = new CustomGrid<Node2D>(Width, Height, CellSize, () => Position + Offset);
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("ui_accept"))
        {
            var a = packedScene.Instantiate<Node2D>();
            if (customGrid.TrySetItem(GetGlobalMousePosition(), a, out var x))
            {
                AddChild(a);
                //a.GlobalPosition = x + new Vector2(size / 2, size / 2); // set to center og grid
                a.GlobalPosition = x;
            }
        }
        if (Input.IsActionJustPressed("mouse_right"))
        {
            if (customGrid.TryRemoveItem(GetGlobalMousePosition(), out var x))
            {
                x.QueueFree();
            }
        }
    }
    public override void _Draw()
    {
        if (customGrid != null)
        {
            DrawSetTransformMatrix(GlobalTransform.AffineInverse());
            //range between 0 and 1
            float gap = 1f;
            for (int i = 0; i < customGrid.GridPositions.Count; i++)
            {
                DrawRect(new Rect2(customGrid.GridPositions[i] + new Vector2(customGrid.CellSize - (customGrid.CellSize * gap), customGrid.CellSize - (customGrid.CellSize * gap)) / 2, customGrid.CellSize * gap, customGrid.CellSize * gap), Color, filled: false, width: 3);
            }
        }

    }
}