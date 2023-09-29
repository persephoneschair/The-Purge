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

    [Header("Scene Objects")]
    public MeterLerper breakpoint;
    public MeterLerper bar;
    public TextMeshPro currentBreakpointMesh;
    public TextMeshPro currentPercentageWrongMesh;
    public Renderer[] floorBars;
    public Material[] floorBarMats;

    private void Update()
    {
        currentBreakpointInt = (int)currentBreakpoint;
        currentPercentageWrongInt = (int)currentPercentageWrong;

        ColoriseFloorBars();

        if (breakpoint.isMoving)
        {
            currentBreakpoint = breakpoint.gameObject.transform.localScale.y * 100f;
            currentBreakpointMesh.text = (100 - currentBreakpoint).ToString("#0") + "%";
        }

        if(bar.isMoving)
        {
            currentPercentageWrong = bar.gameObject.transform.localScale.y * 100f;
            currentPercentageWrongMesh.text = currentPercentageWrong == 0 ? "0%" : currentPercentageWrong.ToString("#0") + "%";
        }
    }

    private void Start()
    {
        breakpoint.ResetTheBar(currentBreakpoint);
        bar.ResetTheBar();
        currentBreakpointMesh.text = (100 - currentBreakpoint).ToString("#0") + "%";
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

    [Button]
    public void StartTheMeter()
    {
        StartTheMeter(setManualPercentageWrong);
    }

    public void StartTheMeter(float percentageWrong)
    {
        StartCoroutine(MeterRoutine(percentageWrong));
    }

    IEnumerator MeterRoutine(float percentageWrong)
    {
        percentageWrongResult = percentageWrong;
        ChevronManager.Get.SinglePulse(true);
        yield return new WaitForSeconds(1f);
        bar.MoveTheBar(percentageWrongResult > (100 - currentBreakpoint) ? currentBreakpoint : percentageWrongResult);
        yield return new WaitUntil(() => !bar.isMoving);
        currentPercentageWrongMesh.text = percentageWrongResult.ToString("#0") + "%";
        Debug.Log("Display result");
        //result
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
