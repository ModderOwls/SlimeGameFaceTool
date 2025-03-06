using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

public partial class DebugUIPlayerEditor : Control
{
	[Export] public PlayerController player;
	[Export] public DebugUI mainUI;
	[Export] DebugUIPlayerEditorDraw drawing;
	[Export] DebugUIInputHistory inputs;
	
	public PlayerTailAnimator tail;

	[ExportGroup("Values")]

	bool circularLast;
	Emotion emotionLast;
	float glanceLast;
	float facingLast;
	byte inputLast;


	[ExportGroup("References - Stats")]

	[Export] Control ui;

	[ExportSubgroup("Search")]

	[Export] Label labelPlayersFound;
	[Export] SpinBox spinPlayer;
	
	[ExportSubgroup("Variables")]

	[Export] Label labelPosition;
	[Export] Label labelVelocity;
	[Export] Label labelAngle;
	[Export] Button buttonOnGround;

	[ExportGroup("References - Viewables")]

	[Export] HBoxContainer viewables;
	[Export] Button buttonViewables;

	[ExportSubgroup("Stats")]

	[Export] Control statParent;
	[Export] SpinBox spinAcceleration;
	[Export] SpinBox spinDeceleration;
	[Export] SpinBox spinRunSpeed;
	[Export] SpinBox spinCapSpeed;
	[Export] SpinBox spinFriction;
	[Export] SpinBox spinAirAcceleration;
	[Export] SpinBox spinJumpForce;
	[Export] SpinBox spinGravity;

	[ExportSubgroup("States")]

	[Export] Control stateParent; //Parent of entire state display system
	[Export] Control statesParent; //Parent of states only
	[Export] PackedScene prefabState;
	[Export] Label labelStateIndex;
	[Export] Label labelStateTimer;
	int stateChildOffset = 3;


    public override void _Process(double delta)
    {
		drawing.QueueRedraw();

		if (player == null || !Visible) return;

		labelPosition.Text = ((player.Position*10).Round()/10).ToString();
		labelVelocity.Text = ((player.velocity*10).Round()/10).ToString();
		labelAngle.Text = (Mathf.Round(player.grAngle*10)/10).ToString();
		buttonOnGround.SetPressedNoSignal(player._Grounded);

		if (statParent.Visible)
		{
			spinAcceleration.Value = player.acceleration;
			spinDeceleration.Value = player.deceleration;
			spinRunSpeed.Value = player.runSpeed;
			spinCapSpeed.Value = player.capSpeed;
			spinFriction.Value = player.friction;
			spinAirAcceleration.Value = player.airAcceleration;
			spinJumpForce.Value = player.jumpForce;
			spinGravity.Value = player.gravity;
		}

		if (drawing._statesToggled)
		{
			//Update all state labels.
			UpdateStates();
		}

		if (inputLast != player.inputHistory[0])
		{
			inputLast = player.inputHistory[0];

			inputs.UpdateHistory(player.inputHistory);
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


	void UpdateStates()
	{
		labelStateIndex.Text = player._index.ToString();
		labelStateTimer.Text = (Mathf.Round(player._timer*100)/100).ToString();

		for (int i = 0; i < listFieldsPlayer.Count; ++i)
		{
			Label stateNode = statesParent.GetChild(i + stateChildOffset) as Label;
			stateNode.Visible = (bool)listFieldsPlayer[i].GetValue(player);
		}
	}

	#region ControlNodeCalls

	public void ToggledViewables(bool toggle)
	{
		drawing._viewables = toggle;

		viewables.Visible = toggle;
	}

	public void ToggledStatInfo(bool toggled)
	{
		statParent.Visible = toggled;
	}

	public void PressedStatReset()
	{
		spinAcceleration.Value = 7.2f;
		spinDeceleration.Value = 48f;
		spinRunSpeed.Value = 130f;
		spinCapSpeed.Value = 720f;
		spinFriction.Value = 4.8f;
		spinAirAcceleration.Value = 6f;
		spinJumpForce.Value = 225f;
		spinGravity.Value = 4.8f;
	}

	List<FieldInfo> listFieldsPlayer = new List<FieldInfo>();
	public void ToggledViewStates(bool toggled)
	{
		drawing._statesToggled = toggled;

		stateParent.Visible = toggled;

		if (!toggled)
		{
			for (int i = stateChildOffset; i < statesParent.GetChildCount(); ++i)
			{
				statesParent.GetChild(i).QueueFree();
			}

			return;
		}

		listFieldsPlayer.Clear();

		//Returns all public fields in PlayerController that start with "_".
		foreach (FieldInfo p in player.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
		{
			if (p.Name.StartsWith('_'))
			{
				if (p.FieldType == typeof(bool))
				{
					listFieldsPlayer.Add(p);
				}
			}
		}

		//Add all boolean states and hide them if false.
		for (int i = 0; i < listFieldsPlayer.Count; ++i)
		{
			Label instance = prefabState.Instantiate() as Label;
			statesParent.AddChild(instance);

			instance.Text = listFieldsPlayer[i].Name;
			instance.Visible = (bool)listFieldsPlayer[i].GetValue(player);
		}
	}

	public void ToggledTailBones(bool toggled)
	{
		drawing._tailBonesToggled = toggled;
	}

	public void ToggledTailVertices(bool toggled)
	{
		drawing._tailVerticesToggled = toggled;
	}

	public void SliderViewTailOpacity(float value)
	{
		drawing.colorsTailOpacity = value;
	}

	public void ToggledPhysicsRays(bool toggled)
	{
		drawing._raysToggled = toggled;
	}
	
	public void SliderViewPhysicsOpacity(float value)
	{
		drawing.colorsPhysicsOpacity = value;
	}

	#region SpinboxStats

	public void SpinStatAcceleration(float value)
	{
		player.statAcceleration = value;
		player.acceleration = value;
	}

	public void SpinStatDeceleration(float value)
	{
		player.statDeceleration = value;
		player.deceleration = value;
	}

	public void SpinStatRunSpeed(float value)
	{
		player.statRunSpeed = value;
		player.runSpeed = value;
	}

	public void SpinStatCapSpeed(float value)
	{
		player.statCapSpeed = value;
		player.capSpeed = value;
	}

	public void SpinStatFriction(float value)
	{
		player.statFriction = value;
		player.friction = value;
	}

	public void SpinStatAirAcceleration(float value)
	{
		player.statAirAcceleration = value;
		player.airAcceleration = value;
	}

	public void SpinStatJumpForce(float value)
	{
		player.statJumpForce = value;
		player.jumpForce = value;
	}

	public void SpinStatGravity(float value)
	{
		player.statGravity = value;
		player.gravity = value;
	}

	#endregion

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

	public void OnPlayerFound()
	{
		player = mainUI.player;

		tail = player.tailAnimator;
		
		ToggledViewStates(true);
	}

	public void OnPlayerLost()
	{
		OnPlayerFound();
	}

	#endregion
}
