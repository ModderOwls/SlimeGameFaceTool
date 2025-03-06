using Godot;
using System;

public partial class AnimStateAtEnd : AnimationState
{
    [Export] AnimationState nextAnimation;

	public override void Start(PlayerAnimator animator)
	{
		animator.PlayAnimation(animationName);
	}

	public override void Update(PlayerAnimator animator)
	{
		if (animator.animator.CurrentAnimationPosition >= animator.animator.CurrentAnimationLength)
		{
			animator.SetAnimationState(nextAnimation);
		}
	}

	public override void End(PlayerAnimator animator)
	{
		
	}
}
