using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AForge.Math;
using NWaves.Operations.Convolution;
using UnityEngine;

public class ConvolutionReverbTest : MonoBehaviour {
    [SerializeField] private AudioClip input;
    [SerializeField] private AudioClip impulseResponse;

    // ir
    private float[] irData;
    private Complex[] irc;
    
    // input
    private float[] inputData;
    private Complex[] inputc;
    
    // conv. reverb values
    private int fftSize = 0; // F = N + M - 1 --> Also to the power of 2
    
    // muliplication in freq domain
    private Complex[] multiplication;
    
    // output signal
    private float[] output;

    private void Start() {
        GetAudioData();
        Convolve();
    }

    private void Convolve() {
        // calculate the fft of input data
        FourierTransform.FFT(inputc, FourierTransform.Direction.Forward);
        // calculate fft of impulse response
        FourierTransform.FFT(irc, FourierTransform.Direction.Forward);
        
        // multiply them in frequency domain
        multiplication = new Complex[fftSize];
        for (int i = 0; i < fftSize; i++) {
            multiplication[i] =  inputc[i] * irc[i];
            multiplication[i] *= fftSize; // normalize it
        }
        
        // calculate the ifft of the multiplication
        FourierTransform.FFT(multiplication, FourierTransform.Direction.Backward);
        
        // save output signal as wave file
        output = new float[fftSize];
        for (int i = 0; i < fftSize; i++) {
            output[i] = (float) multiplication[i].Re;
        }
        
        SaveOutputSignal(output);
    }

    private Complex ComplexMultiply(Complex a, Complex b) {
        return new Complex(a.Re * b.Re - a.Im * b.Im, a.Re * b.Im + a.Im * b.Re);
    }

    private void CalculateFftSize() {
        fftSize = Mathf.NextPowerOfTwo(inputData.Length + irData.Length - 1);
    }

    private void GetAudioData() {

        // get data of ir signal
        irData = new float[impulseResponse.samples];
        impulseResponse.GetData(irData, 0);
        // get data of input signal
        inputData = new float[input.samples];
        input.GetData(inputData, 0);
        
        CalculateFftSize();
        
        
        // change values accordingly of both signals --> zero padding + complex numbers
        irData = ZeroPadding(irData);
        irc = new Complex[irData.Length];
        for (int i = 0; i < irData.Length; i++) {
            irc[i].Re = irData[i];
        }
        
        inputData = ZeroPadding(inputData);
        inputc = new Complex[inputData.Length];
        for (int i = 0; i < inputData.Length; i++) {
            inputc[i].Re = inputData[i];
        }
    }
    
    private float[] ZeroPadding(float[] data) {
        List<float> newData = new List<float>(data);
        int N = data.Length;
        for (int i = 0; i < (fftSize - N); i++) {
            newData.Add(0);
        }
        return newData.ToArray();
    }
    
    private void SaveOutputSignal(float[] outputSignal) {
        string filename = "outputSignal.txt";
        // save the output signal to a txt file
        File.WriteAllLines(Path.Combine(Application.streamingAssetsPath, filename),
            outputSignal.Select(d => d.ToString()));
        
        
        new WaveFile().SaveAudio(Path.Combine(Application.streamingAssetsPath, "output.wav"), outputSignal.Length, 1, outputSignal, input.frequency);
    }
}