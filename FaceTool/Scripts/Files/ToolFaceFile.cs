using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;


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

		//Called whenever a file is loaded or a new one is made.
		List<Node> loadList = new List<Node>();
		public void RequestLoad()
		{
			//Refresh each node that was added.
    		foreach (Node node in loadList)
    		{
        		node.CallDeferred("Load");
    		}
		}

		public void ConnectLoad(Node node)
		{
			loadList.Add(node);
		}
		public void DisconnectLoad(Node node)
		{
			loadList.Remove(node);
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

			name = "";
			currentLimb = -1;
			currentEmotion = 0;
		}

		public void SaveToJSON(String path)
		{
			Godot.Collections.Dictionary data = new Godot.Collections.Dictionary();

			if (name == null || name == "")
			{
				name = "no_name";
			}

			data.Add("name", name);
			data.Add("limbs", new Godot.Collections.Array());

			for (int i = 0; i < limbs.Count; ++i)
			{
				LimbData limb = limbs[i];

    			Godot.Collections.Dictionary limbData = new Godot.Collections.Dictionary
    			{
        			{ "ghost", (int)limb.ghost },
        			{ "emotions", new Godot.Collections.Array() }
    			};

				for (int e = 0; e < limbs[i].emotions.Length; ++e)
				{
					EmotionData emotion = limb.emotions[e];

        			Godot.Collections.Dictionary emotionData = new Godot.Collections.Dictionary
        			{
            			{ "offset", emotion.offset },
            			{ "behaviour", (int)emotion.behaviour }
        			};

					string[] imageStrings = new string[emotion.images.Length];

					for (int img = 0; img < emotion.images.Length; ++img)
					{
						//Turn to base64 so you can store it in a json.
						//Alternatively could've made a folder with images and references in the json instead, but that would've made it more messy to work with.
    					byte[] bytes = emotion.images[img].SavePngToBuffer();
    					imageStrings[img] = Convert.ToBase64String(bytes);
					}

					emotionData.Add("emotions", imageStrings);

        			((Godot.Collections.Array)limbData["emotions"]).Add(emotionData);
				}

    			((Godot.Collections.Array)data["limbs"]).Add(limbData);
			}

			string jsonString = Json.Stringify(data, "\t");
			FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Write);

			file.StoreString(jsonString);

			file.Close();
		}

		public void LoadFromJSON(string path)
		{
    		if (!FileAccess.FileExists(path))
    		{
        		GD.PrintErr("File not found: " + path);
        		return;
    		}

    		FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
    		string jsonString = file.GetAsText();
    		file.Close();

    		Json json = new Json();
    		Error err = json.Parse(jsonString);
    		if (err != Error.Ok)
    		{
        		GD.PrintErr("Failed to parse JSON");
        		return;
    		}

    		Godot.Collections.Dictionary data = (Godot.Collections.Dictionary)json.Data;
    		if (data == null)
    		{
        		GD.PrintErr("Invalid JSON format");
        		return;
    		}

    		Reset();

    		name = (string)data["name"];

    		Godot.Collections.Array limbsArray = (Godot.Collections.Array)data["limbs"];
    		foreach (Godot.Collections.Dictionary limbData in limbsArray)
    		{
        		LimbData limb = new LimbData
        		{
            		ghost = (GhostType)(int)limbData["ghost"]
        		};

        		Godot.Collections.Array emotionsArray = (Godot.Collections.Array)limbData["emotions"];
        		for (int e = 0; e < emotionsArray.Count; e++)
        		{
            		Godot.Collections.Dictionary emotionData = (Godot.Collections.Dictionary)emotionsArray[e];
            		EmotionData emotion = new EmotionData
            		{
                		offset = ParseVector2((string)emotionData["offset"]),
                		behaviour = (BehaviourType)(int)emotionData["behaviour"]
            		};

            		string[] imageStrings = (string[])emotionData["emotions"];
            		for (int img = 0; img < imageStrings.Length; img++)
            		{
						//Turn base 64 back into an Image.
                		byte[] bytes = Convert.FromBase64String(imageStrings[img]);
                		Image image = new Image();
                		image.LoadPngFromBuffer(bytes);
                		emotion.images[img] = image;
            		}

            		limb.emotions[e] = emotion;
        		}

        		limbs.Add(limb);
    		}

			RequestLoad();
		}
		
		//Godot Vector2 parsing seems to be broken.. :/
		Vector2 ParseVector2(string vectorString)
		{
    		vectorString = vectorString.Trim('(', ')');
    		string[] parts = vectorString.Split(',');

    		if (parts.Length != 2)
			{
				GD.PrintErr("Parsing vector2 failed. Falling back to (0, 0).");

				return Vector2.Zero;
			}

			Vector2 vec = Vector2.Zero;

			//Culture info makes sure if there's a '.' it detects it.
    		vec.X = float.Parse(parts[0].Trim(), CultureInfo.InvariantCulture);
    		vec.Y = float.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);

    		return vec;
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
		Heavy
	}

	//Determines if it follows the eye ghost or the mouth ghost.
	public enum GhostType
	{
		Eye,
		Mouth
	}

}