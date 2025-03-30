using Godot;
using System;
using FaceFiles;

//Connects the required variables to the main UI.
//This way changing the button is easy without having to worry about the layout of this scene.
public partial class ToolLimbData : Control
{
	public int index;
	public GhostType ghost;

	[ExportGroup("limb selection")]

	[Export] public Button button;
	[Export] public TextureRect textureRect;


	[ExportGroup("ghost selection")]

	[Export] public TextureButton ghostButton;

	public void ConnectSignals(Node connectTo)
	{		
		button.Connect("pressed", Callable.From(() => connectTo.Call("Select", this)));
		ghostButton.Connect("pressed", Callable.From(() => connectTo.Call("PressedGhost", this)));
	}
}
