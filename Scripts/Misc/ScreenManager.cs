using Godot;
using System;
using System.IO;

public partial class ScreenManager : Control
{
	[ExportSubgroup("Vars")]

	[Export] public Vector2I resolutionScreen = new Vector2I(1920, 1080);
	[Export] Vector2 resolutionGame = new Vector2(384, 216);
	float resFactor = 1;
	
	[Export] public Vector2 zoom;

	private int screens = 1;
	[Export]int screensRoot = 1;
	[Export] public int Screens
	{
		get { return screens; }
		set 
		{
			screens = value;

			int newRoot = Mathf.CeilToInt(Mathf.Sqrt(screens));

			if (screensRoot != newRoot)
			{
				screensRoot = newRoot;

				ReorderScreens();
			}
		}
	}

	[Export] PackedScene scene1;


	[ExportSubgroup("References")]

	[Export] public SubViewportContainer subContainer;
	[Export] public SubViewport subView;

	[Export] Camera2D camera;


	public override void _Ready()
	{
		Node sceneNode = scene1.Instantiate();
		subView.AddChild(sceneNode);

		CallDeferred("DelayedReady");
	}

	void DelayedReady()
	{
		GetWindow().Size = resolutionScreen;
		GetWindow().ContentScaleSize = resolutionScreen;

		Size = resolutionScreen;
		resFactor = (float)resolutionScreen.Y/resolutionGame.Y;

		camera.Position = (Vector2)resolutionScreen/2;

		Size = resolutionScreen;
		subContainer.Size = resolutionGame;

		GD.Print(resFactor);

		ResizeScreen();
	}

	//https://forum.godotengine.org/t/pixel-perfect-scaling/28644/4
	public void ResizeScreen()
	{
		Vector2I newRes = new(Mathf.RoundToInt(resolutionGame.X * zoom.X), Mathf.RoundToInt(resolutionGame.Y * zoom.Y));
		subView.Size = newRes;
		subContainer.Scale = Vector2.One * resFactor / (float)screensRoot * (resolutionGame/(Vector2)subView.Size);
		subContainer.Position = Vector2.Zero;
	}

	void ReorderScreens()
	{
		Vector2I newRes = new(Mathf.RoundToInt(resolutionGame.X * zoom.X), Mathf.RoundToInt(resolutionGame.Y * zoom.Y));
		subView.Size = newRes;
		subContainer.Scale = Vector2.One * resFactor / (float)screensRoot * (resolutionGame/(Vector2)subView.Size);
		subContainer.Position = Vector2.Zero;
	}

	/*float time;
	public override void _Process(double delta)
	{
		time += (float)delta;
		zoom = Vector2.One * (.5f + (Mathf.Sin(time*3)+1)*.25f);
		ResizeScreen();
	}*/
}
