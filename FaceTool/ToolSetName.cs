
using Godot;
using System;
using FaceFiles;

public partial class ToolSetName : LineEdit
{
	public void SetName(string name)
	{
		ToolFaceFile.Instance.name = name;
	}
}
