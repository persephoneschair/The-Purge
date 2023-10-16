using Control;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerObject
{
    public string playerClientID;
    public Player playerClientRef;
    public PodiumBase podium;
    public string otp;
    public string playerName;

    public GlobalLeaderboardStrap strap;
    public GlobalLeaderboardStrap cloneStrap;

    public string twitchName;
    public Texture profileImage;

    public bool eliminated;

    public int distanceFromTiebreak;
    public int points;
    public int totalCorrect;
    public int mainGameCorrect;
    public int purgesSurvived;
    public string submission;
    public float submissionTime;
    public bool flagForCondone;
    public bool wasCorrect;
    public bool inPurge;
    public bool inFinal;

    public PlayerObject(Player pl, string name)
    {
        playerClientRef = pl;
        otp = OTPGenerator.GenerateOTP();
        playerName = name;
        points = 0;
        podium = Podiums.Get.podia.Where(x => x.containedPlayer == null).ToList().PickRandom();
        podium.containedPlayer = this;
    }

    public void ApplyProfilePicture(string name, Texture tx, bool bypassSwitchAccount = false)
    {
        //Player refreshs and rejoins the same game
        if (PlayerManager.Get.players.Count(x => (!string.IsNullOrEmpty(x.twitchName)) && x.twitchName.ToLowerInvariant() == name.ToLowerInvariant()) > 0 && !bypassSwitchAccount)
        {
            PlayerObject oldPlayer = PlayerManager.Get.players.FirstOrDefault(x => x.twitchName.ToLowerInvariant() == name.ToLowerInvariant());
            if (oldPlayer == null)
                return;

            HostManager.Get.SendPayloadToClient(oldPlayer, EventLibrary.HostEventType.SecondInstance, "");

            oldPlayer.playerClientID = playerClientID;
            oldPlayer.playerClientRef = playerClientRef;
            oldPlayer.playerName = playerName;
            oldPlayer.podium.playerNameMesh.text = playerName;

            otp = "";
            podium.containedPlayer = null;
            podium = null;
            playerClientRef = null;
            playerName = "";

            if (PlayerManager.Get.pendingPlayers.Contains(this))
                PlayerManager.Get.pendingPlayers.Remove(this);

            HostManager.Get.SendPayloadToClient(oldPlayer, EventLibrary.HostEventType.Validated, $"{oldPlayer.playerName}|{oldPlayer.points.ToString()}|{oldPlayer.twitchName}");
            //HostManager.Get.UpdateLeaderboards();
            return;
        }
        otp = "";
        twitchName = name.ToLowerInvariant();
        profileImage = tx;
        //podium.avatarRend.material.mainTexture = profileImage;
        if(!LobbyManager.Get.lateEntry)
            podium.InitialisePodium();

        else
        {
            distanceFromTiebreak = int.MaxValue;
            points = 0;
            eliminated = true;
            podium.containedPlayer = null;
        }
        HostManager.Get.SendPayloadToClient(this, EventLibrary.HostEventType.Validated, $"{playerName}|Correct: {totalCorrect}|{twitchName}");
        PlayerManager.Get.players.Add(this);
        PlayerManager.Get.pendingPlayers.Remove(this);
        PlayerManager.Get.UpdatePlayerCount();
        SaveManager.BackUpData();
        //LeaderboardManager.Get.PlayerHasJoined(this);
        //HostManager.GetHost.UpdateLeaderboards();
    }

    public void HandlePlayerScoring(string[] submittedAnswers)
    {
        if(!eliminated && GameplayManager.Get.currentRound != GameplayManager.Round.FinalRound)
            podium.SetPodiumColor(Podium.PodiumMode.Lowlight);

        switch(GameplayManager.Get.currentRound)
        {
            case GameplayManager.Round.Tiebreaker:
                if (int.TryParse(submittedAnswers.FirstOrDefault(), out int response) && int.TryParse(GameplayManager.Get.rounds[(int)GameplayManager.Get.currentRound].currentQuestion.answers.FirstOrDefault().answerText, out int tb))
                    distanceFromTiebreak = Mathf.Abs(response - tb);
                else
                    distanceFromTiebreak = int.MaxValue;

                if(!eliminated)
                    AudioManager.Get.Play(AudioManager.OneShotClip.AnswerReceivedAndLobbyEntry);
                DebugAnswer(distanceFromTiebreak);
                break;

            case GameplayManager.Round.MainRound:
                submission = submittedAnswers.FirstOrDefault();
                submissionTime = GlobalTimeManager.Get.elapsedTime;

                if (submission == GameplayManager.Get.rounds[(int)GameplayManager.Get.currentRound].currentQuestion.answers.FirstOrDefault(x => x.isCorrect).answerText)
                    wasCorrect = true;

                if (!eliminated)
                    AudioManager.Get.Play(AudioManager.OneShotClip.AnswerReceivedAndLobbyEntry);

                DebugAnswer();
                break;

            case GameplayManager.Round.PurgeRound:
                submission = submittedAnswers.FirstOrDefault();
                submissionTime = GlobalTimeManager.Get.elapsedTime;
                wasCorrect = Extensions.Spellchecker(submission,
                    GameplayManager.Get.rounds[(int)GameplayManager.Get.currentRound].currentQuestion.answers
                    .Where(x => x.isCorrect)
                    .Select(x => x.answerText)
                    .ToList());

                if (inPurge)
                    AudioManager.Get.Play(AudioManager.OneShotClip.AnswerReceivedAndLobbyEntry);

                DebugAnswer(submission);
                break;

            case GameplayManager.Round.FinalRound:
                if(FinalRound.controllerIsSelectingElimTarget)
                    (GameplayManager.Get.rounds[(int)GameplayManager.Get.currentRound] as FinalRound).EliminationChoiceSet(submittedAnswers.FirstOrDefault());
                else if (FinalRound.controllerIsSelecting)
                {
                    if(submittedAnswers.FirstOrDefault() == "FIRST" || submittedAnswers.FirstOrDefault() == "LAST")
                        (GameplayManager.Get.rounds[(int)GameplayManager.Get.currentRound] as FinalRound).OrderSelected(submittedAnswers.FirstOrDefault() == "FIRST" ? true : false);
                    else
                        (GameplayManager.Get.rounds[(int)GameplayManager.Get.currentRound] as FinalRound).SetActivePlayer(submittedAnswers.FirstOrDefault());
                }
                    
                else
                {
                    submission = submittedAnswers.FirstOrDefault();
                    submissionTime = GlobalTimeManager.Get.elapsedTime;
                    wasCorrect = Extensions.Spellchecker(submission,
                        GameplayManager.Get.rounds[(int)GameplayManager.Get.currentRound].currentQuestion.answers
                        .Where(x => x.isCorrect)
                        .Select(x => x.answerText)
                        .ToList());

                    if (this == (GameplayManager.Get.rounds[(int)GameplayManager.Get.currentRound] as FinalRound).controller)
                    {
                        AudioManager.Get.Play(AudioManager.OneShotClip.AnswerReceivedAndLobbyEntry);
                        podium.SetPodiumColor(PodiumBase.PodiumMode.Lowlight);
                        DebugAnswer(true);
                        if(!wasCorrect)
                            flagForCondone = true;
                    }
                    else
                        DebugAnswer();
                }
                break;

            default:
                break;
        }
    }

    public void ResetSoftPlayerVariables()
    {
        submission = "";
        submissionTime = 0;
        wasCorrect = false;
        flagForCondone = false;
    }

    public void EliminatePlayer()
    {
        eliminated = true;
        inPurge = false;
        flagForCondone = false;
        podium.SetPodiumColor(Podium.PodiumMode.Eliminated);
        if (!inFinal)
        {
            //(podium as Podium).ToggleSpeechBubble();
            podium.avatarRend.gameObject.SetActive(false);
        }
        else
        {
            podium.avatarRend.material = (podium as FinalPodium).grayscaleMat;
            podium.avatarRend.material.mainTexture = profileImage;
        }
        inFinal = false;
        podium.playerNameMesh.text = "";
        podium.containedPlayer = null;
    }

    public void IterateTotal()
    {
        totalCorrect++;
        if (GameplayManager.Get.currentRound == GameplayManager.Round.MainRound)
            mainGameCorrect++;
        HostManager.Get.SendPayloadToClient(this, EventLibrary.HostEventType.UpdateScore, $"Correct: {totalCorrect}");
    }

    public void DebugAnswer(int tb)
    {
        DebugLog.Print($"{playerName}: {tb} away ({GlobalTimeManager.Get.elapsedTime.ToString("#0.00")}s)", eliminated ? DebugLog.StyleOption.Italic : DebugLog.StyleOption.Bold, DebugLog.ColorOption.Blue);
    }

    public void DebugAnswer()
    {
        DebugLog.Print($"{playerName}: {submission} ({submissionTime.ToString("#0.00")}s)", eliminated ? DebugLog.StyleOption.Italic : DebugLog.StyleOption.Bold, wasCorrect ? DebugLog.ColorOption.Green : DebugLog.ColorOption.Red);
    }

    public void DebugAnswer(string purgeAns)
    {
        DebugLog.Print($"{playerName}: {purgeAns} ({submissionTime.ToString("#0.00")}s)", !inPurge ? DebugLog.StyleOption.Italic : DebugLog.StyleOption.Bold, wasCorrect ? DebugLog.ColorOption.Green : DebugLog.ColorOption.Red);
    }

    public void DebugAnswer(bool finalController)
    {
        DebugLog.Print($"{playerName}: {submission} ({submissionTime.ToString("#0.00")}s)", DebugLog.StyleOption.BoldItalic, wasCorrect ? DebugLog.ColorOption.Blue : DebugLog.ColorOption.Orange);
    }
}
