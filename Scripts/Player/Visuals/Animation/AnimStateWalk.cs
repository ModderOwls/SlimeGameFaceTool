using Godot;
using System;

public partial class AnimStateWalk : AnimationState
{
	[Export] AnimationState[] animationsNext;
	[Export] ConditionFloat[] conditions;

	public override void Start(PlayerAnimator animator)
	{
		animator.PlayAnimation(animationName);
	}

	public override void Update(PlayerAnimator animator)
	{
		animator.animator.SpeedScale = animator.animationValues["grSpeedAbs"]/148;

		for (int i = 0; i < conditions.Length; ++i)
		{
			ConditionFloat c = conditions[i];
			float greaterOrLesser = (Convert.ToByte(c.greater) - .5f ) * 2;
			if (animator.animationValues[c.conditionVariable] * greaterOrLesser > c.than * greaterOrLesser)
			{
				animator.SetAnimationState(animationsNext[i]);
			}
		}
	}

	public override void End(PlayerAnimator animator)
	{
		animator.animator.SpeedScale = 1;
		animator.PlayAnimation("RESET");
	}
}
