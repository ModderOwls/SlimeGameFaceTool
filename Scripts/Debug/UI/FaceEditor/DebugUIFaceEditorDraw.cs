using Godot;
using System;

public partial class DebugUIFaceEditorDraw : Control
{
	[Export] DebugUIFaceEditor editor;

	public bool _viewables = false;

	[Export] public float ghostSize = 5;
	public bool _ghostsToggled = true;
	public bool _ghostInfoToggled = false;
	public readonly static Color[] colorsGhosts = { Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow, Colors.White, Colors.Black };
	public float colorsGhostsOpacity = 0.6f;

	public bool _lookDirectionToggled = true;
	public bool _glanceToggled = true;

	[Export] public float lookDirectionLength = 100;
	[Export] public float glanceLength = 40;

	
	[ExportGroup("References - Viewables")]

	[ExportSubgroup("Look")]
	
	[Export] public Color colorLookDirection;
	[Export] public Color colorGlance;


    public override void _Draw()
    {
		if (editor.face == null || !_viewables) return;
		
		//Draw Look-direction line.
		Vector2 glancePosition = editor.face.GetScreenTransform().Origin / editor.mainUI.Scale;
		if (_lookDirectionToggled)
		{
			DrawLine(glancePosition, glancePosition + editor.face.lookDirection * lookDirectionLength, colorLookDirection, 10);
			
			glancePosition += editor.face.lookDirection * lookDirectionLength;
		}

		//Draw Glance line.
		if (_glanceToggled)
		{
			DrawLine(glancePosition, glancePosition + Vector2.Right * editor.face.Glance * glanceLength, colorGlance, 10);
		}
		
		//Draw Ghosts.
		if (_ghostsToggled)
		{
			for (int i = 0; i < editor.face.playerGhosts.Length; ++i)
			{
				Color color = colorsGhosts[i];
				color.A = colorsGhostsOpacity;

				DrawCircle
				(
					editor.face.playerGhosts[i].GetScreenTransform().Origin / editor.mainUI.Scale, 
					ghostSize, 
					color
				);
			}
		}
    }
}
