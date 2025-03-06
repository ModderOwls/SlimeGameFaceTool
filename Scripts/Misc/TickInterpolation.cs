using Godot;
using System;

public partial class TickInterpolation : Node2D
{
	[Export] public Node2D physicsNode;

	Vector2 lastPosition;

	public override void _Process(double delta)
	{
		float physicsInterpF = (float)Engine.GetPhysicsInterpolationFraction();

		Position = lastPosition.Lerp(physicsNode.Position, physicsInterpF);
	}

    public override void _PhysicsProcess(double delta)
    {
		lastPosition = physicsNode.Position;
    }
}
