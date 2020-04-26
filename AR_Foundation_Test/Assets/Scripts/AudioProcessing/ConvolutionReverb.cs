#define WRITEHEADER

using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEditor.Playables;

public class ConvolutionReverb : MonoBehaviour {
    [SerializeField] private AudioClip input;
    [SerializeField] private AudioClip impulseResponse;
    private AudioSource _audioSource;
    private AudioClip _audioClip;

    private void Start() {
        Debug.Log("Uploading impulse response " + impulseResponse.name);

        _audioSource = GetComponent<AudioSource>();

        // get data of impulse response
        float[] irData = new float[impulseResponse.samples];
        Util.DeinterleaveData(irData, 1);
        impulseResponse.GetData(irData, 0);

        // get data of the input signal
        float[] inputData = new float[input.samples];
        input.GetData(inputData, 0);
        Util.DeinterleaveData(inputData, 1);
        
        ConvolutionOverlapAdd(inputData, irData);
    }

    /// <summary>
    /// Convolution using the Overlap-Add procedure.
    /// </summary>
    /// <param name="inputData">Input signal data of the audio you want to apply the convolution reverberation to.</param>
    /// <param name="irData">The impulse response data that you want to convolve the input signal with.</param>
    private void ConvolutionOverlapAdd(float[] inputData, float[] irData) {

        
        /*
        h = FIR_impulse_response
        M = length(h)
        Nx = length(x)
        N = 8 × M    (see next section for a better choice)
        step_size = N - (M-1)
        H = DFT(h, N)
        position = 0
        y(1 : Nx + M-1) = 0

        while position + step_size ≤ Nx do
            y(position+(1:N)) = y(position+(1:N)) + IDFT(DFT(x(position+(1:step_size)), N) × H)
            position = position + step_size
        end
        
         */

        int Nx = inputData.Length;
        int M = irData.Length;
        // the segment size has to be bigger than or equal to the impulse response length. So:
        int stepSize = M;
        // get length that arrays will be zero-padded to --> the fft size --> F = N + M - 1
        int F = stepSize + M - 1;
        if (!Mathf.IsPowerOfTwo(F)) {
            // add zero padding to it
            F = Mathf.NextPowerOfTwo(F);
        }
        
        int position = 0;
        
        // output signal
        float[] outputSignal = new float[F];
        
        // create temporary arrays of complex numbers with zero padding to the impulse response if needed
        Complex[] newIRData = new Complex[F];
        irData = ZeroPadding(irData, F);
        for (int i = 0; i < F; i++) {
            newIRData[i].Re = irData[i];
        }
        // FFT of the the impulse response to transform it into the frequency domain
        FourierTransform.FFT(newIRData, FourierTransform.Direction.Forward);

        // zero padding of the input data
        Complex[] newInputData = new Complex[F];
        inputData = ZeroPadding(inputData, F);
        for (int i = 0; i < inputData.Length; i++) {
            newInputData[i].Re = inputData[i];
        }
        
        // divide the input data into multiple segments
        int numberSegments = Nx / stepSize;
        Complex [][] inputDatas = new Complex[numberSegments][];
        for (int i = 0; i < numberSegments; i += stepSize) {
            inputDatas[i] = new Complex[stepSize];
            for (int j = 0; j < stepSize; j++) {
                int loc = j + i * stepSize;
                inputDatas[i][j].Re = newInputData[loc].Re;
            }
        }
        
        // calculate the fourier transform of every segment of the input signal
        for (int i = 1; i < numberSegments; i++) {
            // FFT of the input signal and of the impulse response to transform them into the frequency domain
            FourierTransform.FFT(inputDatas[i], FourierTransform.Direction.Forward);
        }
        
        // convolution in the time domain --> multiplication in the frequency domain
        Complex[] inverseFFT = new Complex[F];
        for (int i = 0; i < numberSegments; i += stepSize) {
            // inputDatas[i] = new Complex[stepSize];
            for (int j = 0; j < stepSize; j++) {
                int loc = j + i * stepSize;
                // inverseFFT[i] = inputDatas[i][j] * newIRData[i] * F;
                inverseFFT[loc] = inputDatas[i][j] * newIRData[loc];
            }
        }

        FourierTransform.FFT(inverseFFT, FourierTransform.Direction.Backward);

        for (int i = 0; i < Nx; i++) {
            outputSignal[i] += outputSignal[i] + (float) inverseFFT[i].Re; // overlap-add
        }

        float[] resultSignal = new float[outputSignal.Length];
        // Util.InterleaveData(outputSignal, resultSignal);
        
        // now play the output signal
        // set the _audioClip data to the outputSignal
        // set  _audioSource.clip = _audioClip;
        SaveOutputSignal(outputSignal);
        // SaveOutputSignal(resultSignal);
    }

    private void SaveOutputSignal(float[] outputSignal) { 
        
        string filename = "outputSignal.txt";
        // save the output signal to a txt file
        File.WriteAllLines(Path.Combine(Application.streamingAssetsPath, filename) ,outputSignal.Select(d => d.ToString()));

        filename = "record.wav";
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

        _audioClip = input;
        _audioClip.SetData(outputSignal, 0);
        _audioSource.clip = _audioClip;
        
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