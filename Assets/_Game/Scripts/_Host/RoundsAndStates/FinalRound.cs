using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class FinalRound : RoundBase
{
    public GameObject finalistRowToInstance;
    public GameObject finalistPodiumToInstance;
    public Transform podiumRowTargetTransform;

    public List<GameObject> instantiatedRows = new List<GameObject>();
    public List<FinalPodium> instantiatedFinalPodiums = new List<FinalPodium>();

    public static bool controllerIsSelecting = false;
    public static bool controllerIsSelectingElimTarget = false;
    public static bool descendingOrder = true;
    public static int defaultControllerIndex = 0;

    public List<PlayerObject> finalists = new List<PlayerObject>();

    public TextMeshPro tickerMesh;
    public PlayerObject controller;

    public bool questionWasPassed;
    public PlayerObject passer;

    public void RevealFinalists()
    {
        tickerMesh.text = "";
        AudioManager.Get.Play(AudioManager.OneShotClip.Whoosh);
        LobbyManager.Get.TogglePermaCode();

        int nonEliminatedPlayers = PlayerManager.Get.players.Count(x => !x.eliminated);

        //This code block orders the player by total main game Qs correct then tiebreaker distance, then selects the required number of finalists and flags them as in the final
        foreach (PlayerObject po in PlayerManager.Get.GetOrderedNonEliminatedPlayers()
            .Take(Math.Min(nonEliminatedPlayers, QuestionManager.currentPack.finalGame.Count() + 1)))
            po.inFinal = true;

        //Then toggle speech bubble to reveal scores and tiebreakers, and highlight our finalists
        foreach(PlayerObject po in PlayerManager.Get.players.Where(x => !x.eliminated).OrderByDescending(x => x.mainGameCorrect).ThenBy(x => x.distanceFromTiebreak))
        {
            //(po.podium as Podium).ToggleSpeechBubble($"Qs: {po.mainGameCorrect}\n<size=75%>TB: {po.distanceFromTiebreak}");
            po.podium.SetPodiumColor(po.inFinal ? PodiumBase.PodiumMode.Default : PodiumBase.PodiumMode.Incorrect);
            DebugLog.Print($"{po.playerName}: {po.mainGameCorrect} (TB: {po.distanceFromTiebreak})", DebugLog.StyleOption.Bold, po.inFinal ? DebugLog.ColorOption.Green : DebugLog.ColorOption.Red);
        }
    }

    public void ShowPreFinalLeaderboard()
    {
        FinalLeaderboardManager.Get.GeneratePreFinalLeaderboard();
    }

    public void HidePreFinalLeaderboard()
    {
        FinalLeaderboardManager.Get.ToggleLeaderboard();
    }

    public void GoToFinalView()
    {
        AudioManager.Get.StopLoop();
        AudioManager.Get.Play(AudioManager.OneShotClip.Elimination);
        AudioManager.Get.Play(AudioManager.OneShotClip.LongSting);
        AudioManager.Get.Play(AudioManager.LoopClip.Underscore, true, 9.5f);
        ChevronManager.Get.MultiPulse(true);

        foreach (PlayerObject po in PlayerManager.Get.players)
            HostManager.Get.SendPayloadToClient(po, EventLibrary.HostEventType.Information, "Time for the Final Purge...");

        foreach (PlayerObject po in PlayerManager.Get.players.Where(x => !x.inFinal && !x.eliminated))
            po.EliminatePlayer();

        finalists = PlayerManager.Get.GetOrderedNonEliminatedPlayers();
        foreach (PlayerObject po in finalists)
        {
            po.purgesSurvived++;
            //(po.podium as Podium).ToggleSpeechBubble();
        }            

        int finalistCount = finalists.Count();
        instantiatedRows.Add(Instantiate(finalistRowToInstance, podiumRowTargetTransform));
        if(finalistCount > 3)
            instantiatedRows.Add(Instantiate(finalistRowToInstance, podiumRowTargetTransform));

        podiumRowTargetTransform.GetComponent<DynamicLinearSeparator>().PerformSpread();

        if (instantiatedRows.Count == 1)
        {
            for (int i = 0; i < finalistCount; i++)
                instantiatedFinalPodiums.Add(Instantiate(finalistPodiumToInstance, instantiatedRows.FirstOrDefault().gameObject.transform).GetComponent<FinalPodium>());

            instantiatedRows.FirstOrDefault().GetComponent<DynamicLinearSeparator>().PerformSpread();
        }
                
        else
        {
            for(int i = 0; i < finalistCount; i++)
                instantiatedFinalPodiums.Add(Instantiate(finalistPodiumToInstance, instantiatedRows[i < finalistCount / 2 ? 0 : 1].gameObject.transform).GetComponent<FinalPodium>());

            foreach (GameObject go in instantiatedRows)
                go.GetComponent<DynamicLinearSeparator>().PerformSpread();
        }
        Invoke("PanToFinal", 2f);
    }

    private void PanToFinal()
    {
        for(int i = 0; i < finalists.Count; i++)
        {
            finalists[i].podium = instantiatedFinalPodiums[i];
            instantiatedFinalPodiums[i].InitialisePodium(finalists[i]);
        }            

        CameraLerpManager.Get.ZoomToPosition(CameraLerpManager.CameraPosition.Final, 52f);
        GameplayManager.Get.currentStage = GameplayManager.GameplayStage.LeaderSelectsOrder;
    }

    public void LeaderSelectsOrder()
    {
        //Clear out the answer straps
        base.LoadQuestion();

        controllerIsSelecting = true;
        controller = finalists.FirstOrDefault();
        controller.podium.SetPodiumColor(PodiumBase.PodiumMode.Default);
        ChevronManager.Get.SinglePulse(false);
        AudioManager.Get.Play(AudioManager.OneShotClip.Whoosh);
        questionLozengeAnim.SetTrigger("toggle");
        questionMesh.text = $"{controller.playerName}, you were leading the main game. Would you like to play first or last?";
        ChevronManager.Get.SinglePulse(false);
        HostManager.Get.SendPayloadToClient(controller, EventLibrary.HostEventType.MultipleChoiceQuestion,
            $"Would you like to play FIRST or LAST?|{(GlobalTimeManager.Get.defaultFinalTime - 1).ToString()}|FIRST|LAST");
    }

    public void OrderSelected(bool descending)
    {
        controllerIsSelecting = false;
        ChevronManager.Get.SinglePulse(true);
        AudioManager.Get.Play(AudioManager.OneShotClip.TimeUp);
        descendingOrder = descending;
        defaultControllerIndex = descendingOrder ? 0 : finalists.Count() - 1;
        questionLozengeAnim.SetTrigger("toggle");
        tickerMesh.text = descendingOrder ? $"{controller.playerName} will play first..." : $"{controller.playerName} will play last. {finalists.LastOrDefault().playerName}, you are in control...";

        if(!descendingOrder)
        {
            controller.podium.SetPodiumColor(PodiumBase.PodiumMode.Lowlight);
            controller = finalists.LastOrDefault();
            controller.podium.SetPodiumColor(PodiumBase.PodiumMode.Default);
        }
        GameplayManager.Get.currentStage = GameplayManager.GameplayStage.LoadQuestion;
    }

    public override void LoadQuestion()
    {
        questionWasPassed = false;
        passer = null;
        base.LoadQuestion();
        tickerMesh.text = $"{controller.playerName}, based on half a question, decide whether you would like to play or pass...";
        controller.podium.SetPodiumColor(PodiumBase.PodiumMode.Default);
        currentQuestion = QuestionManager.currentPack.finalGame[GameplayManager.nextFinalQuestionIndex];
        answerPrefabs.Add(Instantiate(answerPrefabToInstance, answerPrefabTarget).GetComponent<AnswerPrefab>());
        answerPrefabs.FirstOrDefault().Init("???");
        GameplayManager.Get.currentStage = GameplayManager.GameplayStage.RunHalfQuestion;
    }

    public void RunHalfQuestion()
    {
        string[] splitQuestion = currentQuestion.questionText.Split('|');
        splitQuestion[0] += "...";
        questionMesh.text = splitQuestion.FirstOrDefault();
        questionLozengeAnim.SetTrigger("toggle");
        AudioManager.Get.Play(AudioManager.OneShotClip.Whoosh);
        ChevronManager.Get.SinglePulse(false);

        List<string> nonControllingPlayers = finalists.Where(x => controller != x && !x.eliminated).Select(x => x.playerName).ToList();
        string mergedString = "PASS TO " + string.Join("|PASS TO ", nonControllingPlayers.ToArray());

        HostManager.Get.SendPayloadToClient(controller, EventLibrary.HostEventType.MultipleChoiceQuestion,
            $"{splitQuestion.FirstOrDefault()}|{(GlobalTimeManager.Get.defaultFinalTime - 1).ToString()}|PLAY|{mergedString}");

        controllerIsSelecting = true;
        Invoke("LateAnswerStrap", 1f);
    }

    private void LateAnswerStrap()
    {
        answerPrefabs.FirstOrDefault().ToggleAnswer();
        AudioManager.Get.Play(AudioManager.OneShotClip.Whoosh);
    }

    public void SetActivePlayer(string decision)
    {
        switch (decision)
        {
            case "PLAY":
                passer = null;
                questionWasPassed = false;
                tickerMesh.text = $"{controller.playerName} will play this question...";
                break;

            default:
                string[] splitDecision = decision.Split(' ');
                controller.podium.SetPodiumColor(PodiumBase.PodiumMode.Lowlight);
                questionWasPassed = true;
                passer = controller;
                controller = finalists.FirstOrDefault(x => x.playerName.ToUpperInvariant().Trim() == splitDecision.LastOrDefault().ToUpperInvariant().Trim());
                controller.podium.SetPodiumColor(PodiumBase.PodiumMode.Default);
                tickerMesh.text = $"The question has been passed to {controller.playerName}...";
                break;
        }
        AudioManager.Get.Play(AudioManager.OneShotClip.TimeUp);
        controllerIsSelecting = false;
        ChevronManager.Get.SinglePulse(true);
        GameplayManager.Get.currentStage = GameplayManager.GameplayStage.RunQuestion;
    }

    public override void RunQuestion()
    {
        ChevronManager.Get.SinglePulse(false);
        AudioManager.Get.Play(AudioManager.OneShotClip.Whoosh);

        foreach (PlayerObject pl in PlayerManager.Get.players)
            HostManager.Get.SendPayloadToClient(pl, EventLibrary.HostEventType.Information, "Get ready for the question...");

        string[] splitQuestion = currentQuestion.questionText.Split('|');
        string joinedQ = string.Join(" ", splitQuestion);
        questionMesh.text = joinedQ;
        GlobalTimeManager.Get.StartClock();

        foreach (PlayerObject pl in PlayerManager.Get.players)
            HostManager.Get.SendPayloadToClient(pl, EventLibrary.HostEventType.SimpleQuestion, $"{joinedQ}|{(GlobalTimeManager.Get.defaultFinalTime - 1).ToString()}");
        
        QuestionRunning();
        DebugLog.Print($"FINAL #{(GameplayManager.nextFinalQuestionIndex + 1).ToString()} RESPONSES", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Default);
    }

    public override void QuestionRunning()
    {
        Invoke("OnQuestionEnded", GlobalTimeManager.Get.defaultFinalTime);
    }

    public override void OnQuestionEnded()
    {
        base.OnQuestionEnded();
        controller.podium.SetPodiumColor(Podium.PodiumMode.Lowlight);
    }

    public override void DisplayAnswer()
    {
        AudioManager.Get.Play(AudioManager.OneShotClip.ResetMeter);
        ChevronManager.Get.SinglePulse(false);
        answerPrefabs.FirstOrDefault().SetAnswerColor(AnswerPrefab.Mode.Correct);
        answerPrefabs.FirstOrDefault().mesh.text = currentQuestion.answers.FirstOrDefault().answerText;

        foreach (PlayerObject pl in PlayerManager.Get.players)
        {
            HostManager.Get.SendPayloadToClient(pl, EventLibrary.HostEventType.SingleAndMultiResult, $"The correct answer was {currentQuestion.answers.FirstOrDefault(x => x.isCorrect).answerText}" +
                $"|{(pl.wasCorrect ? "CORRECT" : "INCORRECT")}");

            if (pl.wasCorrect)
                pl.IterateTotal();
        }

        tickerMesh.text = (controller.wasCorrect ? "<color=green>" : "<color=red>") + (string.IsNullOrEmpty(controller.submission) ? "NO ANSWER" : controller.submission);
        controller.podium.SetPodiumColor(controller.wasCorrect ? PodiumBase.PodiumMode.FinalCorrect : PodiumBase.PodiumMode.Incorrect);
        if (passer != null && controller.wasCorrect)
            passer.podium.SetPodiumColor(PodiumBase.PodiumMode.Incorrect);

        SetStageAtEliminationPhase();
    }

    public void SetStageAtEliminationPhase()
    {
        GameplayManager.Get.currentStage = controller.wasCorrect && !questionWasPassed ? GameplayManager.GameplayStage.ChoiceOfElimination : GameplayManager.GameplayStage.EliminateAPlayer;
    }

    public void ChoiceOfElimination()
    {
        ChevronManager.Get.SinglePulse(false);
        AudioManager.Get.Play(AudioManager.OneShotClip.Whoosh);
        controllerIsSelectingElimTarget = true;
        tickerMesh.text = $"{controller.playerName}, please choose a player to purge...";
        controller.podium.SetPodiumColor(PodiumBase.PodiumMode.Default);
        List<string> nonControllingPlayers = finalists.Where(x => controller != x && !x.eliminated).Select(x => x.playerName).ToList();
        string mergedString = "PURGE " + string.Join("|PURGE ", nonControllingPlayers.ToArray());

        HostManager.Get.SendPayloadToClient(controller, EventLibrary.HostEventType.MultipleChoiceQuestion,
            $"CHOOSE A PLAYER TO PURGE...|{(GlobalTimeManager.Get.defaultFinalTime - 1).ToString()}|{mergedString}");
    }

    public void EliminationChoiceSet(string decision)
    {
        controllerIsSelectingElimTarget = false;

        AudioManager.Get.Play(AudioManager.OneShotClip.TimeUp);

        string[] splitDecision = decision.Split(' ');
        controller.podium.SetPodiumColor(PodiumBase.PodiumMode.Lowlight);

        controller = finalists.FirstOrDefault(x => x.playerName.ToUpperInvariant().Trim() == splitDecision.LastOrDefault().ToUpperInvariant().Trim());
        controller.podium.SetPodiumColor(PodiumBase.PodiumMode.Incorrect);
        tickerMesh.text = $"{controller.playerName} has been chosen to be purged...";

        ChevronManager.Get.SinglePulse(true);
        GameplayManager.Get.currentStage = GameplayManager.GameplayStage.EliminateAPlayer;
    }

    public void EliminateAPlayer()
    {
        ChevronManager.Get.SinglePulse(true);

        AudioManager.Get.Play(AudioManager.OneShotClip.Elimination);

        //Switch back to passer
        if (controller.wasCorrect && questionWasPassed)
            controller = passer;

        foreach (FinalPodium p in instantiatedFinalPodiums.Where(x => x.containedPlayer != null && !x.containedPlayer.eliminated))
            p.SetPodiumColor(PodiumBase.PodiumMode.Lowlight);

        controller.EliminatePlayer();

        foreach (PlayerObject po in finalists.Where(x => !x.eliminated))
            po.purgesSurvived++;

        tickerMesh.text = $"{controller.playerName} was purged...";

        SetNextPlayerInSequence();

        base.ResetForNewQuestion();
        GameplayManager.nextFinalQuestionIndex++;

        GameplayManager.Get.currentStage = finalists.Count(x => x.inFinal) == 1 ? GameplayManager.GameplayStage.RevealWinner : GameplayManager.GameplayStage.LoadQuestion;
    }

    public void RevealWinner()
    {
        AudioManager.Get.StopLoop();
        AudioManager.Get.Play(AudioManager.OneShotClip.LongSting);

        DebugLog.Print($"{finalists.FirstOrDefault(x => !x.eliminated).playerName} has survived The Purge!", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Green);
        tickerMesh.text = $"{finalists.FirstOrDefault(x => !x.eliminated).playerName} has survived The Purge";
        ChevronManager.Get.MultiPulse(false);

        foreach (PlayerObject po in PlayerManager.Get.players)
            HostManager.Get.SendPayloadToClient(po, EventLibrary.HostEventType.Information, $"Thanks for playing\n\nYou earned {po.totalCorrect * GameplayPennys.Get.multiplyFactor} Pennys this game");
    }

    private void SetNextPlayerInSequence()
    {
        do
            defaultControllerIndex = (defaultControllerIndex + (descendingOrder ? 1 : finalists.Count() - 1)) % finalists.Count();
        while (finalists[defaultControllerIndex].eliminated);

        controller = finalists[defaultControllerIndex];
    }
}
