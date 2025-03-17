using Godot;
using System;
using FaceFiles;

public partial class ToolSpriteView : SubViewportContainer
{
	[Export] ToolPlayerManagement management;

	[Export] float defaultSize = 7;
	[Export] Vector2 viewSize;

	[Export] SubViewport viewport;

	bool ghostsEnabled;

    public override void _Draw()
    {
		//Check whether its connected to a player manager.
		if (management == null) return;

		if (ghostsEnabled)
		{
			for (int i = 0; i < management.player.face.playerGhosts.Length; ++i)
			{
				Color color = ToolFaceFile.colorsGhosts[i];
				color.A = 0.7f;

				DrawCircle
				(
					management.player.face.playerGhosts[i].GetScreenTransform().Origin / Scale - GlobalPosition / Scale, 
					1, 
					color
				);
			}
		}
    }

    public override void _Process(double delta)
    {
		QueueRedraw();
    }

    public void SetSize(float newSize)
	{
		Scale = Vector2.One * (defaultSize / newSize);

		Control parent = GetParent() as Control;
		viewport.Size = (Vector2I)(viewSize * newSize).Round();
		Size = viewport.Size;
		PivotOffset = Size/2;
		Position = parent.Size / 2 - Size / 2;
	}

	public void ToggleGhost(bool toggle)
	{
		ghostsEnabled = toggle;
	}
}
