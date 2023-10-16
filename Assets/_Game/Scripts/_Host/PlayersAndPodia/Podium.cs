using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Podium : PodiumBase
{
    public Animator speechBubbleAnim;
    public TextMeshPro speechBubbleMesh;

    public override void InitialisePodium()
    {
        avatarRend.material.mainTexture = containedPlayer.profileImage;
        avatarRend.gameObject.SetActive(true);
        SetPodiumColor(PodiumMode.Default);
        AudioManager.Get.Play(AudioManager.OneShotClip.AnswerReceivedAndLobbyEntry);
    }

    public void ToggleSpeechBubble(string text = "")
    {
        speechBubbleAnim.SetTrigger("toggle");
        speechBubbleMesh.text = text;
    }

    public override void OnMouseOver()
    {
        if (containedPlayer != null && !containedPlayer.eliminated)
            playerNameMesh.text = containedPlayer.playerName;
    }

    public override void OnMouseDown()
    {
        if(containedPlayer != null && containedPlayer.flagForCondone && !containedPlayer.wasCorrect)
        {
            AudioManager.Get.Play(AudioManager.OneShotClip.AnswerReceivedAndLobbyEntry);
            HostManager.Get.SendPayloadToClient(containedPlayer, EventLibrary.HostEventType.Information, $"Your answer has been condoned.");
            containedPlayer.IterateTotal();
            containedPlayer.flagForCondone = false;
            containedPlayer.wasCorrect = true;
            containedPlayer.inPurge = false;
            SetPodiumColor(PodiumMode.Lowlight);
            //ToggleSpeechBubble();
        }
    }

    public override void OnMouseExit()
    {
        playerNameMesh.text = "";
    }
}