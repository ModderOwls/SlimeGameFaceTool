using Godot;
using System;
using System.Collections.Generic;


namespace FaceFiles
{
	public partial class ToolFaceFile : Node
	{
		public static ToolFaceFile Instance { get; private set;}

		[Export] public string name;
		[Export] public int currentLimb = -1;
		[Export] public int currentEmotion;

		public List<LimbData> limbs = new List<LimbData>();

		//Default color scheme for ghosts.
		public readonly static Color[] colorsGhosts = { Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow, Colors.White, Colors.Black };


    	public override void _Ready()
    	{
			Instance = this;
    	}

		public LimbData GetLimbData(int index)
		{
			if (index < 0 || index >= limbs.Count) return null;

			return limbs[index];
		}

		public LimbData GetCurrentLimbData()
		{
			if (currentLimb < 0 || currentLimb >= limbs.Count) return null;

			return limbs[currentLimb];
		}

		public EmotionData GetCurrentEmotionData()
		{
			if (currentLimb < 0 || currentLimb >= limbs.Count) return null;

			return limbs[currentLimb].emotions[currentEmotion];
		}

		//Add to this list if you want it to be alerted when refreshing.
		List<Node> refreshList = new List<Node>();
    	public void RequestRefresh()
		{
			//Pass boolean that tells whether a limb and emotion is selected right now.
			bool isEmpty = GetCurrentEmotionData() == null;

			//Refresh each node that was added.
    		foreach (Node node in refreshList)
    		{
        		node.CallDeferred("Refresh", isEmpty);
    		}
		}

		public void ConnectRefresh(Node node)
		{
			refreshList.Add(node);
		}
		public void DisconnectRefresh(Node node)
		{
			refreshList.Remove(node);
		}

		//Removes any set variables as new.
		public void Reset()
		{
			//Clear any data to prevent memory leak.
			foreach (LimbData limb in limbs)
			{
				limb.Dispose();
			}

			limbs = new List<LimbData>();

			refreshList = new List<Node>();

			name = "";
			currentLimb = -1;
			currentEmotion = 0;
		}
	}

	//Contains important information surrounding limbs for saving and loading.
	public partial class LimbData : GodotObject
	{
		public LimbData()
		{
    		emotions = new EmotionData[Enum.GetValues(typeof(Emotion)).Length];
    		ghost = GhostType.Eye;

    		for (int i = 0; i < emotions.Length; i++)
    		{
				//Fill every slot.
        		emotions[i] = new EmotionData();
    		}
		}

		~LimbData()
		{
			//Cleanup to prevent memory leaks.
			foreach (EmotionData emotion in emotions)
			{
				emotion.Dispose();
			}
		}

		public EmotionData[] emotions;
		public GhostType ghost;
	}

	//Contains important information surrounding *emotions* for saving and loading.
	public partial class EmotionData : GodotObject 
	{
		public EmotionData()
		{
			images = new Image[3];
			offset = Vector2.Zero;
			behaviour = BehaviourType.Default;
		}
		
		~EmotionData()
		{
			foreach (Image image in images)
			{
				image.Dispose();
			}
		}

		public Image[] images;
		public Vector2 offset;
		public BehaviourType behaviour;
	}

	//Determines the type of special behaviour the limb has.
	public enum BehaviourType
	{
		Default,
		Centered
	}

	//Determines if it follows the eye ghost or the mouth ghost.
	public enum GhostType
	{
		Eye,
		Mouth
	}

}