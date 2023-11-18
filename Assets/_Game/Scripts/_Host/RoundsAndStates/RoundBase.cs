using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

public class RoundBase : MonoBehaviour
{
    public Animator scrollingTextAnim;
    public TextMeshProUGUI scrollingTextMesh;

    public Question currentQuestion = null;
    public Animator questionLozengeAnim;
    public TextMeshProUGUI questionMesh;
    public static List<AnswerPrefab> answerPrefabs = new List<AnswerPrefab>();

    public GameObject answerPrefabToInstance;
    public Transform answerPrefabTarget;

    public virtual void LoadQuestion()
    {
        foreach(AnswerPrefab a in answerPrefabs)
            Destroy(a.gameObject);
        answerPrefabs.Clear();
        currentQuestion = null;
        questionMesh.text = "";
    }

    public virtual void RunQuestion()
    {
        AudioManager.Get.Play(AudioManager.OneShotClip.Whoosh);
        questionLozengeAnim.SetTrigger("toggle");
        ChevronManager.Get.SinglePulse(false);
        HouseLightsManager.Get.ToggleAudienceLights();
        foreach (PlayerObject pl in PlayerManager.Get.players)
            HostManager.Get.SendPayloadToClient(pl, EventLibrary.HostEventType.Information, "Get ready for the question...");
    }

    public virtual IEnumerator RunQuestionLate()
    {
        yield break;
    }

    public virtual void QuestionRunning()
    {

    }

    public virtual void OnQuestionEnded()
    {
        AudioManager.Get.Play(AudioManager.OneShotClip.TimeUp);
        ChevronManager.Get.SinglePulse(true);
        foreach (PlayerObject pl in PlayerManager.Get.players)
        {
            HostManager.Get.SendPayloadToClient(pl, EventLibrary.HostEventType.Information, $"TIME UP!");
            if (!pl.eliminated && !pl.inFinal)
                pl.podium.SetPodiumColor(Podium.PodiumMode.Lowlight);
        }            

        HouseLightsManager.Get.ToggleAudienceLights();
        GameplayManager.Get.currentStage = GameplayManager.GameplayStage.DisplayAnswer;

        DebugLog.Print("END OF RESPONSES", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Default);
    }

    public virtual void DisplayAnswer()
    {

    }

    public virtual void PanToMeter()
    {

    }

    public virtual void StartTheMeter()
    {

    }

    public virtual void ResetForNewQuestion()
    {
        questionLozengeAnim.SetTrigger("toggle");
        ResetPlayerVariables();
        PlayerManager.Get.DisplayPlayerCount();
    }

    public virtual void TriggerFinalRound()
    {
        //All are eliminated or all but one are eliminated
        if(PlayerManager.Get.players.Count(x => !x.eliminated) < 2)
        {
            LobbyManager.Get.TogglePermaCode();
            GameplayManager.Get.currentRound = GameplayManager.Round.None;
            GameplayManager.Get.currentStage = GameplayManager.GameplayStage.DisplayFinalLeaderboard;
            AudioManager.Get.StopLoop();
            AudioManager.Get.Play(AudioManager.OneShotClip.LongSting);

            ChevronManager.Get.MultiPulse(false);

            foreach (PlayerObject po in PlayerManager.Get.players)
                HostManager.Get.SendPayloadToClient(po, EventLibrary.HostEventType.Information, $"Thanks for playing\n\nYou earned {po.totalCorrect * GameplayPennys.Get.multiplyFactor} Pennys this game");

            //One survivor
            if(PlayerManager.Get.players.Count(x => !x.eliminated) == 1)
            {
                PlayerObject winner = PlayerManager.Get.players.FirstOrDefault(x => !x.eliminated);
                DebugLog.Print($"{winner.playerName} has survived The Purge!", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Green);
                TriggerScrollingText($"{winner.playerName.ToUpperInvariant()} HAS SURVIVED\nTHE PURGE");
            }
            //No survivors
            else
            {
                DebugLog.Print($"Nobody has survived The Purge!", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Red);
                TriggerScrollingText($"NOBODY HAS SURVIVED\nTHE PURGE");
            }
        }
        else
        {
            GameplayManager.Get.currentRound = GameplayManager.Round.FinalRound;
            GameplayManager.Get.currentStage = GameplayManager.GameplayStage.RevealFinalists;
            AudioManager.Get.Play(AudioManager.OneShotClip.Whoosh);
            AudioManager.Get.Play(AudioManager.OneShotClip.Klaxon);
            TriggerScrollingText("TIME FOR THE\nFINAL PURGE");
            AudioManager.Get.Play(AudioManager.OneShotClip.Whoosh, 3.5f);
        }
    }

    public virtual void ResetPlayerVariables()
    {
        foreach (PlayerObject po in PlayerManager.Get.players)
        {
            po.ResetSoftPlayerVariables();
            if(GameplayManager.Get.currentRound != GameplayManager.Round.FinalRound)
                po.podium.SetPodiumColor(po.eliminated ? Podium.PodiumMode.Eliminated : Podium.PodiumMode.Default);
        }
    }

    public virtual void TriggerScrollingText(string textContent)
    {
        scrollingTextMesh.text = textContent;
        scrollingTextAnim.SetTrigger(UnityEngine.Random.Range(0, 2) == 1 ? "toggleLeft" : "toggleRight");
    }

    public bool ReadyForFinal()
    {
        return
            (QuestionManager.FinalPlayerCountReached()
            || QuestionManager.EndOfMainGameQs()
            || QuestionManager.EndOfPurgeQs())
            ? true : false;
    }
}
