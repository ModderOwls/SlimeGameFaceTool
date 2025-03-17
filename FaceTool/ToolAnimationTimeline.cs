using Godot;
using System;

public partial class ToolAnimationTimeline : HSlider
{
	float currentAnimationLength;

	public void UpdateTimeline(float position)
	{
		SetValueNoSignal(position / currentAnimationLength);
	}

	public void SetTimelineLength(float animationLength)
	{
		currentAnimationLength = animationLength;

		int totalTicks = 0;
		for (int i = 0; i <= animationLength * 10; i++)
		{
			totalTicks = i + 1;
		}

		TickCount = totalTicks;

		SetValueNoSignal(0);
	}
}
