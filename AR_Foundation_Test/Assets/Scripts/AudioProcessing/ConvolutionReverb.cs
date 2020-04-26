using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ConvolutionReverb : MonoBehaviour {
    [SerializeField] private AudioClip input;
    [SerializeField] private AudioClip impulseResponse;
    private AudioSource _audioSource;
    private AudioClip _audioClip;

    private void Start() {
        Debug.Log("Uploading impulse response " + impulseResponse.name);

        // get data of impulse response
        float[] irData = new float[impulseResponse.samples];
        impulseResponse.GetData(irData, 0);

        // get data of the input signal
        float[] inputData = new float[input.samples];
        input.GetData(inputData, 0);

        ConvolutionOverlapAdd(inputData, irData);
    }

    /// <summary>
    /// Convolution using the Overlap-Add procedure.
    /// </summary>
    /// <param name="inputData">Input signal data of the audio you want to apply the convolution reverberation to.</param>
    /// <param name="irData">The impulse response data that you want to convolve the input signal with.</param>
    private void ConvolutionOverlapAdd(float[] inputData, float[] irData) {
        int N = inputData.Length;
        int M = irData.Length;
        // get length that arrays will be zero-padded to --> the fft size --> F = N + M - 1
        int F = Mathf.NextPowerOfTwo(N + M - 1);

        // create temporary arrays of complex numbers with zero padding to F if needed
        Complex[] newIRData = new Complex[F];
        irData = ZeroPadding(irData, F);
        for (int i = 0; i < F; i++) {
            newIRData[i].Re = irData[i];
        }
        Complex[] newInputData = new Complex[F];
        inputData = ZeroPadding(inputData, F);
        for (int i = 0; i < inputData.Length; i++) {
            newInputData[i].Re = inputData[i];
        }

        // FFT of the input signal and of the impulse response to transform them into the frequency domain
        FourierTransform.FFT(newInputData, FourierTransform.Direction.Forward);
        FourierTransform.FFT(newIRData, FourierTransform.Direction.Forward);

        // convolution in the time domain --> multiplication in the frequency domain
        Complex[] inverseFFT = new Complex[F];
        for (int i = 0; i < inverseFFT.Length; i++) {
            // ifft[i] = newInputData[i] * newIRData[i] * F;
            inverseFFT[i] = newInputData[i] * newIRData[i];
        }

        FourierTransform.FFT(inverseFFT, FourierTransform.Direction.Backward);

        // output signal
        float[] outputSignal = new float[inputData.Length];

        for (int i = 0; i < N; i++) {
            outputSignal[i] += outputSignal[i] + (float) inverseFFT[i].Re; // overlap-add
        }
        
        // now play the output signal
        // set the _audioClip data to the outputSignal
        // set  _audioSource.clip = _audioClip;
        SaveOutputSignal(outputSignal);
    }

    private void SaveOutputSignal(float[] outputSignal) { 
        string filename = "record.wav";
        BinaryWriter binwriter;
        #if WRITEHEADER
            filename = Path.Combine(Application.streamingAssetsPath, filename);
            var stream = new FileStream(filename, FileMode.Create);
            binwriter = new BinaryWriter(stream);
            for (int n = 0; n < 44; n++)
                binwriter.Write((byte)0);
        #else
            var stream = new FileStream("record.raw", FileMode.Create);
            binwriter = new BinaryWriter(stream);
        #endif
        for (int n = 0; n < outputSignal.Length; n++)
            binwriter.Write(outputSignal[n]);
    }

    private float[] ZeroPadding(float[] data, int fftSize) {
        List<float> newData = new List<float>(data);
        int N = data.Length;
        for (int i = 0; i < (fftSize - N); i++) {
            newData.Add(0);
        }

        return newData.ToArray();
    }
}