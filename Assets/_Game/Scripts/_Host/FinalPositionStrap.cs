using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FinalPositionStrap : MonoBehaviour
{
    public TextMeshProUGUI[] strapMeshes;
    public RawImage avatar;

    public void Init(string[] values, Texture av)
    {
        for (int i = 0; i < values.Length; i++)
            strapMeshes[i].text = values[i];
        avatar.texture = av;
    }
}
