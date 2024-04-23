using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomGridGodot.scripts
{
    public partial class ColorTest : Node2D
    {
        [Export] public int CellSize = 128;
        [Export] public int Width = 5;
        [Export] public int Height = 4;
        [Export] public Vector2 Offset = Vector2.Zero;
        [Export] public Color Color = Colors.GreenYellow;
        [Export] Font Font;
        CustomGrid<bool> customGrid;
        public override void _Ready()
        {
            customGrid = new CustomGrid<bool>(Width, Height, CellSize, () => Position + Offset);
        }

        public override void _Process(double delta)
        {
            if (Input.IsActionJustPressed("mouse_left"))
            {
                
                if (customGrid.TrySetItem(GetGlobalMousePosition(), true, out var x))
                {
                    QueueRedraw();
                }
            }
            if (Input.IsActionJustPressed("mouse_right"))
            {
                if (customGrid.TryRemoveItem(GetGlobalMousePosition(), out var x))
                {
                    QueueRedraw();
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
                float gap2 = 0.8f;
                for (int i = 0; i < customGrid.GridPositions.Count; i++)
                {
                    DrawRect(new Rect2(customGrid.GridPositions[i] + new Vector2(customGrid.CellSize - (customGrid.CellSize * gap), customGrid.CellSize - (customGrid.CellSize * gap)) / 2, customGrid.CellSize * gap, customGrid.CellSize * gap), Color, filled: false, width: 3);
                    var cellValue = customGrid.TryGetItem(customGrid.GridPositions[i], out var x);
                    //drawing filled rect base on grid cell value
                    DrawRect(new Rect2(customGrid.GridPositions[i] + new Vector2(customGrid.CellSize - (customGrid.CellSize * gap2), customGrid.CellSize - (customGrid.CellSize * gap2)) / 2, customGrid.CellSize * gap2, customGrid.CellSize * gap2), cellValue?Colors.Yellow:Colors.Black, filled: true);
                    DrawString(Font, customGrid.GridPositions[i] + new Vector2(customGrid.CellSize/4, customGrid.CellSize / 2), x.ToString(),modulate:Colors.BlueViolet, fontSize: 16);
                }
            }

        }
    }
}
