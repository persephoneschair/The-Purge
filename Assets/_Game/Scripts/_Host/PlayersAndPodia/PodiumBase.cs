using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PodiumBase : MonoBehaviour
{
    public PlayerObject containedPlayer;
    public TextMeshPro playerNameMesh;

    public Renderer avatarRend;
    public enum PodiumMode { Default, Lowlight, Incorrect, Eliminated, FinalCorrect };
    public Renderer[] podiumParts;
    public Material[] podiumMats;


    void Start()
    {
        SetPodiumColor(PodiumMode.Eliminated);
        playerNameMesh.text = "";
        avatarRend.gameObject.SetActive(false);
    }

    public virtual void InitialisePodium()
    {

    }

    public virtual void InitialisePodium(PlayerObject po)
    {

    }

    public void SetPodiumColor(PodiumMode mode)
    {
        foreach (Renderer r in podiumParts)
        {
            if (r.materials.Length > 1)
                r.materials[0] = podiumMats[(int)mode];
            else
                r.material = podiumMats[(int)mode];
        }
    }

    public virtual void OnMouseOver()
    {

    }

    public virtual void OnMouseDown()
    {

    }

    public virtual void OnMouseExit()
    {

    }
}
