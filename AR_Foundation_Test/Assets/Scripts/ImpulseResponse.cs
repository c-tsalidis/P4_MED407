using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ImpulseResponse : MonoBehaviour {
    private bool micFound = false;
    private AudioSource audioSource;

    private void Start() {
        // Ask for mic access from user before using it
        StartCoroutine(GetMicrophone());
    }

    private void StartRecording() {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = Microphone.Start(Microphone.devices[0], true, 10, AudioSettings.outputSampleRate);
        Debug.Log("Started recording");
        audioSource.loop = true;
        audioSource.Play();
    }

    private void StopRecording() {
        Debug.Log("Stop recording");
        Microphone.End(Microphone.devices[0]);
    }

    private IEnumerator GetMicrophone() {
        yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
        if (Application.HasUserAuthorization(UserAuthorization.Microphone)) {
            micFound = true;
            Debug.Log("Microphone found: ");
            foreach (var device in Microphone.devices) {
                Debug.Log("Name: " + device);
            }

            StartRecording();
        }
        else {
            Debug.Log("Microphone not found");
        }
    }

    private void Update() {
        if (audioSource != null) {
            /*
            float[] samples = new float[audioSource.clip.samples * audioSource.clip.channels];
            audioSource.clip.GetData(samples, 0);
            if(Math.Abs(audioSource.time - 1.0f) < 0.01)
            for (int i = 0; i < samples.Length; ++i) {
                Debug.Log(samples[i]);
                // samples[i] = samples[i] * 0.5f;
            }
            */

            if (audioSource.time >= 2.0f) {
                audioSource.Stop();
                StopRecording();
            }
        }
    }
}