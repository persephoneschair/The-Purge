using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PurgeMeterManager : SingletonMonoBehaviour<PurgeMeterManager>
{
    [Header("Breakpoint Settings")]
    public float breakpointResetInterval = 10f;
    public float breakpointReductionInterval = 10f;

    [Header("Current Breakpoint")]
    [ShowOnly] public float currentBreakpoint = 50f;
    [ShowOnly] public float currentStartingBreakpoint = 50f;
    private int currentBreakpointInt = 50;

    [Header("Current Result")]
    [ShowOnly] public float percentageWrongResult = 0f;
    [ShowOnly] public float currentPercentageWrong = 0f;
    private int currentPercentageWrongInt = 0;
    private bool purgeTriggered;

    [Header("Scene Objects")]
    public MeterLerper breakpoint;
    public MeterLerper bar;
    //public TextMeshPro currentBreakpointMesh;
    //public TextMeshPro currentPercentageWrongMesh;
    public Renderer[] floorBars;
    public Material[] floorBarMats;

    public Animator studioRotateAnim;

    private void Update()
    {
        currentBreakpointInt = (int)currentBreakpoint;
        currentPercentageWrongInt = (int)currentPercentageWrong;

        ColoriseFloorBars();

        if (breakpoint.isMoving)
        {
            currentBreakpoint = breakpoint.gameObject.transform.localScale.y * 100f;
            //currentBreakpointMesh.text = (100 - currentBreakpoint).ToString("#0") + "%";
        }

        if(bar.isMoving)
        {
            currentPercentageWrong = bar.gameObject.transform.localScale.y * 100f;
            //currentPercentageWrongMesh.text = currentPercentageWrong == 0 ? "0%" : currentPercentageWrong.ToString("#0") + "%";
        }
    }

    private void Start()
    {
        breakpoint.ResetTheBar(currentBreakpoint);
        bar.ResetTheBar();
        //currentBreakpointMesh.text = (100 - currentBreakpoint).ToString("#0") + "%";
        foreach (Renderer r in floorBars)
            r.material = floorBarMats[0];
    }

    [Header("Override Settings")]
    public float setManualBreakPoint;
    public float setManualPercentageWrong;

    #region Breakpoint

    [Button]
    public void SetManualBreakpoint()
    {
        SetManualBreakpoint(setManualBreakPoint);
    }
    public void SetManualBreakpoint(float newBreakpoint)
    {
        breakpoint.ResetTheBar(newBreakpoint);
    }

    [Button]
    public void LowerBreakpoint()
    {
        if (currentBreakpoint + breakpointReductionInterval > 100)
            return;
        breakpoint.ResetTheBar(currentBreakpoint + breakpointReductionInterval);
    }

    [Button]
    public void IterateBreakpointAndResetToBaseline()
    {
        currentStartingBreakpoint -= breakpointResetInterval;
        breakpoint.ResetTheBar(currentStartingBreakpoint);
    }

    #endregion

    #region Bar

    private float CalculatePercentageWrong()
    {
        int playersWrong = PlayerManager.Get.players.Count(x => !x.eliminated && !x.wasCorrect);
        int totalPlayers = PlayerManager.Get.players.Count(x => !x.eliminated);
        return ((float)playersWrong / (float)totalPlayers) * 100f;
    }

    public void StartTheMeter()
    {
        AudioManager.Get.StopLoop();
        AudioManager.Get.Play(AudioManager.LoopClip.Meter, true);
        //HouseLightsManager.Get.ToggleStudioLights();
        DebugLog.Print("RUNNING THE METER...", DebugLog.StyleOption.Bold);
        StartCoroutine(MeterRoutine(CalculatePercentageWrong()));
        int cbp = Mathf.CeilToInt(100f - currentBreakpoint);
        DebugLog.Print($"The breakpoint is {cbp}%...", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Orange);
        DebugLog.Print($"{CalculatePercentageWrong().ToString("#0")}% were incorrect...", DebugLog.StyleOption.Bold, CalculatePercentageWrong() > cbp ? DebugLog.ColorOption.Red : DebugLog.ColorOption.Green);
        purgeTriggered = CalculatePercentageWrong() > cbp;
    }

    IEnumerator MeterRoutine(float percentageWrong)
    {
        percentageWrongResult = percentageWrong;
        ChevronManager.Get.SinglePulse(true);
        yield return new WaitForSeconds(1f);
        CameraLerpManager.Get.zoomAnim.enabled = true;
        CameraLerpManager.Get.zoomAnim.SetBool("zooming", true);
        studioRotateAnim.enabled = true;
        studioRotateAnim.SetBool("rotating", true);
        //CameraLerpManager.Get.CalculateXRotAndFovForZoom(percentageWrongResult > (100 - currentBreakpoint) ? 100 - currentBreakpoint : percentageWrongResult);
        bar.MoveTheBar(percentageWrongResult > (100 - currentBreakpoint) ? 100 - currentBreakpoint : percentageWrongResult);
        yield return new WaitUntil(() => !bar.isMoving);
        CameraLerpManager.Get.zoomAnim.SetBool("zooming", false);
        studioRotateAnim.SetBool("rotating", false);
        //currentPercentageWrongMesh.text = percentageWrongResult.ToString("#0") + "%";
        DisplayResult();
        yield return new WaitForSeconds(1f);
        CameraLerpManager.Get.zoomAnim.enabled = false;
        studioRotateAnim.enabled = false;
    }

    private void DisplayResult()
    {
        //HouseLightsManager.Get.ToggleStudioLights();
        CalculateAnswerPercentages();
        if (!purgeTriggered)
        {
            AudioManager.Get.StopLoop();
            AudioManager.Get.Play(AudioManager.OneShotClip.ShortSting);
            AudioManager.Get.Play(AudioManager.LoopClip.Underscore, true, 4.25f);

            //GameplayManager.Get.rounds.FirstOrDefault().TriggerScrollingText($"<size=30%>{CalculatePercentageWrong().ToString("#0")}% INCORRECT</size>\n<color=green>PURGE AVOIDED");
            GameplayManager.Get.rounds.FirstOrDefault().TriggerScrollingText($"<color=green>PURGE AVOIDED");
            ChevronManager.Get.MultiPulse(false);
            GameplayManager.Get.currentStage = GameplayManager.GameplayStage.ResetPostQuestion;
            DebugLog.Print("PURGE AVOIDED", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Green);
        }
        else
        {
            AudioManager.Get.StopLoop();
            AudioManager.Get.Play(AudioManager.OneShotClip.LongSting);
            AudioManager.Get.Play(AudioManager.LoopClip.Underscore, true, 9.5f);

            purgeTriggered = false;
            //GameplayManager.Get.rounds.FirstOrDefault().TriggerScrollingText($"<size=30%>{CalculatePercentageWrong().ToString("#0")}% INCORRECT</size>\n<color=#FF8D8D>PURGE TRIGGERED");
            GameplayManager.Get.rounds.FirstOrDefault().TriggerScrollingText($"<color=#FF8D8D>PURGE TRIGGERED");
            ChevronManager.Get.MultiPulse(true);
            GameplayManager.Get.currentRound = GameplayManager.Round.PurgeRound;
            GameplayManager.Get.currentStage = GameplayManager.GameplayStage.LoadQuestion;
            DebugLog.Print($"PURGE #{(GameplayManager.nextPurgeQuestionIndex + 1).ToString()} TRIGGERED", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Red);
        }
    }

    private void CalculateAnswerPercentages()
    {
        var currentAnswers = GameplayManager.Get.rounds[(int)GameplayManager.Get.currentRound].currentQuestion.answers;
        int[] tally = new int[currentAnswers.Count];
        float[] percentages = new float[currentAnswers.Count];
        List<PlayerObject> activePlayers = PlayerManager.Get.players.Where(x => !x.eliminated).ToList();

        for (int i = 0; i < tally.Length; i++)
        {
            tally[i] = activePlayers.Count(x => x.submission == currentAnswers[i].answerText);
            percentages[i] = ((float)tally[i] / (float)activePlayers.Count) * 100f;
            RoundBase.answerPrefabs[i].mesh.text += $"\n<size=50%>{percentages[i].ToString("#0")}% ({tally[i]})";
        }
        DebugLog.Print($"{(100 - percentages.Sum()).ToString("#0")}% ({activePlayers.Count() - tally.Sum()} players) abstained...", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Yellow);
    }

    [Button]
    public void ResetTheMeter()
    {
        bar.ResetTheBar();
    }

    #endregion

    #region Floor Bars

    void ColoriseFloorBars()
    {
        for(int i = 0; i < floorBars.Length; i++)
        {
            if ((100 - currentBreakpointInt) / 10 <= i)
                floorBars[i].material = floorBarMats[2];
            else if(currentPercentageWrongInt / 10 > i)
                floorBars[i].material = floorBarMats[1];
            else
                floorBars[i].material = floorBarMats[0];
        }
    }

    #endregion
}
