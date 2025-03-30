using Godot;
using System;
using FaceFiles;

//Topmost script, mostly just for handling the main menu and saving/loading.
public partial class ToolFaces : Control
{
	ToolFaceFile faceFileInstance;

	[Export] FileDialog saveDialog;
	[Export] FileDialog loadDialog;

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

		faceFileInstance.RequestLoad();
	}

	public void OpenSaveDialog()
	{
		//Ensure load dialog isnt already open.
		if (!loadDialog.Visible)
		{
			saveDialog.Visible = true;
		}
	}

	public void OpenLoadDialog()
	{
		//Ensure save dialog isnt already open.
		if (!saveDialog.Visible)
		{
			loadDialog.Visible = true;
		}
	}

	public void SaveFile(string path)
	{
		faceFileInstance.SaveToJSON(path);
	}

	public void LoadFile(string path)
	{
		faceFileInstance.LoadFromJSON(path);
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
