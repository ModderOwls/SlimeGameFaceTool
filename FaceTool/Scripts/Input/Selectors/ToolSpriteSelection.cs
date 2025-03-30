using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FaceFiles;

public partial class ToolSpriteSelection : ToolDataSelectionHandler<SpriteSelectionData>
{
    ToolFaceFile faceFileInstance;

    public override void _Ready()
    {
        base._Ready();

        GetWindow().FilesDropped += OnFilesDropped;

        faceFileInstance = ToolFaceFile.Instance;

        faceFileInstance.ConnectRefresh(this);
    }

    public void OnFilesDropped(string[] files)
    {
        if (files.Length != 1)
        {
            //If somehow no files are dropped, do nothing.
            if (files.Length == 0) return;

            GD.Print("Multiple file drops detected, falling back to first.");
        }


        if (!files[0].EndsWith(".png") && !files[0].EndsWith(".webp")) return;

        Image image = new Image();
        image.Load(files[0]);

        SetSpriteTexture(image);
    }

    public override void _UnhandledInput(InputEvent input)
    {
        if (input.IsActionPressed("ui_paste"))
        {
            if (DisplayServer.ClipboardHasImage())
            {
                SetSpriteTexture(DisplayServer.ClipboardGetImage());
            }
        }
    }

    public void SetSpriteTexture(Image image)
    {
        ImageTexture texture = new ImageTexture();
        texture.SetImage(image);

        allData[current].textureRect.Texture = texture;

        UpdateFileSprite(current, image);
    }

    public void UpdateFileSprite(int index, Image image)
    {
        EmotionData data = faceFileInstance.GetCurrentEmotionData();

        if (data == null) return;

        data.images[index] = image;

        faceFileInstance.RequestRefresh();
    }

    public void Refresh(bool isEmpty)
    {
        //If there aren't any limbs added, remove visibility.
        if (faceFileInstance.GetLimbData(0) == null)
        {
            Visible = false;

            return;
        }

        Visible = true;

        Image[] images = faceFileInstance.GetCurrentEmotionData().images;

        for (int i = 0; i < images.Length; ++i)
        {
            if (images[i] != null)
            {
                ImageTexture texture = new ImageTexture();
                texture.SetImage(images[i]);

                allData[i].textureRect.Texture = texture;
            }
            else
            {
                allData[i].textureRect.Texture = null;
            }
        }
    }
}
