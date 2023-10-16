using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.Linq;

public static class QuestionManager
{
    public static Pack currentPack = null;

    public static void DecompilePack(TextAsset tx)
    {
        currentPack = JsonConvert.DeserializeObject<Pack>(tx.text);
        foreach (Question q in currentPack.mainGame)
            q.answers.Shuffle();

        foreach(Question q in currentPack.purgeGame)
        {
            string[] splitAns = q.answers.FirstOrDefault().answerText.Split(',');
            q.answers.Clear();
            foreach (string s in splitAns)
                q.answers.Add(new Answer(s, true));
        }
        foreach(Question q in currentPack.finalGame)
        {
            string[] splitAns = q.answers.FirstOrDefault().answerText.Split(',');
            q.answers.Clear();
            foreach (string s in splitAns)
                q.answers.Add(new Answer(s, true));
        }
    }

    public static int GetRoundQCount()
    {
        switch (GameplayManager.Get.currentRound)
        {
            default:
                return 0;
        }
    }

    /*public static Question GetQuestion(int qNum)
    {
        switch (GameplayManager.Get.currentRound)
        {
            default:
                return null;
        }
    }*/

    public static bool FinalPlayerCountReached()
    {
        return PlayerManager.Get.players.Count(x => !x.eliminated) <= currentPack.finalGame.Count + 1 ? true : false;
    }

    public static bool EndOfMainGameQs()
    {
        return GameplayManager.nextMainQuestionIndex >= currentPack.mainGame.Count ? true : false;
    }

    public static bool EndOfPurgeQs()
    {
        return GameplayManager.nextPurgeQuestionIndex >= currentPack.purgeGame.Count ? true : false;
    }
}
