using Godot;
using System;

[Tool]
[GlobalClass]
public partial class HitboxData : Resource
{
    [Export] public HitCircle[] hits;

    public HitboxData() {}
}
