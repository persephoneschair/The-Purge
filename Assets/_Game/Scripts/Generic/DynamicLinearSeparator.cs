using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicLinearSeparator : MonoBehaviour
{
    public Vector3 Separation = Vector3.zero;
    public bool Centered = false;

    public void PerformSpread()
    {
        int childCount = transform.childCount;
        Vector3 localRoot = Vector3.zero;
        if (Centered)
        {
            localRoot = Separation * (childCount - 1) * -0.5f;
        }

        for (int childIndex = 0; childIndex < childCount; ++childIndex)
        {
            transform.GetChild(childIndex).localPosition = localRoot;
            localRoot += Separation;
        }
    }
}
