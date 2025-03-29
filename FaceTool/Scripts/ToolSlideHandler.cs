using Godot;
using System;


public partial class ToolSlideHandler : Control
{
	[Export] int current;


	
	public void NextSlide()
	{
		current = (current + 1) % GetChildCount();

		UpdateSlides();

		EmitSignal("OnSlideChange", current);
	}

	public void PreviousSlide()
	{
		if (current - 1 >= 0) current--;

		UpdateSlides();

		EmitSignal("OnSlideChange", current);
	}

	void UpdateSlides()
	{
		//Set only the current selected child as visible.
		for (int i = 0; i < GetChildCount(); ++i)
		{
			Control node = GetChild(i) as Control;

			if (i == current)
			{
				node.Visible = true;

				if (node.HasMethod("OnSlideChange"))
				{
					node.Call("OnSlideChange");
				}
			}
			else
			{
				node.Visible = false;
			}
		}
	}

	[Signal]
	public delegate void OnSlideChangeEventHandler(int slideIndex);
}
