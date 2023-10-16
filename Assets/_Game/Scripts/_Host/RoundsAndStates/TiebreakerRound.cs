using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TiebreakerRound : RoundBase
{
    public override void LoadQuestion()
    {
        base.LoadQuestion();
        AudioManager.Get.StopLoop();
        AudioManager.Get.Play(AudioManager.OneShotClip.ShortSting);
        AudioManager.Get.Play(AudioManager.LoopClip.Underscore, true, 4.25f);
        TriggerScrollingText("TIEBREAKER");
        ChevronManager.Get.MultiPulse(false);
        currentQuestion = QuestionManager.currentPack.tiebreaker;
        answerPrefabs.Add(Instantiate(answerPrefabToInstance, answerPrefabTarget).GetComponent<AnswerPrefab>());
        answerPrefabs.FirstOrDefault().Init("???");
    }

    public override void RunQuestion()
    {
        base.RunQuestion();
        StartCoroutine(RunQuestionLate());
        DebugLog.Print("TIEBREAKER RESPONSES", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Default);
    }

    public override IEnumerator RunQuestionLate()
    {
        yield return new WaitForSeconds(1f);
        GlobalTimeManager.Get.StartClock();
        questionMesh.text = currentQuestion.questionText;
        answerPrefabs.FirstOrDefault().ToggleAnswer();
        AudioManager.Get.Play(AudioManager.OneShotClip.Whoosh);

        foreach (PlayerObject pl in PlayerManager.Get.players)
        {
            pl.distanceFromTiebreak = int.MaxValue;
            HostManager.Get.SendPayloadToClient(pl, EventLibrary.HostEventType.NumericalQuestion, $"{currentQuestion.questionText}|{(GlobalTimeManager.Get.defaultTiebreakTime - 1).ToString()}");
        }
        QuestionRunning();
        yield break;
    }

    public override void QuestionRunning()
    {
        Invoke("OnQuestionEnded", GlobalTimeManager.Get.defaultTiebreakTime);
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
            HostManager.Get.SendPayloadToClient(pl, EventLibrary.HostEventType.SingleAndMultiResult, $"The correct answer was {currentQuestion.answers.FirstOrDefault(x => x.isCorrect).answerText}|DEFAULT");
        Invoke("PanToMeter", 2f);
    }

    public override void PanToMeter()
    {
        CameraLerpManager.Get.ZoomToPosition(CameraLerpManager.CameraPosition.Meter);
        GameplayManager.Get.currentStage = GameplayManager.GameplayStage.ResetPostQuestion;
    }

    public override void ResetForNewQuestion()
    {
        base.ResetForNewQuestion();
        CameraLerpManager.Get.ZoomToPosition(CameraLerpManager.CameraPosition.Audience, 50f);

        if (ReadyForFinal())
        {
            TriggerFinalRound();
            return;
        }            
        else
        {
            AudioManager.Get.Play(AudioManager.OneShotClip.Whoosh);
            TriggerScrollingText($"<size=30%>BREAKPOINT SET</size>\n{PurgeMeterManager.Get.currentBreakpoint}%");
            AudioManager.Get.Play(AudioManager.OneShotClip.Whoosh, 3.5f);
            GameplayManager.Get.currentRound = GameplayManager.Round.MainRound;
            GameplayManager.Get.currentStage = GameplayManager.GameplayStage.LoadQuestion;
        }
    }
}
