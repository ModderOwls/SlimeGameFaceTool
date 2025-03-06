using Godot;
using System;
using System.Linq;

public partial class DebugUI : Control
{
	[Export] ScreenManager screenManager;

	[Export] public PlayerController player;
	[Export] public Godot.Collections.Array<PlayerController> players = new Godot.Collections.Array<PlayerController>();

	[Export] TabBar tabBar;
	[Export] Control tabsParent;

	[Export] public bool debugTime;
	public float debugTimeScale = 0.2f;

	[ExportGroup("References")]

	[Export] Button buttonDebugTime;

	public override void _Ready()
	{
		if (Engine.IsEditorHint()) return;
		
		Scale = Vector2.One * (screenManager.resolutionScreen / new Vector2(1920, 1080));

		CallDeferred("SearchPlayers");
	}

    public override void _Process(double delta)
    {
		if (debugTime)
		{
			Engine.TimeScale = debugTimeScale;
			Engine.PhysicsTicksPerSecond = Mathf.RoundToInt(100 * debugTimeScale);
		}
    }

    public override void _UnhandledInput(InputEvent input)
    {
		if (input.IsActionPressed("uiFullScreen"))
		{
			if (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Windowed) DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
			else DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);			
		}
		else if (input.IsActionPressed("debugTime"))
		{
			debugTime = !debugTime;
			Engine.TimeScale = 1;
			Engine.PhysicsTicksPerSecond = 100;

			buttonDebugTime.SetPressedNoSignal(debugTime);
		}

		input.Dispose();
    }

	public Vector2 GetScreenPosition(Vector2 worldPosition)
	{
		return worldPosition * ((Size / Scale) / screenManager.subView.Size) + Size / Scale / 2 ;
	}

	public void TabChange(int index)
	{
		for (int i = 0; i < tabsParent.GetChildren().Count(); ++i)
		{
			Control child = tabsParent.GetChild(i).GetChild(0) as Control;

			if (i == index) child.Visible = true;
			else child.Visible = false;
		}
	}

	public void SearchPlayers()
	{
		players.Clear();

		Godot.Collections.Array<Node> nodes = GetTree().GetNodesInGroup("LocalPlayer");
		foreach (Node p in nodes)
		{
			players.Add(p as PlayerController);
		}
		
		for (int i = 0; i < tabsParent.GetChildren().Count(); ++i)
		{
			tabsParent.GetChild(i).CallDeferred("UpdatePlayerCount");
		}
	}

	public bool ChangePlayer(float value)
	{
		int id = Mathf.CeilToInt(value);

		if (id <= 0 || id > players.Count)
		{
			player = null;
			
			for (int i = 0; i < tabsParent.GetChildren().Count(); ++i)
			{
				tabsParent.GetChild(i).CallDeferred("OnPlayerLost");
			}

			return false;
		}
		
		player = players[id - 1];

		for (int i = 0; i < tabsParent.GetChildren().Count(); ++i)
		{
			tabsParent.GetChild(i).CallDeferred("OnPlayerFound");
		}

		return true;
	}

	public void SliderTimeChange(float value)
	{
		debugTimeScale = value;
	}

	public void ToggledDebugTime(bool toggle)
	{
		debugTime = toggle;
		Engine.TimeScale = 1;
		Engine.PhysicsTicksPerSecond = 100;
	}

	public void TestInput(InputEvent input)
	{
		GD.Print("Input! " + input.ToString());
	}

	public void TestInput()
	{
		GD.Print("Input!");
	}
}
