using Godot;
using System;
using FaceFiles;

//Manages emotions in general. Gets all possible emotions using reflection.
public partial class ToolEmotions : ToolSelectionHandler
{
    ToolFaceFile faceFileInstance;

    public override int current 
    { 
        get { return base.current; }
        set 
        {
            base.current = value;
            
            UpdateFileCurrentEmotion();
        }
    }

    public override void _Ready()
	{
		base._Ready();

		startWith = Enum.GetNames(typeof(Emotion)).Length;

        faceFileInstance = ToolFaceFile.Instance;
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

    void UpdateFileCurrentEmotion()
    {
        faceFileInstance.currentEmotion = current;

        faceFileInstance.RequestRefresh();
    }
}
