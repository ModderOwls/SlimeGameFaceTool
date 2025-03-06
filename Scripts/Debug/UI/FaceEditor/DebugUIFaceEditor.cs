using Godot;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

public partial class DebugUIFaceEditor : Control
{
	[Export] public PlayerFace face;
	[Export] public DebugUI mainUI;
	[Export] DebugUIFaceEditorDraw drawing;


	[ExportGroup("Values")]

	bool circularLast;
	Emotion emotionLast;
	float glanceLast;
	float facingLast;


	[ExportGroup("References - Variables")]

	[Export] Control ui;
	
	[ExportSubgroup("LimbContainers")]

	[Export] GridContainer gridLimb;
	[Export] PackedScene PrefabLimbContainer;

	[ExportSubgroup("Search")]

	[Export] Label labelPlayersFound;
	[Export] SpinBox spinPlayer;
	
	[ExportSubgroup("Variables")]

	[Export] OptionButton optionEmotion;
	[Export] Label labelEmotionTimer;
	[Export] Label labelLook;
	[Export] Slider sliderGlance;
	[Export] Slider sliderFacing;
	[Export] Label labelState;
	[Export] Button buttonCircular;

	[ExportSubgroup("Controls")]

	[Export] Button buttonMouse;
	[Export] Button buttonCenter;
	Node2D centerNode;
	Node2D centerOld;

	
	[ExportGroup("References - Viewables")]

	//[ExportSubgroup("Limbs")]

	[Export] HBoxContainer viewables;
	[Export] Button buttonViewables;
	
	[ExportSubgroup("Ghosts")]

	[Export] GridContainer gridGhost; 
	[Export] PackedScene PrefabGhostContainer;


    public override void _Process(double delta)
    {
		drawing.QueueRedraw();

		if (face == null || !Visible) return;

		//Update Emotion.
		if (emotionLast != face.emotion)
		{
			optionEmotion.Selected = (int)face.emotion;

			emotionLast = face.emotion;
		}

		//Update EmotionTimer, Look and State.
		labelEmotionTimer.Text = (Mathf.Round(face.emotionTimer*100)/100).ToString();
		labelLook.Text = new Vector2(Mathf.Round(face.lookDirection.X*100)/100, Mathf.Round(face.lookDirection.Y*100)/100).ToString();
		labelState.Text = face.effectName;

		//Update Circular.
		if (circularLast != face.circular)
		{
			buttonCircular.SetPressedNoSignal(face.circular);

			circularLast = face.circular;
		}

		//Update Glance.
		if (glanceLast != face.Glance)
		{
			sliderGlance.SetValueNoSignal(face.Glance);

			glanceLast = face.Glance;
		}

		//Update Facing.
		if (facingLast != face.facing)
		{
			sliderFacing.SetValueNoSignal(face.facing);

			facingLast = face.facing;
		}
    }


	public override void _UnhandledInput(InputEvent input)
	{
		if (!ui.Visible) return;

		if (input.IsActionPressed("debugViewables"))
		{
			buttonViewables.ButtonPressed = !buttonViewables.ButtonPressed;
		}
	}


	#region ControlNodeCalls

	public void ItemEmotion(int value)
	{
		switch (value)
		{
			case 0:
				face.EmotionDefault();
				break;
			case 1:
				face.EmotionBlink(100);
				break;
			case 2:
				face.EmotionAnger(100);
				break;
		}
	}

	public void SliderGlance(float glance)
	{
		face.Glance = glance;
	}

	public void SliderFacing(float facing)
	{
		face.facing = facing;
	}

	public void ToggledCircular(bool toggle)
	{
		face.circular = toggle;
	}

	public void ToggledCenter(bool toggle)
	{
		if (toggle)
		{
			if (centerNode == null) centerNode = new Node2D();

			centerOld = face.target;
			face.target = centerNode;

			return;
		}

		face.target = centerOld;
	}

	public void ToggledViewables(bool toggle)
	{
		drawing._viewables = toggle;

		viewables.Visible = toggle;
	}

	public void ToggledViewLimbs(bool toggle)
	{
		foreach (PlayerFaceLimb l in face.playerLimbs)
		{
			l.Visible = toggle;
		}
	}

	public void ToggledViewLimbInfo(bool toggle)
	{
		gridLimb.Visible = toggle;
	}

	public void ToggledViewGhosts(bool toggle)
	{
		drawing._ghostsToggled = toggle;

		if (!drawing._ghostInfoToggled) return;

		gridGhost.Visible = toggle;
	}
	
	public void ToggledViewGhostInfo(bool toggle)
	{
		drawing._ghostInfoToggled = toggle;

		if (!drawing._ghostsToggled) return;

		gridGhost.Visible = toggle;
	}

	public void SliderViewGhostOpacity(float value)
	{
		drawing.colorsGhostsOpacity = value;
	}

	public void ToggledViewLookDirection(bool toggle)
	{
		drawing._lookDirectionToggled = toggle;
	}

	public void ToggledViewGlance(bool toggle)
	{
		drawing._glanceToggled = toggle;
	}

	public void SliderViewLookOpacity(float value)
	{
		drawing.colorLookDirection.A = value;
		drawing.colorGlance.A = value;
	}

	#endregion


	#region DebugUICalls

	#region Wrappers

	//Wrapper for main UI.
    public void SearchPlayers()
	{
		mainUI.SearchPlayers();
	}

	public void ChangePlayer(float value)
	{
		mainUI.ChangePlayer(value);
	}

	#endregion

	
	public void UpdatePlayerCount()
	{
		labelPlayersFound.Text = "Players found: " + mainUI.players.Count;
		spinPlayer.MaxValue = mainUI.players.Count;

		if (mainUI.players.Count == 0 || spinPlayer.Value != 0) return;

		spinPlayer.Value = 1;
	}

	void OnPlayerFound()
	{
		face = mainUI.player.face;

		foreach (Node l in gridLimb.GetChildren())
		{
			l.QueueFree();
		}

		foreach (Node g in gridGhost.GetChildren())
		{
			g.QueueFree();
		}

		if (face == null) return;

		optionEmotion.Clear();
		for (int i = 0; i < Enum.GetNames(typeof(Emotion)).Length; ++i)
		{
			optionEmotion.AddItem(Enum.GetName(typeof(Emotion), i));
		}

		for (int i = 0; i < face.playerLimbs.Length; ++i)
		{
			Node node = PrefabLimbContainer.Instantiate();
			gridLimb.AddChild(node);

			DebugUIFaceLimbContainer container = node as DebugUIFaceLimbContainer;
			container.SetLimbContainer(face.playerLimbs[i]);
		}

		for (int i = 0; i < face.playerGhosts.Length; ++i)
		{
			Node node = PrefabGhostContainer.Instantiate();
			gridGhost.AddChild(node);

			DebugUIFaceGhostContainer container = node as DebugUIFaceGhostContainer;
			container.SetGhostContainer(face.playerGhosts[i], DebugUIFaceEditorDraw.colorsGhosts[i]);
		}
	}

	public void OnPlayerLost()
	{
		OnPlayerFound();
	}

	#endregion
}
