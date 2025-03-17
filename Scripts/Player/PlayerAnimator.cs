using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class PlayerAnimator : Node
{
	[Export] Node animationStateStart;
	public AnimationState animationState;

	public int flipNullifierAnim;


	/// <summary>
	/// Contains useful info for animation states.
	/// Used alot in AnimStateCondition.
	/// </summary>
	[Export] public Dictionary<string, float> animationValues =
	new()
    {
		{"grounded", 1},
		{"turn", 0},
		{"grSpeedAbs", 0},
		{"inputHorAbs", 0},
	};


	/// <summary>
	/// Generally dont recommend for readability,
	/// but can be used for outside scripts as searching for the node each time is slow.
	/// 0: Default player animations.
	/// 1: Class animations.
	/// </summary>
	public AnimationStateBundle[] bundles;


	[ExportSubgroup("SpriteInfo")]

	private Vector2 animationOffset;
	[Export] public Vector2 AnimationOffset
	{
		get { return animationOffset; }
		set
		{
			animationOffset = value;

			if (sprite == null) return;

			sprite.Offset = new Vector2(animationOffset.X * (Convert.ToByte(!sprite.FlipH)-.5f) * 2, animationOffset.Y);
		}
	}


	[ExportSubgroup("References")]

	public AnimationPlayer animator;

	public PlayerController player;
	[Export] public PlayerFace face;
	[Export] public Sprite2D sprite;
	
	public PlayerTailAnimator tailAnimator;


	public override void _Ready()
	{
		if (Engine.IsEditorHint()) return;

		animator = GetNode<AnimationPlayer>("AnimationPlayer");

		GetBundles();

		AnimationState _state = (AnimationState)animationStateStart;
		_state.Start(this);

		SetAnimationState((AnimationState)animationStateStart);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Engine.IsEditorHint()) return;

		animationValues["grounded"] = Convert.ToByte(player._Grounded);
		animationValues["turn"] = Convert.ToByte(player._turn);
		animationValues["inputHorAbs"] = Mathf.Abs(player.inputHorizontal);
		animationValues["grSpeedAbs"] = Mathf.Abs(player.grSpeed);

		animationState.Update(this);
	}

	void GetBundles()
	{
		Node statesParent = GetNode("States");
		if (statesParent == null) 
		{
			GD.PrintErr("No 'States' parent was found. Statemachine will be turned off.");

			SetPhysicsProcess(false);
			
			return;
		}

		bundles = new AnimationStateBundle[statesParent.GetChildCount()];
		for (int i = 0; i < statesParent.GetChildCount(); i++)
		{
			AnimationStateBundle statesBundle = statesParent.GetChild<AnimationStateBundle>(i);
			
			if (statesBundle == null)
			{
				GD.PrintErr("States child '", i, "' has no Bundle script. Statemachine will be turned off.");

				SetPhysicsProcess(false);

				return;
			}

			bundles[i] = statesBundle;
		}
	}

	public void SetAnimationState(AnimationState state)
	{
		animationState?.End(this);

		animationState = state;

		state.Start(this);
		state.Update(this);
	}

	public void SetAnimationState(int bundle, int index)
	{
		AnimationState state = bundles[bundle].states[index];
		
		animationState?.End(this);
		
		animationState = state;

		state.Start(this);
		state.Update(this);
	}

	public void TurnFlip()
	{
		sprite.FlipH = Convert.ToBoolean(Mathf.Sign(player.inputHorizontal) + 1);
	}
	
	public void PlayAnimation(string name)
	{
		animator.Stop();

		animator.Play(name);

		//GD.Print("Playing: " + name);
    }

	public void SetAnimationTime(string animation, float time)
	{
		animator.Seek(time, false);
	}

	public void SetAnimationTimeNormalized(string animation, double normalizedTime)
	{
		double realTime = 0;

		if (normalizedTime != 0)
		{
			realTime = animator.CurrentAnimationLength * normalizedTime;
		}

		animator.Play(animation);
		animator.Seek(realTime);
	}
}
