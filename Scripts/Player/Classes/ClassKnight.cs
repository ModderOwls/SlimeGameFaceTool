using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ClassKnight : ClassBase
{
	private bool _smooth = true;
	[Export] public bool _Smooth
	{
		get { return _smooth; }
		set
		{
			_smooth = value;

			if (!_smooth || player == null) return;

			Vector2 followPosition = player.Position + player.bodyOffset;
			followPosition += Vector2.Left*(direction-0.5f)*(8/(1+Convert.ToByte(!player._Grounded)));

			sprite.GlobalPosition = followPosition;

			sprite.GlobalRotation = -direction * Mathf.Pi / 2;

			sprite.Scale = Vector2.One;
		}
	}

	private bool _onTop;
	[Export] public bool _OnTop
	{
		get { return _onTop; }
		set 
		{
			_onTop = value;

			if (sprite == null) return;

			sprite.ZIndex = -5 + 10 * Convert.ToByte(_onTop);
		}
	}


	[ExportGroup("Attacks")]

	[ExportSubgroup("Light")]

	[Export] public AttackData[] attacksLight;
	[Export] public AttackData[] attacksAirLight;

	[ExportSubgroup("Heavy")]
	
	[Export] public AttackData[] attacksHeavy;
	[Export] public AttackData[] attacksAirHeavy;


	[ExportGroup("Players")]

	Sprite2D sprite;
	Node2D attackHandler;
	ClassAnimator animator;


	[ExportGroup("Input")]

    readonly static byte[] keypadXFlip = { 0, 3, 2, 1, 6, 5, 4, 9, 8, 7, 10, 11, 12 };
	int direction;

	float deltaF;
	

	public override void _Ready()
	{
		SetProcess(false);
		SetPhysicsProcess(false);

		GetParent()?.CallDeferred("SetClass", this);

		attackHandler = GetNode<Node2D>("AttackHandler");
		animator = GetNode<ClassAnimator>("Animation");
		sprite = GetNode<Sprite2D>("Animation/Sprite");
	}

	public override void OnPlayerConnect(PlayerController newPlayer)
	{
        base.OnPlayerConnect(newPlayer);

		SetProcess(true);
		SetPhysicsProcess(true);
	}

	public override void _Process(double delta)
	{
		deltaF = (float)delta;

		if (_smooth)
		{
			MoveIdle();

			return;
		}

		sprite.Position = GlobalPosition;

		sprite.Rotation = GlobalRotation;

		sprite.Scale = new Vector2(-player.facing, 1);
	}

	void MoveIdle()
	{
		Vector2 followPosition = player.Position + player.bodyOffset;
		followPosition += Vector2.Left*(direction-0.5f)*(8/(1+Convert.ToByte(!player._Grounded)));

		Vector2 smoothingPosition = sprite.GlobalPosition.Lerp(followPosition, deltaF*13);


		sprite.GlobalPosition = smoothingPosition;

		sprite.GlobalRotation = Mathf.LerpAngle(sprite.GlobalRotation, -direction * Mathf.Pi / 2, deltaF * 8);
	}

    public override void _PhysicsProcess(double delta)
    {
		direction = (player.facing+1)/2;

		attackHandler.Scale = new Vector2(-player.facing, 1);

		Position = player.bodyOffset;
    }

	public void CenterSword()
	{
		sprite.GlobalPosition = Position + Vector2.Left*(direction-0.5f)*(8/(1+Convert.ToByte(!player._Grounded)));

		sprite.GlobalRotation = -direction * Mathf.Pi / 2;
	}

	public override void AttackLight(bool pressed)
	{
		if (!pressed) return;

		if (_CancelTier == 0) return;

		if (!player._Grounded)
		{
			SearchInput(attacksAirLight, player.inputHistory, player.inputHistoryTimes);

			return;
		}
		
		SearchInput(attacksLight, player.inputHistory, player.inputHistoryTimes);
	}

	public override void AttackHeavy(bool pressed)
	{
		if (!pressed) return;

		if (_CancelTier == 0) return;
	}

	public void SearchInput(AttackData[] data, byte[] history, float[] times)
	{
		if (data.Length == 0) return;

		List<AttackData> dataLeft = new List<AttackData>();
		AttackData attack = data[Convert.ToByte(!player._Grounded)];

		float timeTaken = 0;
		for (int i = 0; i < history.Length; ++i)
		{
			//Stop after hitting neutral.
			if (history[i] == 0) break;

			timeTaken += times[i];

			dataLeft.Clear();

			//Cycle through each Data in list given.
			for (int d = 0; d < data.Length; ++d)
			{
				//Get input from point in History.
				byte input = data[d].inputCombo[^(i+1)];

				//Check if input was correct, or flipped.
				if (input == history[i] || keypadXFlip[input] == history[i])
				{
					//Check if input combo is at the end.
					if (data[d].inputCombo.Length - 1 == i)
					{
						//Check if input was fast enough.
						if (timeTaken <= data[d].inputTime)
						{
							//Input was successful, 
							//but keep cycling to see if there's a longer match.
							attack = data[d];
						}
					} 
					else
					{
						//If not, add for later cycling.
						dataLeft.Add(data[d]);
					}
				}
			}

			data = dataLeft.ToArray();
		}

		animator.PlayAnimation(attack.animation);
		player?.animator.SetAnimationState(1, attack.stateBundleIndex);
	}
}