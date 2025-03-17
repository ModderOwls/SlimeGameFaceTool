using Godot;
using System;
using System.Collections.Generic;
using System.Threading;
using FaceFiles;

public partial class ToolLimbs : ToolDataSelectionHandler<ToolLimbData>
{
    ToolFaceFile faceFileInstance;

    public override int current 
    { 
        get { return base.current; }
        set 
        {
            base.current = value;

            UpdateFileCurrentLimb();
        }
    }

    public override void _Ready()
    {
        base._Ready();

        faceFileInstance = ToolFaceFile.Instance;

        faceFileInstance.ConnectRefresh(this);
    }

    public override void Select(Control node)
    {
        base.Select(node);

        faceFileInstance.RequestRefresh();
    }

    public override Control AddPrefab()
    {
        ToolLimbData data = base.AddPrefab() as ToolLimbData;

        data.index = allData.Count - 1;

        faceFileInstance.limbs.Add(new LimbData());

        return data;
    }

    public override void RemovePrefabAt(int index)
    {
        base.RemovePrefabAt(index);

        //Update all after to have the correct index.
        for (int i = index; i < allData.Count; ++i)
        {
            allData[i].index--;
        }
        
        LimbData data = faceFileInstance.limbs[index];
        faceFileInstance.limbs.RemoveAt(index);
        data.Dispose();
    }

    public void PressedGhost(ToolLimbData data)
    {
        //Resets ghost index if reaching max GhostColors length.
        data.ghost = (GhostType)(((int)data.ghost + 1) % Enum.GetValues(typeof(GhostType)).Length);

        data.ghostButton.SelfModulate = ToolFaceFile.colorsGhosts[(int)data.ghost];

        UpdateFileGhost(data.index, data.ghost);
    }

    public void Delete()
    {
        if (allData.Count == 0) return;

        RemovePrefabAt(current);

        faceFileInstance.RequestRefresh();
    }

    void UpdateFileCurrentLimb()
    {
        faceFileInstance.currentLimb = current;
    }

    void UpdateFileGhost(int index, GhostType ghost)
    {
        faceFileInstance.GetLimbData(index).ghost = ghost;
    }

    public void Refresh(bool isEmpty)
    {
        if (isEmpty) return;

        for (int i = 0; i < faceFileInstance.limbs.Count; ++i)
        {
            Image image = faceFileInstance.limbs[i].emotions[faceFileInstance.currentEmotion].images[1];
            if (image != null)
            {
                ImageTexture texture = new ImageTexture();
                texture.SetImage(image);

                allData[i].textureRect.Texture = texture;
            }
            else
            {
                allData[i].textureRect.Texture = null;
            }
        }
    }
}