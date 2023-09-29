using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChevronManager : SingletonMonoBehaviour<ChevronManager>
{
    public ChevronWall[] walls;

    public void SinglePulse(bool down)
    {
        foreach (ChevronWall wall in walls)
            wall.SinglePulse(down);
    }

    public void MultiPulse(bool down)
    {
        foreach (ChevronWall wall in walls)
            wall.MultiPulse(down);
    }

    public void StaticLights(bool down)
    {
        foreach (ChevronWall wall in walls)
            wall.SetStatic(down);
    }
}
