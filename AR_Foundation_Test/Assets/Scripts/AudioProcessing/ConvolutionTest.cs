using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NWaves.Operations;
using NWaves.Operations.Convolution;
using NWaves.Signals;

using System.IO;
using System.Linq;


public class ConvolutionTest : MonoBehaviour
{
    public AudioClip IR;
    public AudioClip source;

    float[] kernel;
    float[] samples;
    float[] output;

    

    // Start is called before the first frame update
    void Start()
    {
         //source = GetComponent<AudioClip>();
        //IR = GetComponent<AudioClip>();

        samples = new float[source.samples];
        kernel = new float[IR.samples] ;
        // output = new float[samples.Length + kernel.Length - 1];
        AnalyzeSound();
        SaveOutputSignal(output);

    }

    // Update is called once per frame
    void Update()
    {
      
    }

    void AnalyzeSound()
    {
        source.GetData(samples, 0);
        IR.GetData(kernel, 0);
        Convolver convolve = new Convolver(Mathf.NextPowerOfTwo(samples.Length + kernel.Length - 1));
        output = new float[Mathf.NextPowerOfTwo(samples.Length + kernel.Length - 1)];
        convolve.Convolve(samples, kernel, output);
        //Debug.Log(convolve.);
        /*
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
        */
    }
    private void SaveOutputSignal(float[] outputSignal)
    {
        string filename = "outputSignal.txt";
        // save the output signal to a txt file
        File.WriteAllLines(Path.Combine(Application.streamingAssetsPath, filename),
            outputSignal.Select(d => d.ToString()));
    }
}
