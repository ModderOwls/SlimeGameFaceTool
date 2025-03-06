using Godot;
using System;

public partial class AnimationState : Node
{
	[Export] public string animationName;

	public virtual void Start(PlayerAnimator animator) {}

	public virtual void Update(PlayerAnimator animator) {}

	public virtual void End(PlayerAnimator animator) {}
}
