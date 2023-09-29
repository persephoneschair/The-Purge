using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class ChevronWall : MonoBehaviour
{
    public float singleSpeed = 0.1f;
    public float multiSpeed = 0.1f;
    public int multiIterations = 12;
    public ChevronRow[] rows;

    #region Public Functions

    public void SetStatic(bool down)
    {
        KillAllChevrons();
        if (down)
            foreach (ChevronRow row in rows)
                row.LightRowDown();
        else
            foreach(ChevronRow row in rows)
                row.LightRowUp();
    }

    public void SinglePulse(bool down)
    {
        if (down)
            SingleDown();
        else
            SingleUp();
    }

    public void MultiPulse(bool down)
    {
        if (down)
            MultiDown();
        else
            MultiUp();
    }

    [Button]
    public void KillAllChevrons()
    {
        foreach (ChevronRow row in rows)
            row.SwitchRowOff();
    }

    #endregion

    #region Static Lights

    [Button]
    private void StaticGreen()
    {
        SetStatic(false);
    }

    [Button]
    private void StaticRed()
    {
        SetStatic(true);
    }

    #endregion

    #region Single Pulse

    [Button]
    private void SingleDown()
    {
        StartCoroutine(SingleDownRoutine());        
    }
    IEnumerator SingleDownRoutine()
    {
        ChevronRow[] rev = rows.Reverse().ToArray();
        foreach (ChevronRow row in rev)
        {
            KillAllChevrons();
            row.LightRowDown();
            yield return new WaitForSeconds(singleSpeed);
        }
        KillAllChevrons();
    }

    [Button]
    private void SingleUp()
    {
        StartCoroutine(SingleUpRoutine());
    }

    IEnumerator SingleUpRoutine()
    {
        foreach (ChevronRow row in rows)
        {
            KillAllChevrons();
            row.LightRowUp();
            yield return new WaitForSeconds(singleSpeed);
        }
        KillAllChevrons();
    }

    #endregion

    #region Multi Pulse

    [Button]
    private void MultiDown()
    {
        StartCoroutine(MultiRoutine(true));
    }

    [Button]
    private void MultiUp()
    {
        StartCoroutine(MultiRoutine(false));
    }

    IEnumerator MultiRoutine(bool down)
    {
        ChevronRow[] x = down ? rows.Reverse().ToArray() : rows.ToArray();

        for(int i = 0; i < x.Length; i++)
        {
            KillAllChevrons();
            if (down)
                x[i].LightRowDown();
            else
                x[i].LightRowUp();
            if (i > 1)
                for (int j = i; j >= 0; j -= 2)
                    if (down)
                        x[j].LightRowDown();
                    else
                        x[j].LightRowUp();
            yield return new WaitForSeconds(multiSpeed);
        }

        for(int i = 0; i < multiIterations; i++)
        {
            KillAllChevrons();
            for (int j = 1; j < x.Length; j += 2)
                if (down)
                    x[j].LightRowDown();
                else
                    x[j].LightRowUp();
            yield return new WaitForSeconds(multiSpeed);
            KillAllChevrons();
            for (int j = 0; j < x.Length; j += 2)
                if (down)
                    x[j].LightRowDown();
                else
                    x[j].LightRowUp();
            yield return new WaitForSeconds(multiSpeed);
        }

        for (int i = 1; i < x.Length; i++)
        {
            KillAllChevrons();
            if (down)
                x[i].LightRowDown();
            else
                x[i].LightRowUp();
            for (int j = i; j < x.Length; j += 2)
                if (down)
                    x[j].LightRowDown();
                else
                    x[j].LightRowUp();
            yield return new WaitForSeconds(multiSpeed);
        }
        KillAllChevrons();
        yield return new WaitForSeconds(multiSpeed);
        if (down)
            StaticRed();
        else
            StaticGreen();
    }

    #endregion
}
