using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FinalPodium : PodiumBase
{
    public Material grayscaleMat;

    public override void InitialisePodium(PlayerObject po)
    {
        containedPlayer = po;
        avatarRend.material.mainTexture = containedPlayer.profileImage;
        avatarRend.gameObject.SetActive(true);
        if(!containedPlayer.eliminated)
            playerNameMesh.text = po.playerName;

        SetPodiumColor(PodiumMode.Lowlight);
    }

    public override void OnMouseOver()
    {

    }

    public override void OnMouseDown()
    {
        if (containedPlayer != null && containedPlayer.flagForCondone && !containedPlayer.wasCorrect)
        {
            AudioManager.Get.Play(AudioManager.OneShotClip.AnswerReceivedAndLobbyEntry);
            var roundRoot = (GameplayManager.Get.rounds[(int)GameplayManager.Get.currentRound] as FinalRound);
            HostManager.Get.SendPayloadToClient(containedPlayer, EventLibrary.HostEventType.Information, $"Your answer has been condoned.");
            containedPlayer.IterateTotal();
            containedPlayer.flagForCondone = false;
            containedPlayer.wasCorrect = true;
            SetPodiumColor(PodiumMode.FinalCorrect);
            roundRoot.tickerMesh.text = $"<color=green>{containedPlayer.submission}";
            if(roundRoot.questionWasPassed)
                roundRoot.passer.podium.SetPodiumColor(PodiumMode.Incorrect);

            roundRoot.SetStageAtEliminationPhase();
        }
    }

    public override void OnMouseExit()
    {

    }
}
