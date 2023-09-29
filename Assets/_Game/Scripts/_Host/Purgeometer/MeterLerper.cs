using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeterLerper : MonoBehaviour
{
    [Range(0f, 100f)] public float percentageEndpoint;
    private float lerpDuration;
    public bool smoothEnds;

    float elapsedTime;
    Vector3 startPos;
    Vector3 endPos;
    public bool isMoving;

    public void MoveTheBar(float target)
    {
        percentageEndpoint = target;
        MoveToPos(false);
    }

    public void ResetTheBar(float overrideReset = 0f)
    {
        percentageEndpoint = overrideReset;
        MoveToPos(true);
    }

    #region Public Calls

    public void MoveToPos(bool fast)
    {
        if (isMoving)
            return;

        startPos = this.transform.localScale;

        endPos = new Vector3(this.transform.localScale.x, percentageEndpoint / 100f, this.transform.localScale.z);

        float difference = Mathf.Abs(this.transform.localScale.y - (percentageEndpoint / 100f));
        lerpDuration = difference * (fast ? 4f : 25f);

        elapsedTime = 0;
        isMoving = true;
        StartCoroutine(EndLock());
    }

    public string GetFormattedPercentage()
    {
        return (this.transform.localScale.y * 100).ToString("0.00");
    }

    #endregion

    #region Internal Methods

    private void Update()
    {
        if (isMoving)
            PerformLerp();
    }

    private void PerformLerp()
    {
        elapsedTime += Time.deltaTime;
        float percentageComplete = elapsedTime / lerpDuration;

        if (smoothEnds)
            this.gameObject.transform.localScale = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0, 1, percentageComplete));
        else
            this.gameObject.transform.localScale = Vector3.Lerp(startPos, endPos, percentageComplete);
    }

    private IEnumerator EndLock()
    {
        yield return new WaitForSeconds(lerpDuration + 0.1f);
        isMoving = false;
    }

    #endregion
}