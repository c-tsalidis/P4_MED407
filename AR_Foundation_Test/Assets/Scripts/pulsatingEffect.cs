using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pulsatingEffect : MonoBehaviour
{
    private bool coroutineStarted;
    
    // Start is called before the first frame update
    void Start()
    {
        coroutineStarted = true;

    }

    // Update is called once per frame
    void Update()
    {
        if (coroutineStarted)
        {
            StartCoroutine(nameof(pulsatingSize));
        }
    }

    private IEnumerator pulsatingSize()
    {
        coroutineStarted = false;
        float changeSize = 0.15f;

        for (float i = 0f; i <= 1f; i+= 0.05f)
        {
            transform.localScale = new Vector3(
                Mathf.Lerp(transform.localScale.x, transform.localScale.x + changeSize, Mathf.SmoothStep(0f, 1f, i)),
                Mathf.Lerp(transform.localScale.y, transform.localScale.y + changeSize, Mathf.SmoothStep(0f, 1f, i)),
                Mathf.Lerp(transform.localScale.z, transform.localScale.z + changeSize, Mathf.SmoothStep(0f, 1f, i))
                );
            yield return new WaitForSeconds(0.015f);
        }
        
        for (float i = 0f; i <= 1f; i+= 0.1f)
        {
            transform.localScale = new Vector3(
                Mathf.Lerp(transform.localScale.x, transform.localScale.x - changeSize, Mathf.SmoothStep(0f, 1f, i)),
                Mathf.Lerp(transform.localScale.y, transform.localScale.y - changeSize, Mathf.SmoothStep(0f, 1f, i)),
                Mathf.Lerp(transform.localScale.z, transform.localScale.z - changeSize, Mathf.SmoothStep(0f, 1f, i))
                );
            yield return new WaitForSeconds(0.015f);
        }

        coroutineStarted = true;
    }
}
