using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CreditsManager : SingletonMonoBehaviour<CreditsManager>
{

    public GameObject endCard;
    public string[] authorNames;
    public TextMeshProUGUI creditsMesh;
    public Animator creditsAnim;
    public Animator backdrop;

    public Animator[] chevrons;

    private void Start()
    {
        this.gameObject.SetActive(false);
    }

    [Button]
    public void RollCredits()
    {
        AudioManager.Get.StopLoop();
        AudioManager.Get.Play(AudioManager.OneShotClip.Credits);
        string st = creditsMesh.text;
        creditsMesh.text = st.Replace("[AUTHOR1]", authorNames[0]).Replace("[AUTHOR2]", authorNames[1]).Replace("[AUTHOR3]", authorNames[2]).Replace("[AUTHOR4]", authorNames[3]);
        this.gameObject.SetActive(true);
        StartCoroutine(Credits());
        StartCoroutine(RunChevrons());
    }

    IEnumerator RunChevrons()
    {
        yield return new WaitForSeconds(6.75f);
        for(int i = 0; i < 6; i++)
        {
            int rand = UnityEngine.Random.Range(0, 3);

            if (rand == 2)
                foreach (Animator a in chevrons)
                    a.SetTrigger("toggle");
            else
                chevrons[rand].SetTrigger("toggle");

            yield return new WaitForSeconds(6.55f);
        }
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < 2; i++)
        {
            int rand = UnityEngine.Random.Range(0, 3);

            if (rand == 2)
                foreach (Animator a in chevrons)
                    a.SetTrigger("toggle");
            else
                chevrons[rand].SetTrigger("toggle");

            yield return new WaitForSeconds(6.55f);
        }
    }

    IEnumerator Credits()
    {
        backdrop.SetTrigger("toggle");
        yield return new WaitForSeconds(1f);
        creditsAnim.SetTrigger("toggle");
        yield return new WaitForSeconds(57.5f);
        creditsAnim.gameObject.SetActive(false);
        endCard.SetActive(true);
    }
}
