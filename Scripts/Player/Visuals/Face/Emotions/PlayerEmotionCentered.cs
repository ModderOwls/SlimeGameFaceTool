using Godot;
using System;
using System.ComponentModel;

[Tool]
public partial class PlayerEmotionCentered : PlayerEmotion
{
    [Description 
    (
        "The frames for the distance of the limb. 1: Normal sprite, 2: 1 extra frame for abs(x) = 1, etc."
    ) ]
    [Export] public int frames = 2;

    int lastPosAbsX;

    public override void Update()
    {
        //Check if its the same pixel position as last frame update. 
        int posAbsX = Mathf.Abs(limb.intPosition.X);
        if (posAbsX == lastPosAbsX) return;

        if (posAbsX >= frames) posAbsX = frames - 1;
        
        lastPosAbsX = posAbsX;

        int newSpriteIndex = ApplyGlance(glance) * frames + posAbsX;
        Rect2 newRect = limb.RegionRect;

        newRect.Position = spritePositions[newSpriteIndex];
        newRect.Size = spriteSizes[newSpriteIndex];

        limb.RegionRect = newRect;
    }
}
