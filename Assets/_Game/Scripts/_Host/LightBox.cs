using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLB;
using System.Linq;
using NaughtyAttributes;

public class LightBox : MonoBehaviour
{
    [Header("Objects")]
    public GameObject[] lampObjects;
    public VolumetricLightBeamSD[] beams;
    public GameObject piston;
    public Animator anim;

    [Header("Settings")]
    public float[] lampAngles;
    public float spotAngle;
    public Color beamColor;
    public float pistonAngle;

    private void Start()
    {
        beamColor = beams.FirstOrDefault().color;
        spotAngle = beams.FirstOrDefault().spotAngle;
        pistonAngle = piston.transform.localEulerAngles.x;
    }

    [Button]
    public void SetLamps()
    {
        for(int i = 0; i < lampObjects.Length; i++)
        {
            lampObjects[i].transform.localEulerAngles = new Vector3(lampAngles[i], 90f, 0f);
            beams[i].color = beamColor;
            beams[i].spotAngle = spotAngle;
        }
        piston.transform.localEulerAngles = new Vector3(pistonAngle, piston.transform.localEulerAngles.y, piston.transform.localEulerAngles.z);
    }

    [Button]
    public void TogglePosition()
    {
        anim.SetTrigger("toggle");
    }
}
