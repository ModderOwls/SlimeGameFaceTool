using Godot;
using System;
using FaceFiles;

public partial class ToolFaces : Control
{
	ToolFaceFile faceFileInstance;

    public override void _Ready()
    {
		Window root = GetTree().Root;

		//Scale = root.Size / new Vector2(800, 600);

		//Set window settings.
		root.ContentScaleMode = Window.ContentScaleModeEnum.Disabled;
		root.ContentScaleSize = new Vector2I(800, 600);
		root.Size = new Vector2I(800, 600);
		root.Unresizable = false;
		root.AlwaysOnTop = true;

		Size = new Vector2I(800, 600);

		faceFileInstance = ToolFaceFile.Instance;
	}

	public void NewFile()
	{
		faceFileInstance.Reset();

		GetTree().ReloadCurrentScene();
	}

	public void SaveFile()
	{

	}

	public void LoadFile()
	{
		
	}


	[Export] ToolSlideHandler slideHandler;

	public void NextSlide()
	{
		slideHandler.NextSlide();
	}

	public void PreviousSlide()
	{
		slideHandler.PreviousSlide();
	}
}
