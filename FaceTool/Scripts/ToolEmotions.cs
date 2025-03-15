using Godot;
using System;

public partial class ToolEmotions : ToolSelectionHandler
{
	public override void _Ready()
	{
		base._Ready();

		startWith = Enum.GetNames(typeof(Emotion)).Length;
	}

    public override Control AddPrefab()
    {
        Button button = base.AddPrefab() as Button;

		button.Text = Enum.GetName(typeof(Emotion), button.GetIndex());

		return button;
    }

    public override void Select(Control node)
    {
        base.Select(node);

		selector.Size = node.Size;
    }
}
