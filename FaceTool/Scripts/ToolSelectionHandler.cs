using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

//General handler for any kind of UI selector across multiple buttons.
//Couldn't find anything like it in the Godot UI/Control nodes.
//Uses child indexes instead of separate variables to prevent desyncs, but can be overwritten if need be.
public partial class ToolSelectionHandler : Control
{
	public int current = -1;


	[ExportGroup("Settings")]

	[Export] protected int startWith = 0;

	[Description("Adds an offset when adding new prefab instances.")]
	[Export] protected int offset;

	
	[ExportGroup("References")]

	[Export] protected Control selectionParent;
	[Export] protected Control selector;


	[ExportGroup("Prefabs")]

	[Export] protected PackedScene prefab;


    public override void _Ready()
    {
		//Messing with parents during _Ready() calls is unsafe, so call it next frame. 
		CallDeferred("AddStartWith");
    }

	public void AddStartWith()
	{
		for (int i = 0; i < startWith; ++i)
		{
			AddPrefab();
		}
	}

    public virtual Control AddPrefab()
	{
		Control obj = prefab.Instantiate() as Control;

		//Add it as a child - offset.
		selectionParent.AddChild(obj);
		selectionParent.MoveChild(obj, selectionParent.GetChildCount() - offset - 1);

		obj.CallDeferred("ConnectSignals", this);

		//If nothing is selected, get the new one.
		if (current < 0)
		{
			current = 0;

			Select(obj);

			selector.Visible = true;
		}

		return obj;
	}

	public virtual void RemovePrefabAt(int index)
	{
		//Check if something is selected.
		if (current < 0) return;

		//Remove the object at specified index.
		selectionParent.GetChild(index).QueueFree();
		selectionParent.RemoveChild(selectionParent.GetChild(index));

		int totalCount = selectionParent.GetChildCount() - offset - 1;

		if (totalCount < 0)
		{
			current = -1;

			//Ensure selector doesn't get removed with parent and hide it.
			selector.GetParent().RemoveChild(selector);
			AddChild(selector);
			selector.Visible = false;

			return;
		}

		if (current > totalCount)
		{
			current = selectionParent.GetChildCount() - offset - 1;
		}

		Select(selectionParent.GetChild(current) as Control);
	}
	
	public virtual void Select(Control node)
	{
		//Set currently selected node as this one.
		current = node.GetIndex();

		//Set selection visualizer node as child and place it.
		selector.GetParent().RemoveChild(selector);
		node.AddChild(selector);

		selector.GlobalPosition = node.GlobalPosition;
	}
}
