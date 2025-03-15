using Godot;
using System;

//Connects the required variables to the main UI.
//This way changing the button is easy without having to worry about the layout of this scene.
public partial class ToolLimbData : Control
{
	public int index;
	public int currentGhostIndex;

	[ExportGroup("limb selection")]

	[Export] public Button button;
	[Export] public TextureRect textureRect;


	[ExportGroup("ghost selection")]

	[Export] public TextureButton ghostButton;

	public void ConnectSignals(Node connectTo)
	{		
		button.Connect("pressed", Callable.From(() => connectTo.Call("Select", this)));
		ghostButton.Connect("pressed", Callable.From(() => connectTo.Call("PressedGhost", this)));

		//Update Ghost button first.
		connectTo.Call("PressedGhost", this);
	}
}
