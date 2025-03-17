using Godot;
using System;
using FaceFiles;

public partial class ToolOffset : VBoxContainer
{
	Vector2 offset;

	[Export] SpinBox spinOffsetX;
	[Export] SpinBox spinOffsetY;

	ToolFaceFile faceFileInstance;


    public override void _Ready()
    {
		faceFileInstance = ToolFaceFile.Instance;

		faceFileInstance.ConnectRefresh(this);
    }

    public void Refresh(bool isEmpty)
	{
        if (isEmpty) return;

		offset = faceFileInstance.GetCurrentEmotionData().offset;

		spinOffsetX?.SetValueNoSignal(offset.X);
		spinOffsetY?.SetValueNoSignal(offset.Y);
	}

	public void SetX(float newX)
	{
		offset.X = newX;

		UpdateFileOffset();
	}

	public void SetY(float newY)
	{
		offset.Y = newY;

		UpdateFileOffset();
	}

	public void UpdateFileOffset()
	{
		EmotionData data = faceFileInstance.GetCurrentEmotionData();

		if (data == null) return;
		
		data.offset = offset;
	}
}
