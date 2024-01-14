using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class SaveManager
{
    public static List<PlayerObjectSerializable> backupDataList = new List<PlayerObjectSerializable>();
    public static GameplayDataSerializable gameplayData = new GameplayDataSerializable();

    public static void BackUpData()
    {
        backupDataList.Clear();
        foreach(PlayerObject plO in PlayerManager.Get.players)
            backupDataList.Add(NewPlayer(plO));

        var playerData = JsonConvert.SerializeObject(backupDataList);

        File.WriteAllText(Application.persistentDataPath + "\\BackUpData.txt", playerData);

        GameplayDataSerializable gpd = new GameplayDataSerializable()
        {
            nextMainGameQuestionIndex = GameplayManager.Get.nextMainQuestionIndex,
            nextPurgeQuestionIndex = GameplayManager.Get.nextPurgeQuestionIndex,
            nextFinalQuestionIndex = GameplayManager.Get.nextFinalQuestionIndex,
            currentStage = GameplayManager.Get.currentStage,
            currentRound = GameplayManager.Get.currentRound,

            currentBreakpoint = 100f - PurgeMeterManager.Get.currentBreakpoint,
            currentStartingBreakpoint = PurgeMeterManager.Get.currentStartingBreakpoint,

            roundsPlayed = GameplayManager.Get.roundsPlayed
        };

        var gameStateData = JsonConvert.SerializeObject(gpd);

        File.WriteAllText(Application.persistentDataPath + "\\GameplayData.txt", gameStateData);
    }

    public static PlayerObjectSerializable NewPlayer(PlayerObject playerObject)
    {
        PlayerObjectSerializable pl = new PlayerObjectSerializable();
        pl.playerClientID = playerObject.playerClientID;

        pl.playerName = playerObject.playerName;
        pl.twitchName = playerObject.twitchName;
        pl.eliminated = playerObject.eliminated;

        pl.points = playerObject.points;
        pl.totalCorrect = playerObject.totalCorrect;

        pl.distanceFromTiebreak = playerObject.distanceFromTiebreak;
        pl.totalCorrect = playerObject.totalCorrect;
        pl.mainGameCorrect = playerObject.mainGameCorrect;
        pl.purgesSurvived = playerObject.purgesSurvived;

        pl.submission = playerObject.submission;
        pl.submissionTime = playerObject.submissionTime;
        pl.flagForCondone = playerObject.flagForCondone;
        pl.wasCorrect = playerObject.wasCorrect;
        pl.inPurge = playerObject.inPurge;
        pl.inFinal = playerObject.inFinal;

        return pl;
    }

    public static void RestoreData()
    {
        backupDataList.Clear();

        Operator.Get.skipOpeningTitles = true;
        GameplayManager.Get.ProgressGameplay();

        if(File.Exists(Application.persistentDataPath + "\\BackUpData.txt"))
            backupDataList = JsonConvert.DeserializeObject<List<PlayerObjectSerializable>>(File.ReadAllText(Application.persistentDataPath + "\\BackUpData.txt"));
        if (File.Exists(Application.persistentDataPath + "\\GameplayData.txt"))
            gameplayData = JsonConvert.DeserializeObject<GameplayDataSerializable>(File.ReadAllText(Application.persistentDataPath + "\\GameplayData.txt"));

        RestoreGameplayState();
    }

    public static void RestorePlayer(PlayerObject po)
    {
        PlayerObjectSerializable rc = backupDataList.FirstOrDefault(x => x.playerClientID.ToLowerInvariant() == po.playerClientID.ToLowerInvariant());

        if(rc != null)
        {
            po.playerName = rc.playerName;
            po.twitchName = rc.twitchName;
            po.eliminated = rc.eliminated;

            po.points = rc.points;
            po.totalCorrect = rc.totalCorrect;

            po.distanceFromTiebreak = rc.distanceFromTiebreak;
            po.totalCorrect = rc.totalCorrect;
            po.purgesSurvived = rc.purgesSurvived;
            po.mainGameCorrect = rc.mainGameCorrect;

            po.submission = rc.submission;
            po.submissionTime = rc.submissionTime;
            po.flagForCondone = rc.flagForCondone;
            po.wasCorrect = rc.wasCorrect;
            po.inPurge = rc.inPurge;
            po.inFinal = rc.inFinal;
        }
    }

    public static void UpdateAllPlayerPodia()
    {
        foreach(PlayerObject obj in PlayerManager.Get.players)
        {
            
        }
        BackUpData();
    }

    public static void RestoreGameplayState()
    {
        if(gameplayData != null)
        {
            GameplayManager.Get.nextMainQuestionIndex = gameplayData.nextMainGameQuestionIndex;
            GameplayManager.Get.nextPurgeQuestionIndex = gameplayData.nextPurgeQuestionIndex;
            GameplayManager.Get.nextFinalQuestionIndex = gameplayData.nextFinalQuestionIndex;

            GameplayManager.Get.currentStage = gameplayData.currentStage;
            GameplayManager.Get.currentRound = gameplayData.currentRound;

            GameplayManager.Get.roundsPlayed = gameplayData.roundsPlayed;
        }
        Operator.Get.RecoveryCompleted();
    }
}
