using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Resources;

[Tool]
public partial class AttackHandler : Node2D
{
	public List<CollisionShape2D> hitboxes = new();
	public List<CircleShape2D> hitboxesShapes = new();

	private bool enabled = false;
	[Export] public bool Enabled
	{
		get
		{
			return enabled;
		}
		set 
		{ 
			enabled = value;

			if (enabled) return;

			QueueRedraw();

			ClearHitboxes();
		}
	}

	private HitboxData hitbox;
	[Export] public HitboxData Hitbox 
	{
		get
		{
			return hitbox;
		}
		set
		{
			hitbox = value;

			QueueRedraw();

			if (Engine.IsEditorHint()) return;

			ReadyHitbox(hitbox);
		}
	}


	[ExportSubgroup("References")]

	[Export] public PackedScene prefabHitbox;

	[Export] public Node parentHitbox;
	

	[ExportSubgroup("Debug")]

	[Export] Godot.Color boxColor = new(1, .259f, .2f, .2f);
	Godot.Color insideColor = new(1, 1, 1, .1f);


    public override void _Draw()
    {
		if (Hitbox == null || !Enabled) return;

		for (int i = 0; i < Hitbox.hits.Length; ++i)
		{
			DrawCircle(Hitbox.hits[i].position, Hitbox.hits[i].radius, boxColor);
			DrawCircle(Hitbox.hits[i].position, Hitbox.hits[i].radius * .96f, insideColor);
		}
    }

    public override void _Ready()
	{
		hitbox = null;

		parentHitbox = GetChild(0);
	}

	public void ReadyHitbox(HitboxData data){
		//pools hitboxes.
		if (hitboxes.Count != data.hits.Length)
		{
			if (hitboxes.Count < data.hits.Length)
			{
				//instantiate hitboxes and shapes until reaching total amount.
				int hitLeft = data.hits.Length-hitboxes.Count;
				for (int i = 0; i < hitLeft; ++i)
				{
					CreateHitbox();

					//GD.Print("Appended hitbox : ", hitboxes.Count);
				}
			} 
			else
			{
				//free's hitboxes that arent needed.
				for (int i = hitboxes.Count-1; i != data.hits.Length-1; --i)
				{
					hitboxes[i].QueueFree();

					//GD.Print("Removed hitbox : ", i);
				}

				hitboxes.RemoveRange(data.hits.Length, hitboxes.Count-data.hits.Length);
				hitboxesShapes.RemoveRange(data.hits.Length, hitboxes.Count-data.hits.Length);
			}
		}

		for (int i = 0; i < data.hits.Length; ++i)
		{
			hitboxes[i].Position = data.hits[i].position;
			hitboxesShapes[i].Radius = data.hits[i].radius;

			hitboxes[i].Shape = hitboxesShapes[i];
			hitboxes[i].Disabled = false;
		}
	}

	public void ClearHitboxes()
	{
		for (int i = 0; i < hitboxes.Count; ++i)
		{
			hitboxes[i].Disabled = true;
		}

		#if DEBUG

		if (parentHitbox != null && hitboxes.Count != parentHitbox.GetChildCount())
		{
			GD.PrintErr("pooled hitboxes and child count is different! : ", hitboxes.Count, " vs ", parentHitbox.GetChildCount());
		}

		#endif
	}

	void CreateHitbox()
	{
		Node scene = prefabHitbox.Instantiate();
		CollisionShape2D prefab = (CollisionShape2D)scene;
		parentHitbox.AddChild(prefab);

		hitboxes.Add(prefab);
		hitboxesShapes.Add(new CircleShape2D());
	}
}
