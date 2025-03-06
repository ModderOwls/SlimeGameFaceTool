using Godot;
using System;

public partial class AttackData : Resource
{
	[Export] public byte[] inputCombo;
	[Export] public string animation;
	[Export] public int stateBundleIndex;
	[Export] public string playerAnimation;

	[Export] public bool grounded;
	[Export] public ushort state;
	[Export] public float inputTime;

	public AttackData() {}
}


/// <summary>
/// Uses fighting game keypad annotations.
/// </summary>
[Flags]
public enum InputDirection
{
	Empty = 0,
	DownLeft = 1,
	Down = 2,
	DownRight = 3,
	Left = 4,
	Neutral = 5,
	Right = 6,
	UpLeft = 7,
	Up = 8,
	UpRight = 9
}