using Godot;
using System;

[Tool]
[GlobalClass]
public partial class HitCircle : Resource
{
    [Export] public Vector2 position;
    
    [Export] public float radius;

    [Export] public float damage;

    public HitCircle() {}
}