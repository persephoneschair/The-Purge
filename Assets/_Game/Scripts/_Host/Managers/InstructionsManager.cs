using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InstructionsManager : SingletonMonoBehaviour<InstructionsManager>
{
    public TextMeshProUGUI instructionsMesh;
    public Animator instructionsAnim;
    private readonly string[] instructions = new string[4]
    {
        "",
        "",
        "",
        ""
    };

    [Button]
    public void OnShowInstructions()
    {
        AudioManager.Get.Play(AudioManager.OneShotClip.Whoosh);
        instructionsAnim.SetTrigger("toggle");
        string instructs = instructionsMesh.text;
        instructionsMesh.text = instructs
            .Replace("[MAIN]", Extensions.NumberToWords(QuestionManager.currentPack.mainGame.Count))
            .Replace("[PURGE]", Extensions.NumberToWords(QuestionManager.currentPack.purgeGame.Count))
            .Replace("[REMAIN]", Extensions.NumberToWords(QuestionManager.currentPack.finalGame.Count + 1));
    }

    [Button]
    public void OnHideInstructions()
    {
        AudioManager.Get.Play(AudioManager.OneShotClip.Whoosh);
        instructionsAnim.SetTrigger("toggle");
    }
}
