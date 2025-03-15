using Godot;
using System;

public partial class UIMaxSize : Control
{
	[Export] public bool Y;
	[Export] public float maxSize;

	Window root;

	[Export] Label yaba;

    public override void _Ready()
    {
		root = GetTree().Root;
		
		//Listen to size changes in the window.
		GetTree().Root.SizeChanged += OnResize;
    }

	void OnResize()
	{
		float scaleFactor;
		string result = "";

		if (Y)
		{
			scaleFactor = root.Size.Y / 480f;

			if (scaleFactor >= 1)
			{
				Size = new Vector2(Size.X, maxSize / scaleFactor);
				
				result = "yay";
			}
			else
			{
				Size = new Vector2(Size.X, maxSize);

				result = ":(";
			}
		}
		else
		{
			scaleFactor = root.Size.X / 720f;

			if (Size.X * scaleFactor > maxSize)
			{
				Size = new Vector2(maxSize / scaleFactor, Size.Y);
			}
			else
			{
				Size = new Vector2(maxSize, Size.Y);
			}
		}

		yaba.Text = result + " " +(Mathf.Round(scaleFactor)).ToString();
	}
}