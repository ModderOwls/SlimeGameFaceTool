using Godot;
using System;
using System.Collections.Generic;
using System.Threading;

public partial class ToolLimbs : ToolSelectionHandler
{
    public List<ToolLimbData> allData = new List<ToolLimbData>();

    public override Control AddPrefab()
    {
        Control obj = base.AddPrefab();

        //Ensure new data is in correct position.
        allData.Insert(obj.GetIndex(), obj as ToolLimbData);

        return obj;
    }

    public override void RemovePrefabAt(int index)
    {
        base.RemovePrefabAt(index);

        if (current < 0) return;

        allData.RemoveAt(index);
    }

    public void PressedGhost(ToolLimbData data)
    {
        //Resets ghost index if reaching max GhostColors length.
        data.currentGhostIndex = (data.currentGhostIndex + 1) % Enum.GetValues(typeof(GhostColors)).Length;;

        switch ((GhostColors)data.currentGhostIndex)
        {
            case GhostColors.Red:
                data.ghostButton.SelfModulate = Colors.Red;
                break;
            case GhostColors.Green:
                data.ghostButton.SelfModulate = Colors.Green;
                break;
        }
    }

    public void Delete()
    {
        RemovePrefabAt(current);
    }
}

public enum GhostColors
{
    Red,
    Green
}