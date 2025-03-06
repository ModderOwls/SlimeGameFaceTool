using Godot;
using System;

public partial class ConditionFloat : Resource
{
	[Export] public string conditionVariable;
    [Export] public bool greater;
	[Export] public float than;
}
