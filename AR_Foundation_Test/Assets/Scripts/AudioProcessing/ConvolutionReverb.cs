using System;
using System.Collections;
using UnityEngine;

public class ConvolutionReverb: MonoBehaviour {

    [SerializeField] private AudioClip input;
    [SerializeField] private AudioClip impulseResponse;
    
    const float MAXLENGTH = 15.0f;
    const int MAXSAMPLE = 16;

    int globalupdatecount = 0;
    
    private void Start() {
        
        Debug.Log("Uploading impulse response " + impulseResponse.name);
        float[] data = new float[impulseResponse.samples];
        impulseResponse.GetData(data, 0);
        Convolve(data, data.Length / input.channels, input.channels, input.frequency, input.name);
    }

    public void Convolve(float[] data, int numsamples, int numchannels, int samplerate, string name) {

        Debug.Log(data.Length);
        Complex [] complexData = new Complex[data.Length];
        
        // to make the data to the power of two --> Check out Zero Padding
        // Augment the input signal with F-N number of zeros (page 102) --> F is the desired signal length (2^number) --> 2^8 = 256
        if (!Mathf.IsPowerOfTwo(data.Length)) {
            ArrayList newData = new ArrayList(data);
            int F = 256; // desired signal length
            int N = data.Length;
            for (int i = 0; i < (F - N); i++) {
                newData.Add(0);
            }
        }
        
        // convert data values to complex numbers
        for(int i = 0; i < data.Length; i++) {
            complexData[i] = (Complex) data[i];
        }

        /*
        Debug.Log(Mathf.IsPowerOfTwo(data.Length));
        int segments = (int) data.Length / Mathf.Pow(2, 10);
        for (int i = 0; i < segments; i++) {
            
        }
        */
        
        // Apply FFT of input to convert it to frequency domain
        FourierTransform.FFT(complexData, FourierTransform.Direction.Forward);
        
        // apply FFT of impulse response to get the frequency response
        
        // apply FIR filter in frequency domain --> multiplication of input in frequency domain with frequency response of filter
        
        // IFFT (Inverse Fast Fourier Transform) of the multiplication of input and frequency response

    }
}