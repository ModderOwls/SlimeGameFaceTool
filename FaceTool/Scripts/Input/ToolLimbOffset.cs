using Godot;
using System;
using FaceFiles;

public partial class ToolLimbOffset : VBoxContainer
{
	Vector2 offset;

	[Export] SpinBox spinOffsetX;
	[Export] SpinBox spinOffsetY;

	ToolFaceFile faceFileInstance;


    public override void _Ready()
    {
		faceFileInstance = ToolFaceFile.Instance;

		faceFileInstance.ConnectRefresh(this);
		//faceFileInstance.ConnectLoad(this);
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
		
		faceFileInstance.RequestRefresh();
	}

	public void Load()
	{
		EmotionData data = faceFileInstance.GetCurrentEmotionData();

		if (data == null) return;

		spinOffsetX.SetValueNoSignal(data.offset.X);
		spinOffsetY.SetValueNoSignal(data.offset.Y);
	}
}
