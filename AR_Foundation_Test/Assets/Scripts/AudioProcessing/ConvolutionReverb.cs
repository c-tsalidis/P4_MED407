#define WRITEHEADER

using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using AForge.Math;
using NWaves.Operations.Convolution;
using NWaves.Signals;
using UnityEngine.Experimental.PlayerLoop;

public class ConvolutionReverb : MonoBehaviour {
    [SerializeField] private AudioClip input;
    [SerializeField] private AudioClip impulseResponse;
    private AudioSource _audioSource;
    private AudioClip _audioClip;

    private void Start() {
        Debug.Log("Uploading impulse response " + impulseResponse.name);

        // get data of impulse response --> maybe instead of  using this prerecorded impulse response, we could create our own
        float[] irData = new float[impulseResponse.samples];
        impulseResponse.GetData(irData, 0);

        // get data of the input signal
        float[] inputData = new float[input.samples];
        input.GetData(inputData, 0);

        _audioSource = GetComponent<AudioSource>();

        ConvolutionOverlapAdd(inputData, irData);

        /*
        int outputF = Mathf.NextPowerOfTwo(inputData.Length + irData.Length - 1);
        DiscreteSignal inputSignal = new DiscreteSignal(input.frequency, inputData);
        DiscreteSignal impulseResponseSignal = new DiscreteSignal(impulseResponse.frequency, irData);
        var conv = new Convolver(outputF).Convolve(inputSignal, impulseResponseSignal);
        SaveOutputSignal(conv.Samples);
        
        new WaveFile().SaveAudio(Path.Combine(Application.streamingAssetsPath, "waveFile.wav"), conv.Samples.Length, 1, conv.Samples, conv.SamplingRate);


        _audioClip = AudioClip.Create("Convolution Reverb", conv.Samples.Length, 1, conv.SamplingRate, false);
        _audioClip.SetData(conv.Samples, 0);
        _audioSource.clip = _audioClip;
        */
    }

    /// <summary>
    /// Convolution using the Overlap-Add procedure.
    /// </summary>
    /// <param name="inputData">Input signal data of the audio you want to apply the convolution reverberation to.</param>
    /// <param name="irData">The impulse response data that you want to convolve the input signal with.</param>
    private void ConvolutionOverlapAdd(float[] inputData, float[] irData) {
        int Nx = inputData.Length;
        int M = irData.Length;
        // the segment size has to be bigger than or equal to the impulse response length. So:
        int stepSize = M + 1;
        // get length that arrays will be zero-padded to --> the fft size --> F = N + M - 1
        int F = Mathf.NextPowerOfTwo(stepSize + M - 1);
        int outputF = Mathf.NextPowerOfTwo(Nx + M - 1);

        // output signal
        Complex[] outputSignal = new Complex[F];

        // create temporary arrays of complex numbers with zero padding to the impulse response if needed
        Complex[] newIRData = new Complex[F];
        irData = ZeroPadding(irData, F);
        for (int i = 0; i < F; i++) {
            newIRData[i].Re = irData[i];
        }
        // should the impulse response with zero padding be of size of each segment? Because I'm multiplying each block times the impulse response in freq. domain
        // I also think I need to make the zero padding of each segment after they've been split

        // FFT of the the impulse response to transform it into the frequency domain
        FourierTransform.FFT(newIRData, FourierTransform.Direction.Forward);

        // zero padding of the input data
        Complex[] newInputData = new Complex[F];
        // inputData = ZeroPadding(inputData, F);
        for (int i = 0; i < inputData.Length; i++) {
            newInputData[i].Re = inputData[i];
        }

        // divide the input data into multiple segments
        int numberSegments = (int) Math.Ceiling((float) Nx / stepSize);
        Complex[][] inputDatas = new Complex[numberSegments][];
        for (int i = 0; i < numberSegments; i += 1) {
            inputDatas[i] = new Complex[stepSize];
            for (int j = 0; j < stepSize; j++) {
                int loc = j + i * stepSize;
                inputDatas[i][j] = newInputData[loc];
            }
        }

        // calculate the fourier transform of every segment of the input signal
        for (int i = 0; i < numberSegments; i++) {
            // add zero padding to each segment of the input signal
            inputDatas[i] = ZeroPadding(inputDatas[i], F);
            // FFT of the input signal and of the impulse response to transform them into the frequency domain
            FourierTransform.FFT(inputDatas[i], FourierTransform.Direction.Forward);
        }

        // convolution in the time domain --> multiplication in the frequency domain
        // multiply each x_k times frequency response --> this gives us y_k
        Complex[][] outputDatas = new Complex[numberSegments][];
        for (int i = 0; i < numberSegments; i++) {
            outputDatas[i] = new Complex[outputF];
            for (int j = 0; j < stepSize; j++) {
                int loc = j + i * stepSize;
                // x_k * ir
                outputDatas[i][loc] = inputDatas[i][j] * newIRData[loc];
            }
        }

        // calculate the IFFT of each y_k
        for (int i = 0; i < numberSegments; i++) {
            FourierTransform.FFT(outputDatas[i], FourierTransform.Direction.Backward);
        }

        // put all y_k into a single signal y with overlap and add
        // shift each y(k) by numberOfSegments * n samples and add the results together
        // overlap and add --> final convolution looks like this --> y(k) = y_0(k) + y_1(k - N) + y_2(k - 2N) + ...
        Complex[] outputDataComplex = new Complex[outputF];
        for (int i = 0; i < numberSegments; i++) {
            for (int j = 0; j < stepSize; j++) {
                int loc = j + i * stepSize;
                outputDataComplex[loc] += outputDatas[i][j];
            }
        }
        float [] outputData = new float[outputDataComplex.Length];
        for (int i = 0; i < outputData.Length; i++) {
            outputData[i] = (float) outputDataComplex[i].Re;
        }
        SaveOutputSignal(outputData);
        
        // to check if the fft and ifft calculations are correct --> the ifft(fft(inputData)) should give me the original inputData
        /*
        float [] x = {1, 2, 3, 4, 0, 2, 1, 2, 3, 4, 5, 1, 4, 2, 4, 2};
        Complex [] xc = new Complex[x.Length];
        for (int i = 0; i < x.Length; i++) {
            xc[i].Re = x[i];
        }
        FourierTransform.FFT(newInputData, FourierTransform.Direction.Forward);
        FourierTransform.FFT(newInputData, FourierTransform.Direction.Backward);
        */
        
    }

    private void SaveOutputSignal(float[] outputSignal) {
        string filename = "outputSignal.txt";
        // save the output signal to a txt file
        File.WriteAllLines(Path.Combine(Application.streamingAssetsPath, filename),
            outputSignal.Select(d => d.ToString()));
        
        using (var stream = new FileStream(Path.Combine(Application.streamingAssetsPath, "output.wav"), FileMode.OpenOrCreate, FileAccess.ReadWrite)) {
            stream.AppendWaveData(outputSignal);
        }
    }

    private float[] ZeroPadding(float[] data, int fftSize) {
        List<float> newData = new List<float>(data);
        int N = data.Length;
        for (int i = 0; i < (fftSize - N); i++) {
            newData.Add(0);
        }

        return newData.ToArray();
    }

    private Complex[] ZeroPadding(Complex[] data, int fftSize) {
        List<Complex> newData = new List<Complex>(data);
        int N = data.Length;
        for (int i = 0; i < (fftSize - N); i++) {
            newData.Add(new Complex(0, 0));
        }

        return newData.ToArray();
    }
}