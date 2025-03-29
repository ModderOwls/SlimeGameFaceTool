using Godot;
using System;

//Mostly used to call up using signals and then call down.
//Follows Godot's 'signal up, call down' principal to help loose couplings.
public partial class ToolAnimationSlide : HSplitContainer
{
	public override void _Process(double delta)
	{
		if (playerManagement != null)
		{
			UpdateTimeline((float)playerManagement.GetAnimationPosition());
		}
	}


	[ExportGroup("Player Management")]

	[Export] ToolPlayerManagement playerManagement;

	//Signal up to this instead of calling playerManagement directly to keep modularity and components individual.
	public void PlayAnimation(string name)
	{
		if (playerManagement == null) return;

		playerManagement.PlayAnimation(name);

		SetTimelineLength((float)playerManagement.GetAnimationLength());
	}

	public void TogglePlayMode(bool toggle)
	{
		if (playerManagement == null) return;

		if (toggle) playerManagement.PlayMode();
		else playerManagement.StopPlayMode();
	}

	public void SetAnimationTimeNormalized(float time)
	{
		if (playerManagement == null) return;
		
		playerManagement.SetAnimationTimeNormalized(time);
		playerManagement.SetAnimationPaused(true);
	}

	public void PressAnimationPause()
	{
		if (playerManagement == null) return;

		playerManagement.SetAnimationPaused(playerManagement.IsPlaying());
	}


	[ExportGroup("Sprite View")]

	[Export] ToolSpriteView spriteView;

	public void ToggleGhost(bool toggle)
	{
		if (spriteView == null) return;

		spriteView.ToggleGhost(toggle);
	}


	[ExportGroup("Animation Timeline")]

	[Export] ToolAnimationTimeline animationTimeline;

	public void SetTimelineLength(float animationLength)
	{
		if (animationTimeline == null) return;

		animationTimeline.SetTimelineLength(animationLength);
	}

	public void UpdateTimeline(float position)
	{
		if (animationTimeline == null) return;

		animationTimeline.UpdateTimeline(position);
	}
}
