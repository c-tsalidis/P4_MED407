using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util {
    public static void ShiftArray<T>(T[] array, int N) {
        T[] temp = new T[array.Length];
        System.Array.Copy(array, temp, array.Length);
        for (int i = 0; i < array.Length; ++i)
            array[(i + N) % array.Length] = temp[i];
    }

    public static float[] BlackmanWindow(int N) {
        int M = (N % 2 == 0) ? N / 2 : (N + 1) / 2;

        float[] win = new float[N];
        win[0] = win[N - 1] = 0f;
        for (int i = 1; i < M; ++i)
            win[i] = win[N - 1 - i] = 0.42f - 0.5f * Mathf.Cos(2 * Mathf.PI * i / (N - 1)) +
                                      0.08f * Mathf.Cos(4 * Mathf.PI * i / (N - 1));

        return win;
    }

    public static T[][] DeinterleaveData<T>(T[] data, int num_ch) {
        T[][] deinterleaved = new T[num_ch][];
        int channel_length = data.Length / num_ch;
        for (int ch = 0; ch < num_ch; ++ch) {
            deinterleaved[ch] = new T[channel_length];
            for (int i = 0; i < channel_length; ++i)
                deinterleaved[ch][i] = data[i * num_ch];
        }

        return deinterleaved;
    }

    public static void InterleaveData<T>(T[][] data_in, T[] data_out) {
        int num_ch = data_in.Length;
        for (int i = 0; i < data_in[0].Length; ++i) {
            for (int ch = 0; ch < num_ch; ++ch)
                data_out[i * num_ch + ch] = data_in[ch][i];
        }
    }
    
    public static void InterleaveData<T>(T[] data_in, T[] data_out) {
        int num_ch = data_in.Length;
        for (int i = 0; i < data_in.Length; ++i) {
            for (int ch = 0; ch < num_ch; ++ch)
                data_out[i * num_ch + ch] = data_in[i];
        }
    }
}