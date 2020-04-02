using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class MicrophoneFeed : MonoBehaviour {
    public bool useMicrophone = false;

    private AudioSource source;
    private string device;
    private bool prevUseMicrophone = false;
    private AudioClip prevClip = null;

    void Update() {
        if (useMicrophone != prevUseMicrophone) {
            prevUseMicrophone = useMicrophone;
            if (useMicrophone) {
                foreach (string m in Microphone.devices) {
                    device = m;
                    break;
                }

                source = GetComponent<AudioSource>();
                prevClip = source.clip;
                source.Stop();
                Debug.Log("using mic");
                source.clip = Microphone.Start(device, true, 1, AudioSettings.outputSampleRate);
                source.Play();
                Debug.Log("Playing " + source.clip.name);

                int dspBufferSize, dspNumBuffers;
                AudioSettings.GetDSPBufferSize(out dspBufferSize, out dspNumBuffers);

                source.timeSamples =
                    (Microphone.GetPosition(device) + AudioSettings.outputSampleRate -
                     3 * dspBufferSize * dspNumBuffers) % AudioSettings.outputSampleRate;
            }
            else {
                Microphone.End(device);
                source.clip = prevClip;
                source.Play();
            }
        }

        float[] spectrum = new float[256];
        source.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);
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

    void OnGUI() {
        if (GUILayout.Button(useMicrophone ? "Disable microphone" : "Enable microphone"))
            useMicrophone = !useMicrophone;
    }
}