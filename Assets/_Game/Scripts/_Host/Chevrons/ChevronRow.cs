using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChevronRow : MonoBehaviour
{
    public Chevron[] chevrons;

    public void LightRowUp()
    {
        foreach (Chevron c in chevrons)
            c.LightUp();
    }

    public void LightRowDown()
    {
        foreach (Chevron c in chevrons)
            c.LightDown();
    }

    public void SwitchRowOff()
    {
        foreach (Chevron c in chevrons)
            c.SwitchOff();
    }
}
