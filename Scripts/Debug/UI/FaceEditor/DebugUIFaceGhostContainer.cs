using Godot;
using System;

public partial class DebugUIFaceGhostContainer : PanelContainer
{
	public PlayerFaceGhost ghost;

	[Export] Label labelName;
	[Export] Label labelPosition;
	[Export] Label labelOffset;
	[Export] Label labelTrueOffset;
	[Export] ColorRect colorRect;

	public void SetGhostContainer(PlayerFaceGhost ghost, Color color)
	{
		this.ghost = ghost;

        labelName.Text = this.ghost.Name;
		color.A = 0.6f;
		colorRect.Color = color;
	}

	public override void _Process(double delta)
	{
		if (ghost != null)
		{
			labelPosition.Text = ghost.Position.Round().ToString();
			labelOffset.Text = ghost.offset.ToString();
			labelTrueOffset.Text = ghost.trueOffset.ToString();

			return;
		}

		GD.PrintErr("Limb not attached to debug UI limb container, this is not supposed to happen.");

		QueueFree();
	}
}
