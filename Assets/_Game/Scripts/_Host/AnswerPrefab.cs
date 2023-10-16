using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnswerPrefab : MonoBehaviour
{
    public Animator anim;
    public TextMeshProUGUI mesh;

    public Image background;
    public Color[] colors;
    public enum Mode { Default = 0, Incorrect, Correct };

    public void Init(string txt)
    {
        mesh.text = txt;
    }

    public void ToggleAnswer()
    {
        anim.SetTrigger("toggle");
    }

    public void SetAnswerColor(Mode mode)
    {
        mesh.color = (int)mode == 1 ? colors[4] : colors[3];
        background.color = colors[(int)mode];
    }
}
