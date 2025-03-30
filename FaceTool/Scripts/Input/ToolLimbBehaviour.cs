using Godot;
using System;
using FaceFiles;

public partial class ToolLimbBehaviour : Node
{
	[Export] OptionButton optionButton;
	BehaviourType behaviour;

	ToolFaceFile faceFileInstance;

    public override void _Ready()
    {
		faceFileInstance = ToolFaceFile.Instance;

		faceFileInstance.ConnectRefresh(this);
    }

	public void Refresh(bool isEmpty)
	{
		if (isEmpty) return;

		optionButton.Select((int)faceFileInstance.GetCurrentEmotionData().behaviour);
	}

	public void SetBehaviour(int index)
	{
		behaviour = (BehaviourType)index;

		UpdateFileBehaviour();
	}

	public void SetBehaviour(BehaviourType behaviour)
	{
		this.behaviour = behaviour;

		UpdateFileBehaviour();
	}

	public void UpdateFileBehaviour()
	{
		EmotionData data = faceFileInstance.GetCurrentEmotionData();

		if (data == null) return;

		data.behaviour = behaviour;
		
		faceFileInstance.RequestRefresh();
	}
}
