using Godot;
using System;

public partial class ToolSpriteSelection : ToolSelectionHandler
{
    public override void Select(Control node)
    {
        base.Select(node);

		selector.Size = node.Size;
    }
}
