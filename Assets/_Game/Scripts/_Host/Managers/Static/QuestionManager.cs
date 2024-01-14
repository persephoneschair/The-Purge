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
        return GameplayManager.Get.nextMainQuestionIndex >= currentPack.mainGame.Count ? true : false;
    }

    public static bool EndOfPurgeQs()
    {
        return GameplayManager.Get.nextPurgeQuestionIndex >= currentPack.purgeGame.Count ? true : false;
    }


    public static Pack ConvertPackFromLegacyTemplate(TextAsset legacyQs, string authorName)
    {
        QuestionPack qp = null;
        qp = JsonConvert.DeserializeObject<QuestionPack>(legacyQs.text);

        //Author
        Pack pack = new Pack()
        {
            author = authorName
        };

        //Tiebreaker
        Question tbQ = new Question()
        {
            questionText = qp.tiebreakerQuestion[0].question
        };
        tbQ.answers.Add(new Answer(qp.tiebreakerQuestion[0].answer, true));
        pack.tiebreaker = tbQ;

        //Main Game
        foreach(StandardRound sQ in qp.standardQuestions)
        {
            Question q = new Question()
            {
                questionText = sQ.question
            };
            q.answers.Add(new Answer(sQ.correctAnswer, true));
            foreach (string s in sQ.incorrectAnswers)
                q.answers.Add(new Answer(s, false));

            pack.mainGame.Add(q);
        }

        //Purge Game
        foreach(PurgeQuestion pQ in qp.purgeQuestions)
        {
            Question q = new Question()
            {
                questionText = pQ.question
            };
            q.answers.Add(new Answer(string.Join(",", pQ.answer), true));
            
            pack.purgeGame.Add(q);
        }

        //Final Game
        foreach(FinalQuestion fQ in qp.finalQuestions)
        {
            Question q = new Question()
            {
                questionText = string.Join("|", fQ.questionPart1, fQ.questionPart2)
            };
            q.answers.Add(new Answer(string.Join(",", fQ.answer), true));

            pack.finalGame.Add(q);
        }

        Operator.Get.exportedLegacyPack = JsonConvert.SerializeObject(pack, Formatting.Indented);
        currentPack = pack;
        return pack;
    }
}
