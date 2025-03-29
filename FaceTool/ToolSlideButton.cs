using Godot;
using System;

public partial class ToolSlideButton : TextureButton
{
	[Export] public CompressedTexture2D[] textures;

    public override void _Ready()
    {
		MouseEntered += HoverEnter;
		MouseExited += HoverExit;
		ButtonUp += PressUp;
		ButtonDown += PressDown;
    }

    public void OnSlideChanged(int index)
	{
		TextureNormal = textures[index];
	}

	void HoverEnter()
	{
		Modulate = new Color(0.4f, 0.4f, 0.4f, 1);
	}

	void HoverExit()
	{
		Modulate = new Color(0.1f, 0.1f, 0.1f, 1);
	}

	void PressDown()
	{
		Modulate = new Color(1, 1, 1, 1);
	}

	void PressUp()
	{
		if (IsHovered())
		{
			HoverEnter();
		}
		else
		{
			HoverExit();
		}
	}
}
