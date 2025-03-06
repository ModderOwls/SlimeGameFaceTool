using Godot;
using System;

public abstract partial class PlayerEmotion : Resource
{
    public PlayerFaceLimb limb;

    [Export] public CompressedTexture2D sprite { get; set; }
    [Export] public Vector2 spriteSize { get; set; }
    [Export] public Vector2 spriteOffset { get; set; }

    public virtual void Initialize(PlayerFaceLimb limb)
    {
        this.limb = limb;
    }

    public virtual void OnSet(float glance)
    {
		limb.Texture = sprite;
		limb.Offset = spriteOffset;

        Rect2 newRect = limb.RegionRect;
        newRect.Position = Vector2.Zero;
        limb.RegionRect = newRect;

        OnChangeGlance(glance);
    }

    public virtual void OnRemove() { }

    /// <summary>
    /// Update the emotion every frame.
    /// </summary>
    public virtual void Update() { }

    /// <summary>
    /// Override this if you want your emotion to be less sensitive to glance sprite switches.
    /// </summary>
    /// <param name="direction"></param>
    public virtual void OnChangeGlance(float direction)
    {
        int intDirection = Mathf.RoundToInt(direction);

        Rect2 newRect = limb.RegionRect;
        newRect.Position = new Vector2(newRect.Position.X, ApplyGlance(intDirection) * newRect.Size.Y);
        limb.RegionRect = newRect;
    }

    

    /// <summary>
    /// By default the order of an emotion sprite is:
    /// Sprite 0: direction = -1, 
    /// Sprite 1: direction = 0, 
    /// Sprite 2: direction = 1.
    /// 
    /// Override if you have a different order.
    /// </summary>
    /// <param name="direction"></param>
    public virtual int ApplyGlance(int direction)
    {
        return direction + 1;
    }

    public virtual bool SideOverride(bool side)
    {
        return side;
    }
}