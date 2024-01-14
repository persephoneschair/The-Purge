using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.Linq;

public class Operator : SingletonMonoBehaviour<Operator>
{
    [Header("Game Settings")]
    [Tooltip("Supresses Twitch chat messages and will store Pennys and medals in a separate test file")]
    public bool testMode;
    [Tooltip("Skips opening titles")]
    public bool skipOpeningTitles;
    [Tooltip("Players must join the room with valid Twitch username as their name; this will skip the process of validation")]
    public bool fastValidation;
    [Tooltip("Start the game in recovery mode to restore any saved data from a previous game crash")]
    public bool recoveryMode;
    [Tooltip("Limits the number of accounts that may connect to the room (set to 0 for infinite)")]
    [Range(0, 100)] public int playerLimit;

    [Header("Quesion Data")]
    public TextAsset questionPack;
    public TextAsset legacyQuestionPack;
    public string legacyAuthorName;

    [TextArea(5, 10)] public string exportedLegacyPack;

    public override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        if (recoveryMode)
        {
            skipOpeningTitles = true;
            HostManager.Get.host.ReloadHost = true;
        }

        HostManager.Get.host.Init();

        if (questionPack != null)
            QuestionManager.DecompilePack(questionPack);
        else if (legacyQuestionPack != null)
            QuestionManager.ConvertPackFromLegacyTemplate(legacyQuestionPack, legacyAuthorName);
        else
            DebugLog.Print("NO QUESTION PACK LOADED; PLEASE ASSIGN ONE AND RESTART THE BUILD", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Red);

        DataStorage.CreateDataPath();
        GameplayEvent.Log("Game initiated");
        //HotseatPlayerEvent.Log(PlayerObject, "");
        //AudiencePlayerEvent.Log(PlayerObject, "");
        EventLogger.PrintLog();

        if (recoveryMode)
            SaveManager.RestoreData();
    }

    [Button]
    public void ProgressGameplay()
    {
        if (questionPack != null)
            GameplayManager.Get.ProgressGameplay();
    }

    [Button]
    public void Save()
    {
        SaveManager.BackUpData();
    }

    public void RecoveryCompleted()
    {
        Invoke("LockDelay", 5f);
    }

    private void LockDelay()
    {
        LobbyManager.Get.OnLockLobby();
        recoveryMode = false;
        HostManager.Get.host.ReloadHost = false;
        skipOpeningTitles = false;

        PurgeMeterManager.Get.currentBreakpoint = SaveManager.gameplayData.currentBreakpoint;
        PurgeMeterManager.Get.currentStartingBreakpoint = SaveManager.gameplayData.currentStartingBreakpoint;

        PurgeMeterManager.Get.setManualBreakPoint = 100 - PurgeMeterManager.Get.currentBreakpoint;
        PurgeMeterManager.Get.SetManualBreakpoint();
    }
}
