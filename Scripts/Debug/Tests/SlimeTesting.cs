using Godot;
using System;
using System.Collections.Generic;
using System.IO.Pipes;

public partial class SlimeTesting : Polygon2D
{
	[Export] public Color outline;
	[Export] public Color outlineAntiAliasing;

	[Export] public float pullStrength;

	[Export] public float damp;
	[Export] public float bounce;
	[Export] public float adhesive;

	public bool inputHold;

	public Vector2[] poly = new Vector2[24];
	public SlimePoint[] points = new SlimePoint[24];

	Vector2[] polyOut = new Vector2[25];

	int nearest;
	
	readonly static Vector2[] stateDefault = { 
		new Vector2(0, -12),
		new Vector2(-3.11f, -11.58f),
		new Vector2(-6f, -10.39f),
		new Vector2(-8.48f, -8.48f),
		new Vector2(-10.39f, -6f),
		new Vector2(-11.58f, -3.11f),
		new Vector2(-12, 0),
		new Vector2(-11.58f, 3.11f),
		new Vector2(-10.39f, 6f),
		new Vector2(-8.48f, 8.48f),
		new Vector2(-6f, 10.39f),
		new Vector2(-3.11f, 11.58f),
		new Vector2(0, 12),
		new Vector2(3.11f, 11.58f),
		new Vector2(6f, 10.39f),
		new Vector2(8.48f, 8.48f),
		new Vector2(10.39f, 6f),
		new Vector2(11.58f, 3.11f),
		new Vector2(12, 0), 
		new Vector2(11.58f, -3.11f),
		new Vector2(10.39f, -6f),
		new Vector2(8.48f, -8.48f),
		new Vector2(6f, -10.39f),
		new Vector2(3.11f, -11.58f)
	};
	
	//let point next to eachother influence eachother's velocity.
	//then just make dragging (only one vertex at a time) rely on velocity.
	//this way it drags other vertexes with it like with real liquid adhesivity.

    public override void _Ready()
    {
		for (int i = 0; i < points.Length; ++i)
		{
			points[i].position = stateDefault[i];

			if (i == 0){
				points[i].prevId = 23;
				points[i].nextId = 1;
			} else if (i == 23){
				points[i].prevId = 22;
				points[i].nextId = 0;
			} else{
				points[i].prevId = i - 1;
				points[i].nextId = i + 1;
			}
		}
    }

    public override void _Draw()
    {
		polyOut[24] = poly[0];
		DrawPolyline(polyOut, outline, 1);
		DrawPolyline(polyOut, outlineAntiAliasing, .3f);
    }

    public override void _Process(double delta)
    {
		float deltaF = (float)delta;

		for (int i = 0; i < points.Length; ++i)
		{
			Vector2 displacement = points[i].position - stateDefault[i];
			Vector2 adhesivity = points[points[i].nextId].position - points[i].position + (points[points[i].prevId].position - points[i].position);
			
			points[i].velocity -= displacement * bounce * deltaF;
			points[i].velocity *= 1f - damp * deltaF;
			points[i].position += points[i].velocity * deltaF;
			points[i].position += adhesivity * deltaF * adhesive;

			poly[i] = points[i].position;
			polyOut[i] = poly[i];
		}

		if (inputHold)
		{
			/* push mode.
			for (int i = 0; i < points.Length; ++i)
			{
				Vector2 distance = points[i].position - GetGlobalMousePosition();
				float force = 1800 / (1 + distance.LengthSquared());
				points[i].velocity += distance.Normalized() * force * deltaF;
			}*/

			/*float nearestDistance = stateDefault[0].DistanceSquaredTo(GetGlobalMousePosition());
			for (int i = 1; i < points.Length; ++i)
			{
				if (stateDefault[i].DistanceSquaredTo(GetGlobalMousePosition()) < nearestDistance)
				{
					nearest = i;
					nearestDistance = stateDefault[i].DistanceSquaredTo(GetGlobalMousePosition());
				}
			}*/

			Vector2 pull = GetGlobalMousePosition() - points[nearest].position;
			float length = GetGlobalMousePosition().Length();
			points[nearest].velocity += pull * (pullStrength/length) * deltaF;
		}

		Polygon = poly;
    }

	void InputClick(bool pressed){
		if (pressed){
			inputHold = true;

			nearest = 0;
			float nearestDistance = points[0].position.DistanceSquaredTo(GetGlobalMousePosition());
			for (int i = 1; i < points.Length; ++i)
			{
				if (points[i].position.DistanceSquaredTo(GetGlobalMousePosition()) < nearestDistance)
				{
					nearest = i;
					nearestDistance = points[i].position.DistanceSquaredTo(GetGlobalMousePosition());
				}
			}
		} else{
			inputHold = false;

			for (int i = 0; i < points.Length; ++i)
			{
				//points[i].velocity = (stateDefault[i] - points[i].position) * 3;
			}
		}
	}

    public override void _Input(InputEvent evt)
	{
		if (evt.IsEcho() || !evt.IsActionType()) return;

    	// Mouse in viewport coordinates.
    	if (evt.IsAction("playerAction1")){
			InputClick(evt.IsPressed());
		}
	}

	public struct SlimePoint {
		public Vector2 position;
		public Vector2 velocity;

		public int nextId;
		public int prevId;
	}
}
