using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayDataSerializable
{
    public int nextMainGameQuestionIndex;
    public int nextPurgeQuestionIndex;
    public int nextFinalQuestionIndex;
    public GameplayManager.GameplayStage currentStage;
    public GameplayManager.Round currentRound;

    public float currentBreakpoint;
    public float currentStartingBreakpoint;

    public int roundsPlayed;
}
