using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DashSliderController : MonoBehaviour
{
    private Slider dashSlider;

    private bool isDashed;
    private float dashTime;

    private void Start()
    {
        dashSlider = GetComponent<Slider>();
    }

    private void Update()
    {
        if (isDashed)
        {
            StartCoroutine(DashTimer());
            isDashed = false;
        }
    }

    public void UpdateSlider(Component sender, object data)
    {
        isDashed = true;
        dashTime = Convert.ToSingle(data); // Use ToSingle instead of ToInt16 for float data
    }

    IEnumerator DashTimer()
    {
        float currentTime = dashTime;

        while (currentTime > 0)
        {
            currentTime -= Time.deltaTime;

            float normalizedTime = currentTime / dashTime;

            if (dashSlider != null)
            {
                dashSlider.value = normalizedTime;
            }

            yield return null;
        }
        
        // Reset the slider value when the timer is complete
        if (dashSlider != null)
        {
            dashSlider.value = dashTime;
        }
    }
}

