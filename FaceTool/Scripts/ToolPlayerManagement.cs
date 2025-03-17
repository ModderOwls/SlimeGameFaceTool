using Godot;
using System;

//Uses the entirety of the player prefab (scene in godot) for the sake of accuracy.
public partial class ToolPlayerManagement : Node2D
{
	[Export] public PlayerController player;

	string animation;

	bool playing;

	bool movePlayer;
	float timePassed;


    public override void _Ready()
    {
		//Wait for all Ready calls to finish at the next frame.
		CallDeferred("InitializePlayer");

		if (player == null)
		{
			GD.PrintErr("Please add a Player to the PlayerManagement node!");

			SetProcess(false);
			SetPhysicsProcess(false);
		}
    }

    public override void _PhysicsProcess(double delta)
    {
		timePassed += (float)delta;

		if (playing) return;

		//Spins the player in a circle, 
		//useful when working with the jump model or particle systems for example.
		if (movePlayer)
		{
			Vector2 normal = new Vector2(Mathf.Sin(timePassed * 4), Mathf.Cos(timePassed * 4));

			player.Position = normal * 24;
			player.face.lookDirection = normal.Orthogonal();

			return;
		}

		player.Position = new Vector2(-12, 14);
    }

    public void InitializePlayer()
	{
		//player.SetProcess(false);
		player.SetPhysicsProcess(false);
		player.SetProcessUnhandledInput(false);

		player.facing = 1;
		player.face.facing = 1;
			
		player.face.lookDirection = Vector2.Right;
		player.sprite.FlipH = true;

		player.Position = new Vector2(-12, 14);

		player.animator.PlayAnimation("Idle");
	}

	//Reactivates player, meaning you can move around yourself.
	public void PlayMode()
	{
		playing = true;

		player.SetPhysicsProcess(true);
		player.SetProcessUnhandledInput(true);

		player.face.SetEffectDefault();
	}

	public void StopPlayMode()
	{
		playing = false;

		InitializePlayer();

		PlayAnimation(animation);
	}

	public void PlayAnimation(string name)
	{
		//Fix: Player does not support all animations activating in the air atm.
		if (playing) return;

		DetectSpecialAnimation(name, animation);

		animation = name;
		
		//Interrupting animations can mess with the face's effects.
		if (IsPlaying()) player.face.SetEffectDefault();

		player.animator.PlayAnimation(name);
	}

	public void SetAnimationTimeNormalized(double time)
	{
		player.animator.SetAnimationTimeNormalized(animation, time);
	}

	public void SetAnimationPaused(bool paused)
	{
		if (paused) player.animator.animator.Pause();
		else player.animator.animator.Play();
	}

	public bool IsPlaying()
	{
		return player.animator.animator.IsPlaying();
	}

	public double GetAnimationPosition()
	{
		return player.animator.animator.CurrentAnimationPosition;
	}

	public double GetAnimationLength()
	{
		return player.animator.animator.CurrentAnimationLength;
	}

	//Special behaviour for specific animations.
	//Not.. great, but it only gets called once, and is only for the tool.
	public void DetectSpecialAnimation(string name, string previousName)
	{
		if (name == previousName || playing) return;

		//If the previous was special, undo its changes.
		switch (previousName)
		{
			case "IdleCentered":
				player.face.lookDirection = Vector2.Right;
				break;
			case "Jump":
				player.tailAnimator.EndJump();

				movePlayer = false;
				player.face.lookDirection = Vector2.Right;
				break;
			case "Walk":
				player.face.SetTrueOffset(0, Vector2.Zero);
				break;
		}

		//Detect the new animation for behaviour.
		switch (name)
		{
			case "IdleCentered":
				player.face.lookDirection = Vector2.Zero;
				break;
			case "Jump":
				player.Position = new Vector2(Mathf.Sin(timePassed * 4) * 24, Mathf.Cos(timePassed * 4) * 24);

				player.tailAnimator.StartJump();

				movePlayer = true;
				break;
		}
	}
}
