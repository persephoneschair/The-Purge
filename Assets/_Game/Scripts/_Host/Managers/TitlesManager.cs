using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TitlesManager : SingletonMonoBehaviour<TitlesManager>
{
    public float typingDelay = 0.2f;
    public float hitDelay = 1.5f;

    public Animator redChevrons;
    public Animator greenChevrons;
    public Animator textAnim;
    public Animator finalTitleAnim;
    public TextMeshProUGUI textMesh;
    public TextMeshProUGUI finalMesh;

    [TextArea(3, 4)] public string[] titles;

    [Button]
    public void RunTitleSequence()
    {
        if (Operator.Get.skipOpeningTitles)
            EndOfTitleSequence();
        else
        {
            GameplayManager.Get.currentStage = GameplayManager.GameplayStage.DoNothing;
            StartCoroutine(TitleSequence());
        }           
    }

    IEnumerator TitleSequence()
    {
        AudioManager.Get.Play(AudioManager.OneShotClip.Titles);
        yield return new WaitForSeconds(3f);
        for(int i = 0; i < titles.Length; i++)
        {
            textMesh.text = titles[i];
            if(i % 2 == 0)
                greenChevrons.SetTrigger("toggle");
            if (i > 0)
                redChevrons.SetTrigger("toggle");

            textAnim.SetTrigger("toggle");
            yield return new WaitForSeconds(6.4f);
        }

        string p = "THE";
        for(int i = 0; i < 3; i++)
        {
            finalMesh.text += p[i].ToString();
            yield return new WaitForSeconds(typingDelay);
        }
        finalMesh.text = "THE\nPURGE";
        yield return new WaitForSeconds(0.85f);
        string[] colors = new string[3] {"<color=green>", "<color=red>", "<color=white>" };
        for (int i = 0; i < 3; i++)
        {
            finalMesh.text = colors[i] + "THE\nPURGE";
            yield return new WaitForSeconds(hitDelay);
        }
        finalTitleAnim.SetTrigger("toggle");
        yield return new WaitForSeconds(1.25f);
        finalMesh.text = "<font=HollowPurge>THE\nPURGE";

        yield return new WaitForSeconds(3f);
        EndOfTitleSequence();
    }

    void EndOfTitleSequence()
    {
        GameplayManager.Get.currentStage = GameplayManager.GameplayStage.OpenLobby;
        GameplayManager.Get.ProgressGameplay();
        this.gameObject.SetActive(false);
    }
}
