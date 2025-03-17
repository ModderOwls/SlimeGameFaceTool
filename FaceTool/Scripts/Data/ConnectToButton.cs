using Godot;
using System;

public partial class ConnectToButton : Button
{
	public void ConnectSignals(Node connectTo)
	{
		Connect("pressed", Callable.From(() => connectTo.Call("Select", this)));
	}
}
