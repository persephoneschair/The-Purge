using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObjectSerializable
{
    public string playerClientID;
    public string playerName;

    public string twitchName;

    public bool eliminated;

    public int points;
    public int totalCorrect;

    public int mainGameCorrect;
    public int distanceFromTiebreak;
    public int purgesSurvived;

    public string submission;
    public float submissionTime;
    public bool flagForCondone;
    public bool wasCorrect;
    public bool inPurge;
    public bool inFinal;
}
