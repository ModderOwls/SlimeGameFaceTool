using Godot;
using System;

public partial class CameraHandler : Node2D
{
	[Export] public float betterZoom = 1;
	Vector2I screenSize = new Vector2I(384, 216);

	Camera2D camera;

    public override void _Ready()
    {
		camera = GetNode<Camera2D>("Camera2D");

    }

    public override void _Process(double delta)
	{
		//GetViewport().GetWindow().Size = new Vector2I(Mathf.RoundToInt(screenSize.X / betterZoom), Mathf.RoundToInt(screenSize.Y / betterZoom));
	}
}
