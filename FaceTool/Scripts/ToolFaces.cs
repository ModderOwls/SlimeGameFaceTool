using Godot;
using System;

public partial class ToolFaces : Control
{
    public override void _Ready()
    {
		Window root = GetTree().Root;

		//Scale = root.Size / new Vector2(800, 600);

		//Set window settings.
		root.ContentScaleMode = Window.ContentScaleModeEnum.Disabled;
		root.ContentScaleSize = new Vector2I(800, 600);
		root.Size = new Vector2I(800, 600);
		root.Unresizable = false;
		root.AlwaysOnTop = true;

		Size = new Vector2I(800, 600);
	}
}
