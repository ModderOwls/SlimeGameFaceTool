using Godot;
using System;
using System.Linq;
using System.Security.Principal;
using System.Collections.Generic;
using Godot.NativeInterop;

public partial class PlayerTailAnimator : Polygon2D
{
	[Export] int tailLength = 20;
	[Export] public Color outline;
	[Export] public Color outlineAntiAliasing;

	public Bone[] bones;

	[ExportGroup("Reel-In")]

	[Export] float reelInSpeed = 120;
	[Export] float reelInStartSpeed = 160;
	float reelInProgress = 1;
	float reelInDistance;

	bool _reelInTail;
	Vector2 oldPosition;

	Vector2 centerPosition;
	Vector2 lightPosition;
	float deltaF;
	float physicsInterpF;

	Vector2 tailRotation;

	Node2D smoothing;
	PlayerController player;
	public Sprite2D sprite;

	Node2D spriteLight;
	
	Vector2[] lastPositions;
	
	public Vector2[] poly = new Vector2[24];
	float[] polyDot = new float[24];
	int polyLengthFourth;

	readonly static Vector2[] stateDefault = 
	{ 
		new Vector2(0, -6),
		new Vector2(-1.56f, -5.79f),
		new Vector2(-3f, -5.2f),
		new Vector2(-4.24f, -4.24f),
		new Vector2(-5.2f, -3f),
		new Vector2(-5.79f, -1.56f),
		new Vector2(-6, 0),
		new Vector2(-5.79f, 1.56f),
		new Vector2(-5.2f, 3f),
		new Vector2(-4.24f, 4.24f),
		new Vector2(-3f, 5.2f),
		new Vector2(-1.56f, 5.79f),
		new Vector2(0, 6),
		new Vector2(1.56f, 5.79f),
		new Vector2(3f, 5.2f),
		new Vector2(4.24f, 4.24f),
		new Vector2(5.2f, 3f),
		new Vector2(5.79f, 1.56f),
		new Vector2(6, 0), 
		new Vector2(5.79f, -1.56f),
		new Vector2(5.2f, -3f),
		new Vector2(4.24f, -4.24f),
		new Vector2(3f, -5.2f),
		new Vector2(1.56f, -5.79f)
	};

	public override void _Ready()
	{
		SetProcess(false);
		SetPhysicsProcess(false);

		smoothing = (Node2D)GetParent().GetParent();
		player = (PlayerController)GetParent().GetParent().GetParent();

		spriteLight = GetNode<Node2D>("LightDot");

		lastPositions = new Vector2[tailLength];
		
		polyLengthFourth = poly.Length/4;
		bones = new Bone[polyLengthFourth+1];
		
		player.tailAnimator = this;
	}

    public override void _Draw()
    {
	    Vector2[] closedPoly = new Vector2[25];
		for (int i = 0; i < poly.Length; ++i){
			closedPoly[i] = poly[i];
		}
		closedPoly[24] = poly[0];
		
		DrawPolyline(closedPoly, outline, 1);
		DrawPolyline(closedPoly, outlineAntiAliasing, .5f);
    }

    public override void _Process(double delta)
	{
		deltaF = (float)delta;

		physicsInterpF = (float)Engine.GetPhysicsInterpolationFraction();

		if (_reelInTail)
		{
			ReelIn();

			return;
		}

		//Set positioning of the tail.
		centerPosition = smoothing.Position + sprite.Offset;
		TextureOffset = -centerPosition + new Vector2(8, 8);

		//try adding like 500 lastPosition's, looks like slime. Could be used for a different visual?

		poly = Polygon;

		ResetVertices(1);

		Vector2 lightLerp = player.velocity/30;
		if (player.velocity.LengthSquared() > 8100)
		{
			lightLerp = player.velocity.Normalized()*3;
		}
		
		lightPosition = lightPosition.Lerp(lightLerp, deltaF*5);

		spriteLight.Scale = Vector2.One * (1 - lightPosition.Length()/14) * .19f;
		spriteLight.Position = centerPosition + lightPosition;

		EffectTail();

		Polygon = poly;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 previousPosition = player.Position + sprite.Offset;

		for (int i = 0; i < lastPositions.Length; ++i)
		{
			Vector2 tempPosition = lastPositions[i];
			lastPositions[i] = previousPosition;
			previousPosition = tempPosition;
		}
	}

	#region SpecialEffects

	void EffectTail()
	{
		//Reset the bonesTail to recalculate it.
		Array.Clear(bones);

		int insideCircle = CalculateBonesFollow();

		//Ensure there is atleast one bone that is outside the slime model.
		if (insideCircle != bones.Length)
		{
			Render(insideCircle);
		}
	}	
	
	void EffectHat()
	{
		//Uses outdated system.
		//Originally added more bones that would add a little hat, neat but hardly worked even back then.

		/*Bone[] bones = new Bone[poly.Length/4+1];

		int polyLengthFourth = poly.Length/4;
		int insideCircle = 0;

		//Make skeleton tail instead of having to calculate it twice for individual vertexes's on opposite sides.
		for (int i = 0; i < bones.Length; ++i)
		{
			float pong = 1-Mathf.Abs((float)i/bones.Length-1);
			float wave = Mathf.Cos(pong * Mathf.Pi) / 2 + .5f;

			Bone bone = GetPointBoneHat(pong, wave);
			bones[i] = bone;

			if ((bone.position-centerPosition).LengthSquared() < 36)
			{
				++insideCircle;
			}
		}

		if (insideCircle != bones.Length)
		{		
			int vertexAmount = poly.Length/2 - insideCircle*2 + 1;
			//int offset = TMod(Mathf.RoundToInt(-Mathf.RadToDeg(meshRotation) / 15 + polyLengthFourth + 0) + insideCircle, poly.Length);

			int offset = 0;
			float offsetDot = 0;
			for (int i = 0; i < polyDot.Length; ++i)
			{
				polyDot[i] = (poly[i] - centerPosition).Normalized().Dot(Vector2.Up);
				if (polyDot[i] > offsetDot)
				{
					offsetDot = polyDot[i];
					offset = i;
				}
			}
			offset = TMod(offset - polyLengthFourth + 2, poly.Length);

			int end = TMod(offset + vertexAmount, poly.Length);

			int halfLength = polyLengthFourth;
			int index = insideCircle;

			//Set vertexes according to the skeleton.
			for (int i = offset; i != end; i = TMod(i + 1, poly.Length))
			{
				int boneIndex = halfLength-Mathf.Abs(halfLength - index);
				int dir = Mathf.Sign(halfLength - index);

				Vector2 newPosition = bones[boneIndex].position - bones[boneIndex].width * dir;

				poly[i] = poly[i].Lerp(newPosition, (polyDot[i]*.7f));
			
				++index;
			}
		}*/
	}

	#endregion

	public void StartJump()
	{
		SetProcess(true);
		SetPhysicsProcess(true);
		
		//GlobalPosition = smoothing.Position + sprite.Offset;

		Vector2 startPos = player.Position + sprite.Offset;
		Vector2 endPos = player.Position + sprite.Offset + Vector2.Down * 8;
		for (int i = 0; i < lastPositions.Length; ++i)
		{
			lastPositions[i] = startPos.Lerp(endPos, (float)i/(lastPositions.Length-1));
		}
		
		ZIndex = 1;

		_reelInTail = false;
		
		poly = Polygon;

		Vector2 spritePosition = player.Position + sprite.Offset;
		for (int i = 0; i < poly.Length; ++i)
		{
			poly[i] = stateDefault[i] + spritePosition;
		}

		Polygon = poly;

		Visible = true;
		spriteLight.Visible = true;
	}

	public void EndJump(){
		ZIndex = -1;

		spriteLight.Visible = false;
		_reelInTail = true;

		oldPosition = centerPosition;

		float tailDistance = 0;
		for (int i = 1; i < bones.Length; ++i)
		{
			tailDistance += bones[i-1].position.DistanceTo(bones[i].position);
		}

		//Set reel-in speeds.
		reelInDistance = tailDistance / tailLength;
		reelInProgress = reelInStartSpeed / tailLength + deltaF * reelInSpeed / reelInDistance;
	}

#region CalculateBones

	/// <summary>
	/// Calculate the according bones to the last few positions, 
	/// and return amount of bones that are inside the slime model.
	/// </summary>
	/// <returns></returns>
	int CalculateBonesFollow()
	{
		int insideCircle = 1;

		bones[0].position = centerPosition;

		//Make skeleton tail instead of having to calculate it twice for individual vertexes's on opposite sides.
		for (int i = 1; i < bones.Length; ++i)
		{
			float pong = 1-Mathf.Abs((float)i/bones.Length-1);
			float wave = Mathf.Cos(pong * Mathf.Pi) / 2 + .5f;

			Bone bone = GetPointBoneTail(pong, wave);
			bones[i] = bone;

			if ((bone.position-centerPosition).LengthSquared() < 36)
			{
				++insideCircle;
			}
		}

		return insideCircle;
	}

	int CalculateBonesReelIn()
	{
		bones[0].position = centerPosition;

		int insideCircle = 1;
		for (int i = 1; i < bones.Length; ++i)
		{
			Vector2 bonePosition = bones[i].position;
			Vector2 lastBonePosition = bones[i-1].position;

			float pong = 1-Mathf.Abs((float)i/bones.Length-1);
			float wave = Mathf.Cos(pong * Mathf.Pi) / 2 + .5f;
			
			float interpolationFactor = deltaF * reelInDistance * reelInProgress;
			bonePosition = bonePosition.Lerp(lastBonePosition, interpolationFactor);
			Vector2 boneWidth = (bonePosition - lastBonePosition).Normalized().Orthogonal() * wave * 6;

			if ((bonePosition-centerPosition).LengthSquared() < 36)
			{
				++insideCircle;
			}

			bones[i].position = bonePosition;
			bones[i].width = boneWidth;
		}

		return insideCircle;
	}

#endregion

	void ResetVertices(float multiplier)
	{
		for (int i = 0; i < poly.Length; ++i)
		{
			poly[i] = centerPosition + stateDefault[i] * multiplier;
		}
	}
	
	/// <summary>
	/// Sets vertices according to the bones variable and how many of them are inside of the circle model.
	/// </summary>
	/// <param name="insideCircle"></param>
	void Render(int insideCircle)
	{
		if (insideCircle >= bones.Length) return;

		//Rotation of the head and the first vertex.
		Vector2 meshRotationPos = bones[insideCircle].position.Lerp(bones[insideCircle-1].position, physicsInterpF);
		float meshRotation = (meshRotationPos-centerPosition).Angle()-Mathf.DegToRad(90);
		
		//Set first and last vertices to cycle through.		
		int vertexAmount = poly.Length/2 - insideCircle*2 + 1;
		int offset = TMod(Mathf.RoundToInt(-Mathf.RadToDeg(meshRotation) / 15 + polyLengthFourth + 0) + insideCircle, poly.Length);
		int end = TMod(offset + vertexAmount, poly.Length);

		//Extra variables for easier reading.
		int halfLength = polyLengthFourth;
		int index = insideCircle;

		//Set vertices according to the skeleton.
		for (int i = offset; i != end; i = TMod(i + 1, poly.Length))
		{
			int boneIndex = halfLength - Mathf.Abs(halfLength - index);
			int dir = Mathf.Sign(halfLength - index);

			Vector2 newPosition = bones[boneIndex].position - bones[boneIndex].width * dir;

			poly[i] = newPosition;
		
			++index;
		}

		tailRotation = bones[0].position - bones[1].position;
	}

	void ReelIn()
	{
		poly = Polygon;

    	reelInProgress += deltaF * reelInSpeed / reelInDistance;

		//Set positioning of the tail.
		centerPosition = smoothing.Position + Vector2.Down*9;
		TextureOffset = -centerPosition + new Vector2(8, 8);

		int insideCircle = CalculateBonesReelIn();

		ResetVertices(0.9f);

		Render(insideCircle);

		if (insideCircle >= bones.Length)
		{
			SetProcess(false);
			SetPhysicsProcess(false);

			Visible = false;
			spriteLight.Visible = false;
		}

		oldPosition = centerPosition;

		Polygon = poly;
	}
	
	//Get the point in the lastPosition array for the tail.
	//pos needs to be 0-1.
	Bone GetPointBoneTail(float pos, float width){
		int arrayLength = lastPositions.Length-1;

		float arrayPos = pos * arrayLength - physicsInterpF;
		float arrayInterp = arrayPos % 1;

		int arrayId1;
		int arrayId2;

		switch (pos){
			case 0:
				arrayId1 = 0;
				arrayId2 = 1;
				arrayInterp = 1;
				break;
			case 1:
				arrayId1 = arrayLength-1;
				arrayId2 = arrayLength;
				arrayInterp = 1;
				break;
			default:
				arrayId1 = Mathf.FloorToInt(arrayPos);
				arrayId2 = arrayId1+1;
				break;
		}

		Vector2 position1 = lastPositions[arrayId1];
		Vector2 position2 = lastPositions[arrayId2];

		Vector2 lerpPosition = position1.Lerp(position2, arrayInterp);
		Vector2 widthPosition = (position2 - position1).Normalized().Orthogonal() * width * 6;
		
    	return new Bone {
        	position = lerpPosition,
        	width = widthPosition
    	};
	}

	Bone GetPointBoneHat(float pos, float width){
		int arrayLength = lastPositions.Length-1;

		float arrayPos = pos * arrayLength;
		float arrayInterp = arrayPos % 1;

		int arrayId1;
		int arrayId2;

		switch (pos){
			case 0:
				arrayId1 = 0;
				arrayId2 = 1;
				arrayInterp = 1;
				break;
			case 1:
				arrayId1 = arrayLength-1;
				arrayId2 = arrayLength;
				arrayInterp = 1;
				break;
			default:
				arrayId1 = Mathf.FloorToInt(arrayPos);
				arrayId2 = arrayId1+1;
				break;
		}

		Vector2 position1 = centerPosition;
		Vector2 position2 = centerPosition + Vector2.Up*16;

		Vector2 lerpPosition = position1.Lerp(position2, arrayInterp);
		Vector2 widthPosition = (position2 - position1).Normalized().Orthogonal() * width * 6;

		Bone bone;
		bone.position = lerpPosition;
		bone.width = widthPosition;
		
		return bone;
	}	

	//True modulus, negatives go to 23 instead of -1 for example.
	int TMod(int number, int arrayLength)
	{
		return ((number % arrayLength) + arrayLength) % arrayLength;
	}
	
	public struct Bone {
		public Vector2 position;
		public Vector2 width;
	}
}

		/*int midIndex = Mathf.FloorToInt((lastPositions.Length-1)*.6f);

		bones[0] = lastPositions[1].Lerp(lastPositions[0], interp);
		bones[1] = lastPositions[midIndex].Lerp(lastPositions[midIndex-1], interp);
		bones[2] = lastPositions[^1].Lerp(lastPositions[^2], interp);;

		Vector2 velNorm = player.vel.Normalized();
		spriteLight.Position = velNorm * 1.5f;
		spriteLight.Scale = new Vector2(.1f + Mathf.Abs(player.vel.X)/2000, .1f + Mathf.Abs(player.vel.Y)/2000);

		poly = Polygon;

		Vector2 velOrthogonal = velNorm.Orthogonal() * 6;
		Vector2 dirOrthogonal = (bones[0] - bones[2]).Normalized().Orthogonal() * 5;

		poly[0] = velOrthogonal;
		poly[4] = -velOrthogonal;

		poly[1] = bones[1] - GlobalPosition + dirOrthogonal;
		poly[3] = bones[1] - GlobalPosition - dirOrthogonal;

		poly[2] = bones[2] - GlobalPosition;*/