using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NWaves.Operations;
using NWaves.Operations.Convolution;
using NWaves.Signals;

public class ConvolutionTest : MonoBehaviour
{
    public AudioSource IR;
    public AudioSource source;

    float[] kernel;
    float[] samples;
    float[] output;

    Convolver convolve = new Convolver(512);

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
        IR = GetComponent<AudioSource>();

        samples = new float[1024];
        kernel = new float[1024];
        output = new float[samples.Length + kernel.Length - 1];

        
    }

    // Update is called once per frame
    void Update()
    {
        AnalyzeSound();
    }

    void AnalyzeSound()
    {
        source.GetOutputData(samples, 0);
        IR.GetOutputData(kernel, 0);

        convolve.Convolve(samples, kernel, output);

        for (int i = 0; i < output.Length; i++)
        {
            System.Diagnostics.Debug.WriteLine(output[i]);
        }
        for (int i = 0; i < samples.Length; i++)
        {
            System.Diagnostics.Debug.WriteLine(samples[i]);
        }
        for (int i = 0; i < kernel.Length; i++)
        {
            System.Diagnostics.Debug.WriteLine(kernel[i]);
        }
    }
}
