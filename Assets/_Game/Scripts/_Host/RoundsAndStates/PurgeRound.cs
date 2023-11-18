using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PurgeRound : RoundBase
{
    public override void LoadQuestion()
    {
        base.LoadQuestion();

        questionLozengeAnim.SetTrigger("toggle");
        CameraLerpManager.Get.ZoomToPosition(CameraLerpManager.CameraPosition.Audience, 50f);

        currentQuestion = QuestionManager.currentPack.purgeGame[GameplayManager.nextPurgeQuestionIndex];
        answerPrefabs.Add(Instantiate(answerPrefabToInstance, answerPrefabTarget).GetComponent<AnswerPrefab>());
        answerPrefabs.FirstOrDefault().Init("???");

        StartCoroutine(HighlightRelevantPlayers());
        GameplayManager.Get.currentStage = GameplayManager.GameplayStage.DoNothing;
    }

    private IEnumerator HighlightRelevantPlayers()
    {
        yield return new WaitForSeconds(2f);
        foreach(PlayerObject po in PlayerManager.Get.players.Where(x => !x.eliminated))
        {
            if (po.wasCorrect)
                po.ResetSoftPlayerVariables();

            else
            {
                po.podium.SetPodiumColor(Podium.PodiumMode.Incorrect);
                po.ResetSoftPlayerVariables();
                po.inPurge = true;
                AudioManager.Get.Play(AudioManager.OneShotClip.AnswerReceivedAndLobbyEntry);
                yield return new WaitForSeconds(0.25f);
            }
        }
        GameplayManager.Get.currentStage = GameplayManager.GameplayStage.RunQuestion;
        DebugLog.Print($"PLAYERS IN PURGE #{(GameplayManager.nextPurgeQuestionIndex + 1).ToString()}:", DebugLog.StyleOption.Bold);
        foreach (PlayerObject po in PlayerManager.Get.players.Where(x => x.inPurge))
            DebugLog.Print(po.playerName, DebugLog.StyleOption.Bold, DebugLog.ColorOption.Orange);
    }

    public override void RunQuestion()
    {
        base.RunQuestion();
        StartCoroutine(RunQuestionLate());
        DebugLog.Print($"PURGE #{(GameplayManager.nextPurgeQuestionIndex + 1).ToString()} RESPONSES", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Default);
    }

    public override IEnumerator RunQuestionLate()
    {
        yield return new WaitForSeconds(1f);
        GlobalTimeManager.Get.StartClock();
        questionMesh.text = currentQuestion.questionText;
        answerPrefabs.FirstOrDefault().ToggleAnswer();
        AudioManager.Get.Play(AudioManager.OneShotClip.Whoosh);

        foreach (PlayerObject pl in PlayerManager.Get.players)
            HostManager.Get.SendPayloadToClient(pl, EventLibrary.HostEventType.SimpleQuestion, $"{currentQuestion.questionText}|{(GlobalTimeManager.Get.defaultPurgeTime - 1).ToString()}");

        QuestionRunning();
        yield break;
    }
    public override void QuestionRunning()
    {
        Invoke("OnQuestionEnded", GlobalTimeManager.Get.defaultPurgeTime);
    }

    public override void OnQuestionEnded()
    {
        base.OnQuestionEnded();
    }

    public override void DisplayAnswer()
    {
        ChevronManager.Get.SinglePulse(false);
        AudioManager.Get.Play(AudioManager.OneShotClip.ResetMeter);
        answerPrefabs.FirstOrDefault().SetAnswerColor(AnswerPrefab.Mode.Correct);
        answerPrefabs.FirstOrDefault().mesh.text = currentQuestion.answers.FirstOrDefault().answerText;

        foreach (PlayerObject pl in PlayerManager.Get.players)
        {
            HostManager.Get.SendPayloadToClient(pl, EventLibrary.HostEventType.SingleAndMultiResult, $"The correct answer was {currentQuestion.answers.FirstOrDefault(x => x.isCorrect).answerText}" +
                $"|{(pl.wasCorrect ? "CORRECT" : "INCORRECT")}");

            if (pl.wasCorrect)
                pl.IterateTotal();
        }
        GameplayManager.Get.currentStage = GameplayManager.GameplayStage.RevealCorrect;
    }

    public void RevealCorrectPlayers()
    {
        AudioManager.Get.Play(AudioManager.OneShotClip.Whoosh);
        foreach (PlayerObject po in PlayerManager.Get.players.Where(x => x.inPurge))
        {
            if (po.wasCorrect)
            {
                po.podium.SetPodiumColor(Podium.PodiumMode.Default);
                po.inPurge = false;
                po.ResetSoftPlayerVariables();
            }
            else
            {
                po.flagForCondone = true;
                po.podium.SetPodiumColor(Podium.PodiumMode.Incorrect);
                DebugLog.Print($"{po.playerName}: {(string.IsNullOrEmpty(po.submission) ? "NO ANSWER" : po.submission)}", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Orange);
                //(po.podium as Podium).ToggleSpeechBubble(string.IsNullOrEmpty(po.submission) ? "NO ANSWER" : po.submission);
            }                
        }
    }


    public override void ResetForNewQuestion()
    {
        int elimCount = PlayerManager.Get.players.Where(x => x.inPurge && !x.wasCorrect).Count();
        if(elimCount > 0)
            AudioManager.Get.Play(AudioManager.OneShotClip.Elimination);
        else
            AudioManager.Get.Play(AudioManager.OneShotClip.NoElim);
        foreach (PlayerObject po in PlayerManager.Get.players.Where(x => x.inPurge && !x.wasCorrect))
            po.EliminatePlayer();

        foreach (PlayerObject po in PlayerManager.Get.players.Where(x => !x.eliminated))
            po.purgesSurvived++;

        base.ResetForNewQuestion();
        GameplayManager.nextPurgeQuestionIndex++;
        GameplayManager.nextMainQuestionIndex++;
        PurgeMeterManager.Get.ResetTheMeter();

        if (ReadyForFinal())
        {
            base.TriggerFinalRound();
            return;
        }            
        else
        {
            PurgeMeterManager.Get.IterateBreakpointAndResetToBaseline();
            Invoke("TriggerScrollingTextLate", 2f);
            GameplayManager.Get.currentRound = GameplayManager.Round.MainRound;
            GameplayManager.Get.currentStage = GameplayManager.GameplayStage.LoadQuestion;
        }
    }

    private void TriggerScrollingTextLate()
    {
        AudioManager.Get.Play(AudioManager.OneShotClip.Whoosh);
        TriggerScrollingText($"<size=30%>BREAKPOINT RESET</size>\n{100 - PurgeMeterManager.Get.currentStartingBreakpoint}%");
        AudioManager.Get.Play(AudioManager.OneShotClip.Whoosh, 3.5f);
    }
}
