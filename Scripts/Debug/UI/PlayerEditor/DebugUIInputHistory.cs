using Godot;
using System;

public partial class DebugUIInputHistory : HBoxContainer
{
	[Export] public Texture2D[] textures;

	public void UpdateHistory(byte[] newHistory)
	{
		for (int i = 0; i < GetChildCount(); ++i)
		{
			TextureRect rect = GetChild(i) as TextureRect;

			rect.Texture = textures[newHistory[i]];
		}
	}
}
