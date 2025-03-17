using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class ToolGetAllAnimations : Tree
{
    [Export] Node AnimatorNode;

    [Export(PropertyHint.Dir)] string rootPath = "res://Animations/Player/";
	[Export] string fileExtension = ".anim";

    public override void _Ready()
    {
        TreeItem root = CreateItem();

        AddFilesToTree(root, rootPath, fileExtension);
    }

    private void AddFilesToTree(TreeItem root, string path, string extension)
    {
        DirAccess dir = DirAccess.Open(path);
        if (DirAccess.GetOpenError() != Error.Ok)
        {
            GD.PrintErr("Could not gain access to folder path: " + path);

            return;
        }

        dir.ListDirBegin();

        string fileName = dir.GetNext();

        Dictionary<string, TreeItem> directoryItems = new Dictionary<string, TreeItem>();

        while (fileName != "")
        {
            string fullPath = path + "/" + fileName;

            if (dir.CurrentIsDir())
            {
				//Add directory folder and add an entry to the dictionary.
                TreeItem dirItem = CreateItem(root);
                dirItem.SetText(0, fileName);

                directoryItems[fullPath] = dirItem;

				//Recurse.
                AddFilesToTree(dirItem, fullPath, extension);
            }
            else if (fileName.EndsWith(extension))
            {
				//Add under correct directory.
                string directoryPath = path;
                TreeItem dirItem = directoryItems.ContainsKey(directoryPath) ? directoryItems[directoryPath] : root;

				//Add file itself.
                TreeItem fileItem = CreateItem(dirItem);
                fileItem.SetText(0, fileName.Substr(0, fileName.Length - extension.Length));
            }

			fileName = dir.GetNext();
        }
    }

    public void PressedItem()
    {
        TreeItem selected = GetSelected();

        if (selected.GetChildCount() != 0) return;
        
        EmitSignal("PlayAnimation", selected.GetText(GetSelectedColumn()));
    }

    [Signal]
    public delegate void PlayAnimationEventHandler(string name);
}
