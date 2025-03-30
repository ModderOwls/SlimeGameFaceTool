using FaceFiles;
using Godot;
using System;

//Uses the entirety of the player prefab (scene in godot) for the sake of accuracy.
public partial class ToolPlayerManagement : Node2D
{
	[Export] public PlayerController player;

	ToolFaceFile faceFileInstance;

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

		ToolFaceFile.Instance.ConnectRefresh(this);
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

	public void Refresh(bool isEmpty)
	{
		faceFileInstance = ToolFaceFile.Instance;

		Node limbsParent = player.face.limbsParent;

		if (limbsParent == null)
		{
			GD.PrintErr("Player face limbs parent is not assigned. Assign it.");

			return;
		}

		//No need to re-instantiate previous ones, just update old ones and only add/remove if necessary.
		while (faceFileInstance.limbs.Count != limbsParent.GetChildCount())
		{
			int childCount = limbsParent.GetChildCount();

			if (faceFileInstance.limbs.Count > childCount)
			{
				Sprite2D limb = new PlayerFaceLimbDefault();
				limbsParent.AddChild(limb);

				limb.RegionEnabled = true;

				limb.SetProcess(true);
				limb.SetPhysicsProcess(true);
			}
			if (faceFileInstance.limbs.Count < childCount)
			{
				limbsParent.GetChild(childCount - 1).Free();
			}
		}

		player.face.GetLimbs();

		for (int i = 0; i < faceFileInstance.limbs.Count; ++i)
		{
			ApplyLimbData(i);
		}

		player.face.EmotionDefault();
	}

	public void ApplyLimbData(int index)
	{
		PlayerFaceLimbDefault limb = player.face.playerLimbs[index] as PlayerFaceLimbDefault;
		LimbData limbData = faceFileInstance.limbs[index];
		
		limb.ghost = player.face.playerGhosts[(int)limbData.ghost];
		limb.flipOnSide = limbData.ghost == GhostType.Eye;

		limb.emotions = new PlayerEmotion[limbData.emotions.Length];

		for (int i = 0; i < limbData.emotions.Length; ++i)
		{
			limb.emotions[i] = ApplyEmotionData(limbData.emotions[i]);

			limb.emotions[i].Initialize(limb);
		}
	}

	public PlayerEmotion ApplyEmotionData(EmotionData data)
	{
		PlayerEmotion emotion;

		//Add behaviour types here if necessary
		switch (data.behaviour)
		{
			case BehaviourType.Heavy:
				emotion = new PlayerEmotionHeavy();
				break;
			default:
				emotion = new PlayerEmotionDefault();
				break;
		}

		CombineImagesDefault(emotion, data);

		emotion.spriteOffset = data.offset;

		return emotion;
	}

	public void CombineImagesDefault(PlayerEmotion emotion, EmotionData data)
    {
        if (data.images == null || data.images.Length == 0) return;

		//Fallback if an image wasn't put in.
		for (int i = 0; i < data.images.Length; ++i)
		{
			if (data.images[i] == null)
			{
				Image fallbackImage = new Image();
				fallbackImage.SetData(1, 1, false, Image.Format.Rgba8, new byte[4]);

				data.images[i] = fallbackImage;
			}
		}

        int width = data.images[0].GetWidth();
        int totalHeight = 0;

        //Calculate total height
        foreach (var img in data.images)
        {
            if (img.GetWidth() > width)
            {
				width = img.GetWidth();
            }

            totalHeight += img.GetHeight();
        }

        Image combinedImage = Image.Create(width, totalHeight, false, Image.Format.Rgba8);
        combinedImage.Fill(Colors.Transparent);

        //Copy each image onto the new Image
        int yOffset = 0;
        for (int i = 0; i < data.images.Length; ++i)
        {
			Image image = data.images[i];

			image.Convert(Image.Format.Rgba8);
            combinedImage.BlitRect(image, new Rect2I(0, 0, image.GetWidth(), image.GetHeight()), new Vector2I(0, yOffset));

			emotion.spritePositions[i] = new Vector2(0, yOffset);
			emotion.spriteSizes[i] = new Vector2(image.GetWidth(), image.GetHeight());

            yOffset += image.GetHeight();
        }

		ImageTexture texture = new ImageTexture();
		texture.SetImage(combinedImage);
		emotion.sprite = texture;

		//Useful for debugging if you suspect images are combined/imported wrong:
		//combinedImage.SavePng("user://yeag" + Time.GetTicksMsec() + ".png");
    }
}
