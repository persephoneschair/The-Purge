using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MainRound : RoundBase
{
    public override void LoadQuestion()
    {
        base.LoadQuestion();
        currentQuestion = QuestionManager.currentPack.mainGame[GameplayManager.Get.nextMainQuestionIndex];
        for (int i = 0; i < currentQuestion.answers.Count; i++)
        {
            answerPrefabs.Add(Instantiate(answerPrefabToInstance, answerPrefabTarget).GetComponent<AnswerPrefab>());
            answerPrefabs[i].Init(currentQuestion.answers[i].answerText);
        }
        GameplayManager.Get.ProgressGameplay();
    }

    public override void RunQuestion()
    {
        base.RunQuestion();
        StartCoroutine(RunQuestionLate());
        DebugLog.Print($"QUESTION #{(GameplayManager.Get.nextMainQuestionIndex + 1).ToString()} RESPONSES", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Default);
    }

    public override IEnumerator RunQuestionLate()
    {
        yield return new WaitForSeconds(1f);
        foreach (AnswerPrefab a in answerPrefabs)
        {
            a.ToggleAnswer();
            AudioManager.Get.Play(AudioManager.OneShotClip.Whoosh);
            yield return new WaitForSeconds(2f);
        }
        GlobalTimeManager.Get.StartClock();
        questionMesh.text = currentQuestion.questionText;

        foreach (PlayerObject pl in PlayerManager.Get.players)
            HostManager.Get.SendPayloadToClient(pl,
                EventLibrary.HostEventType.MultipleChoiceQuestion,
                $"{currentQuestion.questionText}|{(GlobalTimeManager.Get.defaultMainGameTime - 1).ToString()}|{string.Join("|", currentQuestion.answers.Select(x => x.answerText))}");
        QuestionRunning();
        yield break;
    }

    public override void QuestionRunning()
    {
        Invoke("OnQuestionEnded", GlobalTimeManager.Get.defaultMainGameTime);
    }

    public override void OnQuestionEnded()
    {
        base.OnQuestionEnded();
    }

    public override void DisplayAnswer()
    {
        ChevronManager.Get.SinglePulse(false);
        AudioManager.Get.Play(AudioManager.OneShotClip.ResetMeter);
        for (int i = 0; i < currentQuestion.answers.Count; i++)
        {
            answerPrefabs[i].SetAnswerColor(currentQuestion.answers[i].isCorrect ? AnswerPrefab.Mode.Correct : AnswerPrefab.Mode.Incorrect);
            answerPrefabs[i].mesh.text = currentQuestion.answers[i].answerText;
        }

        foreach (PlayerObject pl in PlayerManager.Get.players)
        {
            HostManager.Get.SendPayloadToClient(pl, EventLibrary.HostEventType.SingleAndMultiResult,
                $"The correct answer was {currentQuestion.answers.FirstOrDefault(x => x.isCorrect).answerText}" +
                $"|{(pl.wasCorrect ? "CORRECT" : "INCORRECT")}");

            if (pl.wasCorrect)
                pl.IterateTotal();
        }            
        Invoke("PanToMeter", 2f);
    }

    public override void PanToMeter()
    {
        CameraLerpManager.Get.ZoomToPosition(CameraLerpManager.CameraPosition.Meter);
        GameplayManager.Get.currentStage = GameplayManager.GameplayStage.StartTheMeter;
    }

    public override void StartTheMeter()
    {
        PurgeMeterManager.Get.StartTheMeter();
        GameplayManager.Get.currentStage = GameplayManager.GameplayStage.DoNothing;
    }

    public override void ResetForNewQuestion()
    {
        base.ResetForNewQuestion();
        GameplayManager.Get.nextMainQuestionIndex++;
        CameraLerpManager.Get.ZoomToPosition(CameraLerpManager.CameraPosition.Audience, 50f);
        PurgeMeterManager.Get.ResetTheMeter();

        if(ReadyForFinal())
        {
            base.TriggerFinalRound();
            return;
        }
        else
        {
            GameplayManager.Get.currentStage = GameplayManager.GameplayStage.LoadQuestion;
            AudioManager.Get.Play(AudioManager.OneShotClip.Whoosh);

            int newBreakpoint = Mathf.CeilToInt(100 - (PurgeMeterManager.Get.currentBreakpoint + PurgeMeterManager.Get.breakpointReductionInterval));
            if (newBreakpoint < 0)
                newBreakpoint = 0;

            string text = "BREAKPOINT LOWERED";
            if (PurgeMeterManager.Get.currentBreakpoint >= 100)
                text = "BREAKPOINT SET";

            TriggerScrollingText($"<size=30%>{text}</size>\n{newBreakpoint}%");
            AudioManager.Get.Play(AudioManager.OneShotClip.Whoosh, 3.5f);
            PurgeMeterManager.Get.LowerBreakpoint();
        }
    }
}
