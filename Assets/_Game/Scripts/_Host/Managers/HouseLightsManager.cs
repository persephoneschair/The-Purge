using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseLightsManager : SingletonMonoBehaviour<HouseLightsManager>
{
    public LightBox[] audienceLights;
    public LightBox[] studioLights;

    [Button]
    public void ToggleAudienceLights()
    {
        foreach (LightBox l in audienceLights)
            l.TogglePosition();
    }

    public void ToggleStudioLights()
    {
        foreach (LightBox l in studioLights)
            l.TogglePosition();
    }
}
