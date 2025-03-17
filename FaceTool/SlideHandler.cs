using Godot;
using System;

public partial class SlideHandler : Control
{
	[Export] int current;
	
	public void NextSlide()
	{
		current = (current + 1) % GetChildCount();

		UpdateSlides();
	}

	public void PreviousSlide()
	{
		current = (current - 1) % GetChildCount();

		UpdateSlides();
	}

	void UpdateSlides()
	{
		//Set only the current selected child as visible.
		for (int i = 0; i < GetChildCount(); ++i)
		{
			Control node = GetChild(i) as Control;

			node.Visible = i == current;
		}
	}
}
