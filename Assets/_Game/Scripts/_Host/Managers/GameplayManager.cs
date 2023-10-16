using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using TMPro;
using System.Linq;

public class GameplayManager : SingletonMonoBehaviour<GameplayManager>
{
    [Header("Rounds")]
    public RoundBase[] rounds;

    [Header("Question Data")]
    public static int nextMainQuestionIndex = 0;
    public static int nextPurgeQuestionIndex = 0;
    public static int nextFinalQuestionIndex = 0;

    public enum GameplayStage
    {
        RunTitles,
        OpenLobby,
        LockLobby,
        RevealInstructions,
        HideInstructions,

        LoadQuestion,
        RunQuestion,
        DisplayAnswer,
        RevealCorrect,

        StartTheMeter,
        ResetPostQuestion,

        RevealFinalists,
        DisplayPreFinalLeadberboard,
        HidePreFinalLeaderboard,
        GoToFinalView,
        LeaderSelectsOrder,
        RunHalfQuestion,
        ChoiceOfElimination,
        EliminateAPlayer,
        RevealWinner,

        DisplayFinalLeaderboard,
        HideFinalLeaderboard,
        RollCredits,
        DoNothing
    };
    public GameplayStage currentStage = GameplayStage.DoNothing;

    public enum Round
    {
        Tiebreaker,
        MainRound,
        PurgeRound,
        FinalRound,
        None
    };
    public Round currentRound = Round.None;
    public int roundsPlayed = 0;

    [Button]
    public void ProgressGameplay()
    {
        switch (currentStage)
        {
            case GameplayStage.RunTitles:
                TitlesManager.Get.RunTitleSequence();
                //If in recovery mode, we need to call Restore Players to restore specific player data (client end should be handled by the reload host call)
                //Also need to call Restore gameplay state to bring us back to where we need to be (skipping titles along the way)
                //Reveal instructions would probably be a sensible place to go to, though check that doesn't iterate any game state data itself
                break;

            case GameplayStage.OpenLobby:
                LobbyManager.Get.OnOpenLobby();
                currentStage++;
                break;

            case GameplayStage.LockLobby:
                //We cannot lock the lobby without at least two players
                if (PlayerManager.Get.players.Count < 2)
                {
                    DebugLog.Print($"At least two players must have joined the game...", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Red);
                    return;
                }
                LobbyManager.Get.OnLockLobby();
                currentStage++;
                break;

            case GameplayStage.RevealInstructions:
                InstructionsManager.Get.OnShowInstructions();
                currentStage++;
                break;

            case GameplayStage.HideInstructions:
                InstructionsManager.Get.OnShowInstructions();
                currentStage++;
                currentRound = Round.Tiebreaker;
                break;

            case GameplayStage.LoadQuestion:
                currentStage++;
                rounds[(int)currentRound].LoadQuestion();
                break;

            case GameplayStage.RunQuestion:
                rounds[(int)currentRound].RunQuestion();
                currentStage = GameplayStage.DoNothing;
                break;

            case GameplayStage.DisplayAnswer:
                currentStage = GameplayStage.DoNothing;
                rounds[(int)currentRound].DisplayAnswer();
                break;

            case GameplayStage.RevealCorrect:
                currentStage = GameplayStage.ResetPostQuestion;
                (rounds[(int)currentRound] as PurgeRound).RevealCorrectPlayers();
                break;

            case GameplayStage.StartTheMeter:
                currentStage = GameplayStage.DoNothing;
                rounds[(int)currentRound].StartTheMeter();
                break;

            case GameplayStage.ResetPostQuestion:
                rounds[(int)currentRound].ResetForNewQuestion();
                break;

            case GameplayStage.RevealFinalists:
                (rounds[(int)currentRound] as FinalRound).RevealFinalists();
                currentStage++;
                break;

            case GameplayStage.DisplayPreFinalLeadberboard:
                (rounds[(int)currentRound] as FinalRound).ShowPreFinalLeaderboard();
                currentStage++;
                break;

            case GameplayStage.HidePreFinalLeaderboard:
                (rounds[(int)currentRound] as FinalRound).HidePreFinalLeaderboard();
                currentStage++;
                break;

            case GameplayStage.GoToFinalView:
                (rounds[(int)currentRound] as FinalRound).GoToFinalView();
                currentStage = GameplayStage.DoNothing;
                break;

            case GameplayStage.LeaderSelectsOrder:
                (rounds[(int)currentRound] as FinalRound).LeaderSelectsOrder();
                currentStage = GameplayStage.DoNothing;
                break;

            case GameplayStage.RunHalfQuestion:
                (rounds[(int)currentRound] as FinalRound).RunHalfQuestion();
                currentStage = GameplayStage.DoNothing;
                break;

            case GameplayStage.ChoiceOfElimination:
                currentStage = GameplayStage.DoNothing;
                (rounds[(int)currentRound] as FinalRound).ChoiceOfElimination();
                break;

            case GameplayStage.EliminateAPlayer:
                currentStage = GameplayStage.DoNothing;
                (rounds[(int)currentRound] as FinalRound).EliminateAPlayer();
                break;

            case GameplayStage.RevealWinner:
                currentStage++;
                (rounds[(int)currentRound] as FinalRound).RevealWinner();
                break;

            case GameplayStage.DisplayFinalLeaderboard:
                currentStage++;
                FinalLeaderboardManager.Get.GenerateFinalLeaderboard();
                break;

            case GameplayStage.HideFinalLeaderboard:
                currentStage++;
                FinalLeaderboardManager.Get.ToggleLeaderboard();
                break;

            case GameplayStage.RollCredits:
                GameplayPennys.Get.UpdatePennysAndMedals();
                CreditsManager.Get.RollCredits();
                currentStage++;
                break;

            case GameplayStage.DoNothing:
                break;
        }
    }
}
