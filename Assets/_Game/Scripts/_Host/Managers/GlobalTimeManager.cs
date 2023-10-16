using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GlobalTimeManager : SingletonMonoBehaviour<GlobalTimeManager>
{
    private bool questionClockRunning;
    [ShowOnly] public float elapsedTime;
    public float tickRate = 0.25f;
    private float elapsedTicker;

    public TextMeshProUGUI timerMesh;
    public Animator timerAnim;

    public float defaultTiebreakTime;
    public float defaultMainGameTime;
    public float defaultPurgeTime;
    public float defaultFinalTime;

    private void Update()
    {
        if (questionClockRunning)
            QuestionTimer();
        else
        {
            elapsedTime = 0;
            elapsedTicker = 0;
            timerMesh.text = "<color=red>00.00";
        }
    }

    [Button]
    public void StartClock()
    {
        if (questionClockRunning)
            return;

        timerAnim.SetTrigger("toggle");
        questionClockRunning = true;
    }

    private void HideClock()
    {
        timerAnim.SetTrigger("toggle");
    }

    void QuestionTimer()
    {
        elapsedTime += (1f * Time.deltaTime);
        elapsedTicker += (1f * Time.deltaTime);
        if(elapsedTicker > tickRate)
        {
            AudioManager.Get.Play(AudioManager.OneShotClip.ClockTick);
            elapsedTicker = 0;
        }

        switch(GameplayManager.Get.currentRound)
        {
            case GameplayManager.Round.Tiebreaker:
                timerMesh.text = GetColoredTimer(defaultTiebreakTime) + (defaultTiebreakTime - elapsedTime <= 0.01f ? "00.00" : (defaultTiebreakTime - elapsedTime).ToString("00.00"));
                if (elapsedTime >= defaultTiebreakTime)
                    EndTimer();
                break;

            case GameplayManager.Round.MainRound:
                timerMesh.text = GetColoredTimer(defaultMainGameTime) + (defaultMainGameTime - elapsedTime <= 0.01f ? "00.00" : (defaultMainGameTime - elapsedTime).ToString("00.00"));
                if (elapsedTime >= defaultMainGameTime)
                    EndTimer();
                break;

            case GameplayManager.Round.PurgeRound:
                timerMesh.text = GetColoredTimer(defaultPurgeTime) + (defaultPurgeTime - elapsedTime <= 0.01f ? "00.00" : (defaultPurgeTime - elapsedTime).ToString("00.00"));
                if (elapsedTime >= defaultPurgeTime)
                    EndTimer();
                break;

            case GameplayManager.Round.FinalRound:
                timerMesh.text = GetColoredTimer(defaultFinalTime) + (defaultFinalTime - elapsedTime <= 0.01f ? "00.00" : (defaultFinalTime - elapsedTime).ToString("00.00"));
                if (elapsedTime >= defaultFinalTime)
                    EndTimer();
                break;

            default:
                timerMesh.text = GetColoredTimer(10) + (10 - elapsedTime).ToString("00.00");
                if (elapsedTime >= 10)
                    EndTimer();
                break;
        }
    }

    void EndTimer()
    {
        questionClockRunning = false;
        Invoke("HideClock", 2f);
    }

    public float GetRawTimestamp()
    {
        return elapsedTime;
    }

    public string GetFormattedTimestamp()
    {
        return elapsedTime.ToString("#0.00");
    }

    public string GetColoredTimer(float time)
    {
        if (time - elapsedTime > 5)
            return "<color=green>";
        else if(time - elapsedTime < 5)
        {
            if ((time - elapsedTime) % 1 > 0.5f)
                return "<color=yellow>";
            else
                return "<color=red>";
        }
        return "";
    }
}
