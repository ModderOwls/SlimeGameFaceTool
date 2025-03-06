using Godot;
using System;
using System.Runtime.InteropServices;

[Tool]
public partial class PlayerFaceLimbDefault : PlayerFaceLimb
{
	[ExportSubgroup("Values")]

	[Export] public Vector2 multiplier = Vector2.One;

	[Export] public bool flipOnSide;

	bool finishedInitialization;

    public override void _Ready()
    {
		if (ghost == null)
		{
			SetProcess(false);
			SetPhysicsProcess(false);

			GD.PrintErr("You have no ghost assigned to a LimbDefault. Either add/connect one or pick a different limb type.");
		}
    }

	public override void Initialize(Emotion emotion)
	{
		for (int i = 0; i < emotions.Length; ++i)
		{
			emotions[i].Initialize(this);
		}

		finishedInitialization = true;

		ChangeGlance(0);
	}

    public override void _Process(double delta)
    {
		if (Engine.IsEditorHint()) return;

		float trueOffsetXFlip = ghost.trueOffset.X * (Convert.ToByte(FlipH)-.5f) * 2;
		intPosition = new Vector2I(Mathf.RoundToInt(ghost.Position.X * multiplier.X + trueOffsetXFlip), Mathf.RoundToInt(ghost.Position.Y * multiplier.Y + ghost.trueOffset.Y));
		Position = intPosition;

		if (intPosition.X != 0)
		{
			//Flip sprite of limb depending on side of face.
			int signDirection = Mathf.Sign(truePosition.X);
			bool direction = Convert.ToBoolean(signDirection+1);
			FlipH = emotions[(int)emotion].SideOverride(direction);

			//Flips offset if flipOnSide is true.
			if (flipOnSide)
			{
				Vector2 offsetEmotion = emotions[(int)emotion].spriteOffset;
				if (intPosition.X > 0)
				{
					Offset = new Vector2(offsetEmotion.X, offsetEmotion.Y);
				}
				else
				{
					Offset = new Vector2(-offsetEmotion.X, offsetEmotion.Y);
				}
			}
		}
		
		emotions[(int)emotion].Update();
    }

    public override void _PhysicsProcess(double delta)
    {
		truePosition = ghost.Position * multiplier;
    }


    public override void ChangeEmotion(Emotion emotion)
	{
		this.emotion = emotion;

		emotions[(int)emotion].OnSet(glance);
	}

	public override void ChangeGlance(float direction)
	{
		//Ensure initialization was finished so all emotions have limbs attached.
		if (!finishedInitialization) return;

		glance = direction;

		emotions[(int)emotion].OnChangeGlance(direction);
	}

	public override Emotion GetEmotionValues()
	{
		return emotion;
	}
}
