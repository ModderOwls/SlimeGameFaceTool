using Godot;
using System;

public partial class AnimStateJump : AnimationState
{
	[Export] AnimationState[] animationsNext;
	[Export] ConditionFloat[] conditions;

	public override void Start(PlayerAnimator animator)
	{
		animator.PlayAnimation(animationName);
	}

	public override void Update(PlayerAnimator animator)
	{
		if (animator.animationValues["grounded"] == 1)
		{
			for (int i = 0; i < conditions.Length; ++i)
			{
				ConditionFloat c = conditions[i];
				float greaterOrLesser = (Convert.ToByte(c.greater) - .5f ) * 2;
				if (animator.animationValues[c.conditionVariable] * greaterOrLesser > c.than * greaterOrLesser)
				{
					animator.SetAnimationState(animationsNext[i]);

					return;
				}
			}

			GD.PrintErr("Jump animation state hole spotted. Please make it so there's no way to fail all conditions. Defaulting to first animation.");

			animator.SetAnimationState(animationsNext[0]);
		}
	}

	public override void End(PlayerAnimator animator)
	{
		
	}
}
