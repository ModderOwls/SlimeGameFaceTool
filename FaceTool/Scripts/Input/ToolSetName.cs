
using Godot;
using System;
using FaceFiles;

public partial class ToolSetName : LineEdit
{
    public override void _Ready()
    {
		TextChanged += SetName;

		ToolFaceFile.Instance.ConnectLoad(this);
    }


	public void SetName(string name)
	{
		ToolFaceFile.Instance.name = name;
	}

	public void Load()
	{
		Text = ToolFaceFile.Instance.name;
	}
}
