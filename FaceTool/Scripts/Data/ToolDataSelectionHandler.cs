using Godot;
using System;
using System.Collections.Generic;

//Alternative to ToolSelectionData that instead stores the prefabs with given type.
public partial class ToolDataSelectionHandler<T> : ToolSelectionHandler
{
    public List<T> allData = new List<T>();

    public override Control AddPrefab()
    {
        Control obj = base.AddPrefab();

        //Ensure new data is in correct position.
        allData.Insert(obj.GetIndex(), (T)(object)obj);

        return obj;
    }

    public override void RemovePrefabAt(int index)
    {
        base.RemovePrefabAt(index);

        if (current >= allData.Count) return;

        allData.RemoveAt(index);
    }
}
