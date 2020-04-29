﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioMeter : MonoBehaviour
{
    private const int SAMPLE_SIZE = 1024;

    //average power output of the sound
    public float rmsValue;
    public float dbValue;
    public float pitchValue;

    public float maxVisualScale = 25;
    public float visualModifier = 50.0f;
    public float smoothSpeed = 10.0f;
    public float keepPercentage = 0.5f;

    private AudioSource source;
    private float[] samples;
    private float[] spectrum;
    private float sampleRate;

    private Transform[] visualList;
    private float[] visualScale;
    private int amnVisual = 40;
    
    private void Start()
    {
        source = GetComponent<AudioSource>();
        samples = new float[SAMPLE_SIZE];
        spectrum = new float[SAMPLE_SIZE];
        sampleRate = AudioSettings.outputSampleRate;

        SpawnLine();
    }
    private void SpawnLine()
    {
        visualScale = new float[amnVisual];
        visualList = new Transform[amnVisual];

        for (int i = 0; i < amnVisual; i++)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube) as GameObject;
            //visualList[i].localScale = Vector3(1, 1, 0.1);
            visualList[i] = go.transform;
            visualList[i].position = Vector3.right * i;
            
        }
    }
    private void Update()
    {
        AnalyzeSound();
        UpdateVisual();
    }
    private void UpdateVisual() 
    {
        int visualIndex = 0;
        int spectrumIndex = 0;
        int averageSize = (int)((SAMPLE_SIZE * keepPercentage) / amnVisual);

        while (visualIndex < amnVisual)
        {
            int j = 0;
            float sum = 0;

            while (j < averageSize)
            {
                sum += spectrum[spectrumIndex];
                spectrumIndex++;
                j++;
            }
            float scaleY = sum / averageSize * visualModifier;
            visualScale[visualIndex] -= Time.deltaTime * smoothSpeed;

            if (visualScale[visualIndex] < scaleY)
                visualScale[visualIndex] = scaleY;

            if (visualScale[visualIndex] >  maxVisualScale)
            visualScale[visualIndex] = maxVisualScale;
            

            visualList[visualIndex].localScale = Vector3.one + Vector3.up * visualScale[visualIndex];
            visualIndex++;
        }
    }
    private void AnalyzeSound()
    {
        source.GetOutputData(samples, 0);

        //get rms values
        int i = 0;
        float sum = 0;
        for(; i < SAMPLE_SIZE; i++)
        {
            sum += samples[i] * samples[i];
        }
        rmsValue = Mathf.Sqrt(sum / SAMPLE_SIZE);

        //Getting the DB value
        dbValue = 20 * Mathf.Log10(rmsValue / 0.1f);

        //Get sound spectrum
        source.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);

        //find pitch
        float maxV = 0;
        var maxN = 0;

        for (i = 0; i < SAMPLE_SIZE; i++)
        { // find max 
            if (!(spectrum[i] > maxV) || !(spectrum[i] > 0.0f))
                continue;

            maxV = spectrum[i];
            maxN = i; // maxN is the index of max
            
        }
        float freqN = maxN; 

        if (maxN > 0 && maxN < SAMPLE_SIZE - 1)
        { 
            // interpolate index using neighbours
            var dL = spectrum[maxN - 1] / spectrum[maxN];
            var dR = spectrum[maxN + 1] / spectrum[maxN];
            freqN += 0.5f * (dR * dR - dL * dL);
        }
        pitchValue = freqN * (sampleRate / 2) / SAMPLE_SIZE; // convert index to frequency
    }
}

