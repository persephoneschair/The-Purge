using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using NaughtyAttributes;
using System.Linq;

public class GameplayPennys : SingletonMonoBehaviour<GameplayPennys>
{
    private PlayerShell playerList;
    private MedalTableObject medalList;
    private readonly string path = @"D:\Unity Projects\HerokuPennyData\PennyStorage";

    public int authorPennys;
    [Range (1, 50)] public int multiplyFactor;
    public string gameName;


    [Button]
    public void UpdatePennysAndMedals()
    {
        AwardPennys();
        if(!string.IsNullOrEmpty(gameName))
            AwardMedals();
        WriteNewFile();
    }

    private void LoadJSON()
    {
        playerList = JsonConvert.DeserializeObject<PlayerShell>(File.ReadAllText(path + @"\NewPennys.txt"));
    }

    private void LoadMedalJSON()
    {
        medalList = JsonConvert.DeserializeObject<MedalTableObject>(File.ReadAllText(path + $@"\{gameName}.txt"));
    }

    private void AwardPennys()
    {
        List<PlayerObject> list = PlayerManager.Get.players.OrderByDescending(p => p.totalCorrect).ThenBy(p => p.twitchName).Where(x => x.totalCorrect > 0).ToList();
        PlayerPennyData ppd;

        LoadJSON();
        foreach (PlayerObject p in list)
        {
            ppd = playerList.playerList.FirstOrDefault(x => x.PlayerName.ToLowerInvariant() == p.twitchName.ToLowerInvariant());
            if (ppd == null)
                CreateNewPlayer(p);
            else
            {
                ppd.CurrentSeasonPennys += (p.totalCorrect * multiplyFactor);
                ppd.AllTimePennys += (p.totalCorrect * multiplyFactor);
            }
        }

        ppd = null;
        ppd = playerList.playerList.FirstOrDefault(x => x.PlayerName.ToLowerInvariant() == QuestionManager.currentPack.author.ToLowerInvariant());
        if (ppd == null)
            CreateNewAuthor(QuestionManager.currentPack.author.ToLowerInvariant());
        else
        {
            ppd.CurrentSeasonPennys += authorPennys;
            ppd.AllTimePennys += authorPennys;
            ppd.AuthorCredits++;
        }
    }

    private void AwardMedals()
    {
        PlayerObject winner = PlayerManager.Get.players.FirstOrDefault(x => !x.eliminated);
        if (winner == null)
            return;

        LoadMedalJSON();
        medalList.goldMedallists.Add(winner.twitchName.ToLowerInvariant());
    }

    private void CreateNewPlayer(PlayerObject p)
    {
        PlayerPennyData newP = new PlayerPennyData()
        {
            PlayerName = p.twitchName.ToLowerInvariant(),
            CurrentSeasonPennys = (p.totalCorrect * multiplyFactor),
            AllTimePennys = (p.totalCorrect * multiplyFactor)
        };
        playerList.playerList.Add(newP);
    }

    private void CreateNewAuthor(string p)
    {
        PlayerPennyData newP = new PlayerPennyData()
        {
            PlayerName = p,
            CurrentSeasonPennys = authorPennys,
            AllTimePennys = authorPennys,
            AuthorCredits = 1
        };
        playerList.playerList.Add(newP);
    }

    private void WriteNewFile()
    {
        string pennyPath = Operator.Get.testMode ? path + @"\NewPennysTest.txt" : path + @"\NewPennys.txt";
        string medalPath = Operator.Get.testMode ? path + $@"\{gameName}Test.txt" : path + $@"\{gameName}.txt";

        string newDataContent = JsonConvert.SerializeObject(playerList);
        File.WriteAllText(pennyPath, newDataContent);

        if (!string.IsNullOrEmpty(gameName))
        {
            newDataContent = JsonConvert.SerializeObject(medalList);
            File.WriteAllText(medalPath, newDataContent);
        }

        if (Operator.Get.testMode)
            DebugLog.Print("TEST DATA WRITTEN TO DRIVE", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Orange);
        else
            DebugLog.Print("DATA WRITTEN TO DRIVE", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Green);
    }
}
