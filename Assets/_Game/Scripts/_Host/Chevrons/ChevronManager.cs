using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChevronManager : SingletonMonoBehaviour<ChevronManager>
{
    public ChevronWall[] walls;

    #region Public Functions
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
    #endregion

    #region Debug Buttons
    [Button]
    private void SinglePulseUp()
    {
        SinglePulse(false);
    }

    [Button]
    private void SinglePulseDown()
    {
        SinglePulse(true);
    }

    [Button]
    private void MultiPulseUp()
    {
        MultiPulse(false);
    }

    [Button]
    private void MultiPulseDown()
    {
        MultiPulse(true);
    }

    [Button]
    private void StaticUp()
    {
        StaticLights(false);
    }

    [Button]
    private void StaticDown()
    {
        StaticLights(true);
    }
    #endregion
}
