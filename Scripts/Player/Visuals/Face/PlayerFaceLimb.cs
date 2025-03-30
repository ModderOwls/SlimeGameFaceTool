using Godot;
using System;

public abstract partial class PlayerFaceLimb : Sprite2D
{
	[Export] public PlayerFaceGhost ghost;

	public Vector2 truePosition;
	public Vector2I intPosition;

	[Export] public Vector2 multiplier = Vector2.One;

	
	[ExportSubgroup("Emotions")]

	[Export] public PlayerEmotion[] emotions;
	protected Emotion emotion = Emotion.Default;

	protected float glance;


	public abstract void Initialize(Emotion emotion);

    public abstract void ChangeEmotion(Emotion emotion);
	
    public abstract void ChangeGlance(float direction);

	public abstract Emotion GetEmotionValues();
}

public enum Emotion
{
	Default = 0,
	Blinking = 1,
	Anger = 2
}