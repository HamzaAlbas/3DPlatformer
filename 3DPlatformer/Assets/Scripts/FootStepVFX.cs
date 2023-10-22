using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootStepVFX : MonoBehaviour
{
    public GameObject[] rightFootSteps;
    public GameObject[] leftFootSteps;


    private int rightIndex;
    private int leftIndex;

    public void RightFootStep()
    {
        if (rightIndex >= rightFootSteps.Length) rightIndex = 0;

        rightFootSteps[rightIndex].SetActive(true);

        rightIndex++;
    }

    public void LeftFootStep()
    {
        if (leftIndex >= leftFootSteps.Length) leftIndex = 0;

        leftFootSteps[leftIndex].SetActive(true);

        leftIndex++;
    }
}
