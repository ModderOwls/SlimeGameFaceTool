using Godot;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[Tool]
public partial class PlayerFaceGhost : Node2D
{
    [Export] public Vector2 offset;
    public Vector2 trueOffset;
}
