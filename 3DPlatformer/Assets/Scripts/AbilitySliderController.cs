using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AbilitySliderController : MonoBehaviour
{
    private Slider slider;

    private bool abilityStarted;
    private float abilityTime;

    private void Start()
    {
        slider = GetComponent<Slider>();
    }

    private void Update()
    {
        if (abilityStarted)
        {
            StartCoroutine(DashTimer());
            abilityStarted = false;
        }
    }

    public void UpdateSlider(Component sender, object data)
    {
        abilityStarted = true;
        abilityTime = Convert.ToSingle(data);
    }

    IEnumerator DashTimer()
    {
        float currentTime = abilityTime;

        while (currentTime > 0)
        {
            currentTime -= Time.deltaTime;

            float normalizedTime = currentTime / abilityTime;

            if (slider != null)
            {
                slider.value = normalizedTime;
            }

            yield return null;
        }
        
        if (slider != null)
        {
            slider.value = abilityTime;
        }
    }
}

