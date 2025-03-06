using Godot;
using System;

public partial class DebugUIFaceLimbContainer : PanelContainer
{
	public PlayerFaceLimb limb;

	[Export] Label labelName;
	[Export] Label labelGhost;
	[Export] Label labelPosition;
	[Export] Label labelOffset;

	[Export] TextureRect textureSprite;
	[Export] Sprite2D sprite;

	public void SetLimbContainer(PlayerFaceLimb limb)
	{
		this.limb = limb;

		labelName.Text = limb.Name;
		labelGhost.Text = limb.ghost.Name;
	}

	public override void _Process(double delta)
	{
		if (limb != null)
		{
			labelPosition.Text = limb.Position.ToString();
			labelOffset.Text = limb.Offset.ToString();

			sprite.Texture = limb.Texture;
			sprite.RegionRect = limb.RegionRect;

			return;
		}

		GD.PrintErr("Limb not attached to debug UI limb container, this is not supposed to happen.");

		QueueFree();
	}
}
