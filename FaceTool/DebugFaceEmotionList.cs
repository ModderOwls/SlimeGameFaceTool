using Godot;
using System;

public partial class DebugFaceEmotionList : VBoxContainer
{
	[Export] PackedScene prefabEmotionButton;

	public override void _Ready()
	{
		for (int i = 0; i < Enum.GetNames(typeof(Emotion)).Length; ++i)
		{
			Button buttonEmotion = prefabEmotionButton.Instantiate() as Button; 
			
			AddChild(buttonEmotion);
			buttonEmotion.Text = Enum.GetName(typeof(Emotion), i);
		}
	}
}
