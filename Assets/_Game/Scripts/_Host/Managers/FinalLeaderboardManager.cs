using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FinalLeaderboardManager : SingletonMonoBehaviour<FinalLeaderboardManager>
{
    public Animator anim;
    public Transform instanceTarget;
    public GameObject strapToInstance;
    public List<FinalPositionStrap> instancedStraps;

    [Button]
    public void GeneratePreFinalLeaderboard()
    {
        ClearLeaderboard();
        foreach (PlayerObject po in PlayerManager.Get.players.OrderByDescending(x => x.purgesSurvived).ThenByDescending(x => x.mainGameCorrect).ThenBy(x => x.distanceFromTiebreak).ThenBy(x => x.playerName))
        {
            instancedStraps.Add(Instantiate(strapToInstance.GetComponent<FinalPositionStrap>(), instanceTarget));
            instancedStraps.LastOrDefault().Init(new string[3] { po.playerName, po.mainGameCorrect.ToString(), po.purgesSurvived.ToString() }, po.profileImage);
        }
        ToggleLeaderboard();
    }

    [Button]
    public void GenerateFinalLeaderboard()
    {
        ClearLeaderboard();
        foreach (PlayerObject po in PlayerManager.Get.players.OrderByDescending(x => x.purgesSurvived).ThenByDescending(x => x.totalCorrect).ThenBy(x => x.distanceFromTiebreak).ThenBy(x => x.playerName))
        {
            instancedStraps.Add(Instantiate(strapToInstance.GetComponent<FinalPositionStrap>(), instanceTarget));
            instancedStraps.LastOrDefault().Init(new string[3] { po.playerName, po.totalCorrect.ToString(), po.purgesSurvived.ToString() }, po.profileImage);
        }
        ToggleLeaderboard();
    }

    private void ClearLeaderboard()
    {
        foreach (FinalPositionStrap s in instancedStraps)
            Destroy(s.gameObject);
        instancedStraps.Clear();
    }

    [Button]
    public void ToggleLeaderboard()
    {
        AudioManager.Get.Play(AudioManager.OneShotClip.Whoosh);
        anim.SetTrigger("toggle");
    }
}
