using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ImpulseResponse : MonoBehaviour {
    private bool micFound = false;
    private bool stoppedRecording = false;
    private AudioSource audioSource;

    private void Start() {
        audioSource = GetComponent<AudioSource>();
        // Ask for mic access from user before using it
        // StartCoroutine(GetMicrophone());
        audioSource.clip = Microphone.Start(null, true, 1, AudioSettings.outputSampleRate);
        Debug.Log("Started recording");
        while (!(Microphone.GetPosition(null) > 0)) {}
        Debug.Log("start playing... position is " + Microphone.GetPosition(null));
        // audioSource.loop = true;
        audioSource.Play();
    }

    private void StartRecording() {
        audioSource.clip = Microphone.Start(Microphone.devices[0], true, 5, AudioSettings.outputSampleRate);
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
        /*
        if (Time.time > 6.0f) {
            if (!stoppedRecording) {
                StopRecording();
                stoppedRecording = true;
            }
        }
        */
        float[] spectrum = new float[256];
        AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);
        for (int i = 1; i < spectrum.Length - 1; i++) {
            Debug.DrawLine(new Vector3(i - 1, spectrum[i] + 10, 0), new Vector3(i, spectrum[i + 1] + 10, 0), Color.red);
            Debug.DrawLine(new Vector3(i - 1, Mathf.Log(spectrum[i - 1]) + 10, 2),
                new Vector3(i, Mathf.Log(spectrum[i]) + 10, 2), Color.cyan);
            Debug.DrawLine(new Vector3(Mathf.Log(i - 1), spectrum[i - 1] - 10, 1),
                new Vector3(Mathf.Log(i), spectrum[i] - 10, 1), Color.green);
            Debug.DrawLine(new Vector3(Mathf.Log(i - 1), Mathf.Log(spectrum[i - 1]), 3),
                new Vector3(Mathf.Log(i), Mathf.Log(spectrum[i]), 3), Color.blue);
        }
    }
}