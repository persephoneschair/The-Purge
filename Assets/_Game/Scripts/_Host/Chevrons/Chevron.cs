using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chevron : MonoBehaviour
{
    public GameObject[] down;
    public GameObject[] up;

    private void Start()
    {
        SwitchOff();
    }

    public void LightUp()
    {
        for (int i = 0; i < up.Length; i++)
            up[i].SetActive(true);
    }

    public void LightDown()
    {
        for (int i = 0; i < down.Length; i++)
            down[i].SetActive(true);
    }

    public void SwitchOff()
    {
        for (int i = 0; i < down.Length; i++)
        {
            down[i].SetActive(false);
            up[i].SetActive(false);
        }
    }
        
}
