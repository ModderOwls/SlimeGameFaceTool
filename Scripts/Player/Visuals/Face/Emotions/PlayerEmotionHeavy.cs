using Godot;
using System;
using System.ComponentModel;

[Tool]
public partial class PlayerEmotionHeavy : PlayerEmotion
{
    public override void OnSet(float glance)
    {
        base.OnSet(glance);

        limb.multiplier = new Vector2(0.4f, 1);
    }

    public override void OnRemove()
    {
        base.OnRemove();

        limb.multiplier = Vector2.One;
    }
}
