using Godot;
using System;
using System.Diagnostics;
using System.Transactions;

[Tool]
public partial class PlayerFace : Node2D
{
	[ExportGroup("Emotions")]

	[Export] public Emotion emotion;
	[Export] public float emotionTimer;
	Emotion transitionEmotion;


	[ExportGroup("Values")]

	public Vector2 lookDirection = new Vector2(-3, 0);
	private float glance;
	[Export] public float Glance
	{
		get { return glance * -facing; }
		set
		{
			glance = value;

			if (Engine.IsEditorHint()) return;

			UpdateGlance();
		}
	}
	/// <summary>
	/// Flips glancing direction.
	/// </summary>
	public float facing = -1;
	public bool circular;

	Vector2 facePosition;

	[Export] public Vector2[] followGhostPositions;

	float deltaF;
	float deltaPhysF;


	[ExportGroup("References")]

	[Export] public Node2D target;

	[Export] public PlayerFaceLimb[] playerLimbs;
	[Export] public PlayerFaceGhost[] playerGhosts;

	public delegate void EffectState();
	public EffectState Effect;
	public string effectName;


	#if DEBUG

	[ExportGroup("Debug")]

	[Export] Texture2D[] debugSprite;
	[Export] Vector2[] debugOffset;
	[Export] Color debugColor;
	[Export] Color debugColorTrue;

	[Export] Sprite2D spriteOffset;
	[Export] PlayerAnimator animatorPlayer;


	public override void _Draw()
	{
		if (!Engine.IsEditorHint() || animatorPlayer == null) return;

		if (followGhostPositions.Length < debugSprite.Length) followGhostPositions = new Vector2[debugSprite.Length];

		for (int i = 0; i < debugOffset.Length; ++i)
		{
			Rect2 rect = new Rect2();
			
			Vector2 truePosition = followGhostPositions[i] + Vector2.Down * 7;
			Vector2I intPosition = new Vector2I(Mathf.RoundToInt(truePosition.X), Mathf.RoundToInt(truePosition.Y));
			rect.Position = intPosition + debugOffset[i];
			rect.Size = Vector2.One;

			if (intPosition.X != 0)
			{
				//Flip sprite of limb depending on side of face.
				int signDirection = Mathf.Sign(truePosition.X);
				
				rect.Size = new Vector2(-signDirection , 1);
			}

			rect.Size *= debugSprite[i].GetSize();

			DrawCircle(truePosition, .5f, debugColorTrue);
			
			DrawTextureRect(debugSprite[i], rect, false, debugColor);
		}
	}

	#endif

	
	public override void _Ready()
	{
		if (Engine.IsEditorHint()) return;

		deltaPhysF = (float)GetPhysicsProcessDeltaTime();

		SetEffectDefault();

		InitializeLimbs();
	}


	public override void _Process(double delta)
	{
		if (Engine.IsEditorHint())
		{
			QueueRedraw();

			return;
		}

		deltaF = (float)delta;

		Position = new Vector2(Mathf.Round(target.Position.X), Mathf.Round(target.Position.Y)) + Vector2.Down * 7;

		Effect();

		if (emotion != Emotion.Default)
		{
			emotionTimer -= deltaF;

			if (emotionTimer < 0)
			{
				switch (transitionEmotion)
				{
					case Emotion.Default:
						EmotionDefault();
						break;
					case Emotion.Blinking:
						EmotionBlink();
						break;
				}
			}
		}
	}


#region Limbs

	void InitializeLimbs()
	{
		for (int i =0; i < playerLimbs.Length; ++i)
		{
			playerLimbs[i].Initialize(emotion);
		}
	}

	void UpdateEmotion()
	{
		for (int i = 0; i < playerLimbs.Length; ++i)
		{
			playerLimbs[i].ChangeEmotion(emotion);
		}
	}

	void UpdateGlance()
	{
		for (int i = 0; i < playerLimbs.Length; ++i)
		{
			playerLimbs[i].ChangeGlance(glance);
		}
	}

	public void SetTrueOffset(int ghostId, Vector2 offset)
	{
		playerGhosts[ghostId].trueOffset = new Vector2(offset.X, offset.Y);
	}
	public void SetTrueOffset(int[] ghostId, Vector2[] offset)
	{
		for (int i = 0; i < ghostId.Length; ++i)
		{
			playerGhosts[ghostId[i]].trueOffset = offset[i];
		}
	}

#endregion


#region Effects

	#region Default

	void EffectDefault()
	{
		float faceDistance = 5;

		facePosition = lookDirection;

		facePosition = new Vector2(Mathf.Sin(facePosition.X)*faceDistance, Mathf.Sin(facePosition.Y)*faceDistance);

		for (int i = 0; i < playerGhosts.Length; ++i)
		{
			PlayerFaceGhost ghost = playerGhosts[i];

			playerGhosts[i].Position = ghost.Position.Lerp(facePosition + ghost.offset, deltaF * 5);
		}
	}
	
	public void SetEffectDefault() 
	{
		Effect = EffectDefault;

		effectName = "Default";
	}

	#endregion

	#region FollowGhost

	void EffectFollowGhost()
	{
		int signDirection = -Mathf.Sign(lookDirection.X);

		//if (playerGhosts.Length != 0) return;

		for (int i = 0; i < playerGhosts.Length; ++i)
		{
			playerGhosts[i].Position = new Vector2(followGhostPositions[i].X * signDirection, followGhostPositions[i].Y);
		}
	}

	public void SetEffectFollowGhost() 
	{
		Effect = EffectFollowGhost;

		effectName = "FollowGhost";
	}

	#endregion

	#region ChaseGhost

	Vector2[] chaseGhostFrom = new Vector2[2];
	float chaseTime;
	float chaseTimer;
	void EffectChaseGhost()
	{
		chaseTimer -= deltaF;

		if (chaseTimer < 0)
		{
			SetEffectFollowGhost();

			return;
		}

		int signDirection = Mathf.Sign(lookDirection.X);

		for (int i = 0; i < playerGhosts.Length; ++i)
		{
			playerGhosts[i].Position = chaseGhostFrom[i].Lerp(
				new Vector2(followGhostPositions[i].X * -signDirection, followGhostPositions[i].Y), 
				(chaseTime-chaseTimer)/chaseTime
			);
		}
	}

	public void SetEffectChaseGhost(float chaseTime)
	{
		for (int i = 0; i < playerGhosts.Length; ++i)
		{
			chaseGhostFrom[i] = playerGhosts[i].Position;
		}

		this.chaseTime = chaseTime;
		this.chaseTimer = chaseTime;

		Effect = EffectChaseGhost;

		effectName = "ChaseGhost";
	}

	#endregion

#endregion


#region Emotions

	public void EmotionDefault()
	{
		emotion = Emotion.Default;
		emotionTimer = 0;

		UpdateEmotion();
	}
	
	public void EmotionBlink()
	{
		emotion = Emotion.Blinking;
		emotionTimer = .1f;
		
		transitionEmotion = Emotion.Default;

		UpdateEmotion();
	}
	public void EmotionBlink(float time)
	{
		emotion = Emotion.Blinking;
		emotionTimer = time;

		transitionEmotion = Emotion.Default;

		UpdateEmotion();
	}

	public void EmotionAnger(float time)
	{
		emotion = Emotion.Anger;
		emotionTimer = time;

		transitionEmotion = Emotion.Blinking;

		UpdateEmotion();
	}

#endregion

}
