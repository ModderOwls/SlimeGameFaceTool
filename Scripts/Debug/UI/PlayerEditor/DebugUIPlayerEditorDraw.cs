using Godot;
using System;

public partial class DebugUIPlayerEditorDraw : Control
{
	[Export] DebugUIPlayerEditor editor;

	[Export] float tailBoneSize = 3;
	[Export] float tailVerticesSize = 3.5f;

	[Export] float raySize = 4;

	public bool _viewables = false;

	public bool _statesToggled = true;

	public bool _tailBonesToggled = false;
	public bool _tailVerticesToggled = false;
	public float colorsTailOpacity = 0.8f;

	public bool _raysToggled = true;
	public float colorsPhysicsOpacity = 0.6f;


	[ExportGroup("References - Viewables")]

	[ExportSubgroup("Tail")]

	[Export] Color colorTailBone;
	[Export] Color colorTailWidth;


    public override void _Draw()
    {
		if (editor.player == null || !_viewables) return;
		
		if (editor.tail.IsProcessing())
		{
			if (_tailBonesToggled)
			{
				// Bone Visualization
				Vector2[] bonePositions = new Vector2[editor.tail.bones.Length];
				for (int i = 0; i < bonePositions.Length; ++i)
				{
					bonePositions[i] = editor.mainUI.GetScreenPosition(editor.tail.bones[i].position);
				}

				colorTailBone.A = colorsTailOpacity;
				DrawPolyline(bonePositions, colorTailBone, tailBoneSize);
				
				colorTailWidth.A = colorsTailOpacity;
				for (int i = 0; i < bonePositions.Length; ++i)
				{
					DrawLine(
						editor.mainUI.GetScreenPosition(editor.tail.bones[i].position - editor.tail.bones[i].width), 
						editor.mainUI.GetScreenPosition(editor.tail.bones[i].position + editor.tail.bones[i].width), 
						colorTailWidth, 
						tailBoneSize
						);
				}
			}

			if (_tailVerticesToggled)
			{
				for (int i = 0; i < editor.tail.poly.Length; ++i)
				{
					float index = (float)i/(editor.tail.poly.Length-1);
					Color vertexColor;
					vertexColor.R = index;
					vertexColor.B = 1 - index;
					vertexColor.G = (0.5f - index) % 1;
					vertexColor.A = colorsTailOpacity;
					DrawCircle(editor.mainUI.GetScreenPosition(editor.tail.poly[i]), tailVerticesSize, vertexColor);
				}
			}
		}

		if (_raysToggled)
		{
			foreach (DrawRay ray in editor.player.drawRays)
			{
				Color rayColor = ray.color;
				rayColor.A = colorsPhysicsOpacity;
				DrawLine(editor.mainUI.GetScreenPosition(ray.from), editor.mainUI.GetScreenPosition(ray.to), rayColor, raySize);
			}
		}
    }
}
