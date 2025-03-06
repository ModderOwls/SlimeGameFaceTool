using Godot;
using System;

public partial class ClassAnimator : Node
{
    [Export] public AnimationPlayer animations;

    public override void _Ready()
    {
        animations.AnimationFinished += AnimationEnd;
    }
    
    public void PlayAnimation(string name)
    {
        animations.Stop();
        animations.Play(name);
    }

    public void AnimationEnd(StringName name)
    {
        string nameString = name.ToString();
        
        //'!' Implies it can loop.
        if (nameString.StartsWith('!')) return;

        PlayAnimation("!Idle");

        animations.Advance(0);
    }
}
