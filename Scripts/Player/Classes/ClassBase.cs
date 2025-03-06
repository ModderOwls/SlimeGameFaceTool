using Godot;
using System;

public partial class ClassBase : Node2D
{
	/// <summary>
	/// Important to set as it finds the animationtree statemachine using this.
	/// </summary>
	[Export] public string className;

	[ExportSubgroup("Cancellables")]

	/// <summary>
	/// How easily you can break out of an animation. Not related to attack cancelling on hit.
	/// 0: Cannot.
	/// 1: Attacking.
	/// 2: Jumping OR attacking.
	/// 3: All above or by moving.
	/// 4: Not attacking.
	/// </summary>
	private byte _cancelTier = 4;
	[Export] public byte _CancelTier
	{ 
		get { return _cancelTier; }
		set
		{
			_cancelTier = value;

			switch (_cancelTier)
			{
				case 0:
				case 1:
					player?.StateSetCooldown(true, 0, 1, 1);
					break;
				case 2:
					player?.StateSetCooldown(false, 0, 1, 1);
					break;
				default:
					player?.StateSetCooldown(false, 1, 1, 1);
					break;
			}
		} 
	}
	
	/// <summary>
	/// Can you break out of an animation if hitting something.
	/// </summary>
	[Export] public bool _HitCancellable { get; set; }


	[ExportSubgroup("States")]

	[Export] public bool _Invulnerable { get; set; }
	[Export] public byte _Armor { get; set; }


	[ExportSubgroup("References")]

	public PlayerController player;


	public virtual void OnPlayerConnect(PlayerController newPlayer)
	{
		player = newPlayer;
	}


	public virtual void AttackLight(bool pressed) { }

	public virtual void AttackHeavy(bool pressed) { }
}