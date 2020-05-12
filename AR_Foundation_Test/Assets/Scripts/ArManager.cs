﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Random = UnityEngine.Random;

[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARAnchorManager))]
[RequireComponent(typeof(ARPlaneManager))]
public class ArManager : MonoBehaviour {
    #region AR variables

    // ar managers scripts
    private ARRaycastManager _arRaycastManager;
    private ARAnchorManager _arAnchorManager;
    private ARPlaneManager _arPlaneManager;

    // list containing all the ray cast hits where the user is touching in the screen
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    // reference point used for instantiating everything according to its position
    private GameObject _arAnchor;

    // touch position where the user touches the screen
    private Vector2 _touchPos;

    // has the reference point (ar anchor) been set?
    private bool _isArAnchorSet;

    #endregion

    #region Scene setup and functionality variables

    // rounds variables
    private int _round = -1; // index for the currently playing round
    private int _previousRound;
    [SerializeField] private int totalRounds = 10; // total amount of rounds

    // array containing the objects to be placed into the ar view
    private GameObject[] objectsToPlace = new GameObject[2]; // two objects to be placed per round
    [SerializeField] private GameObject go_reverbHrtf;
    [SerializeField] private GameObject go_reverb;
    [SerializeField] private GameObject go_hrtf;
    [SerializeField] private GameObject go_none;

    // is the scene set up?
    private bool _isSceneSetup = false;
    
    // array containing all the colors of the gameobjects
    private Color[] _colors;

    // array of all the spawning positions for the objects to be placed
    private Vector3[] _spawnPosition;

    // the output audio mixer for resonance audio --> Resonance audio mixer
    [SerializeField] private AudioMixer resonanceAudioMixer;

    #endregion

    #region UI Elements

    // ui text corresponding to the current round
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private GameObject testFinishedPanel;

    #endregion


    private string arrayOfCoordinates;
    private string arrayOfColors;

    private void Start() {
        // ar setup
        _arRaycastManager = gameObject.GetComponent<ARRaycastManager>();
        _arAnchorManager = gameObject.GetComponent<ARAnchorManager>();
        _arPlaneManager = gameObject.GetComponent<ARPlaneManager>();

        // scene setup
        _spawnPosition = new Vector3[totalRounds * 3 * objectsToPlace.Length]; // three sets of rounds
        _colors = new Color[totalRounds * 3 * objectsToPlace.Length]; // three sets of rounds
    }


    private void Update() {
        // Play sound on hit
        if (Input.touchCount > 0) {
            var touch = Input.GetTouch(0);
            _touchPos = touch.position;
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(_touchPos);
            if (Physics.Raycast(ray, out hit, 1000)) {
                if (hit.transform.CompareTag("Sphere")) {
                    hit.transform.GetComponent<AudioSource>().Play();
                }
            }
        }
    }

    private void SetUpObjectsToPlace() {
        for (int i = 0; i < _spawnPosition.Length; i++) {
            _spawnPosition[i] = new Vector3(Random.Range(0, 3), 0.5f, Random.Range(0, 3));
            arrayOfCoordinates += "new Vector3" + _spawnPosition[i] + ",  "; // to get a string of all the random values to predefine the random values (for evaluating the testing of the prototype)

            int random = (int) Random.Range(0, 2);
            Color[] c = new[] { Color.blue, Color.magenta };
            _colors[i] = c[random];
            string[] cs = new[] {"Color.blue", "Color.magenta"};
            arrayOfColors += cs[random] + ",  ";
        }

        Debug.Log(arrayOfCoordinates);
        Debug.Log(arrayOfColors);

        go_reverbHrtf = Instantiate(go_reverbHrtf, Vector3.up, Quaternion.identity);
        go_reverbHrtf.SetActive(false);
        
        go_hrtf = Instantiate(go_hrtf, Vector3.up, Quaternion.identity);
        go_hrtf.SetActive(false);
        
        go_none = Instantiate(go_none, Vector3.up, Quaternion.identity);
        go_none.SetActive(false);
        
        go_reverb = Instantiate(go_reverb, Vector3.up, Quaternion.identity);
        go_reverb.SetActive(false);

        _isSceneSetup = true;
    }

    /// <summary>
    /// Method for the rounds and placement of spheres
    /// </summary>
    public void UpdateRound(bool forward) {
        if(!_isSceneSetup) SetUpObjectsToPlace();
        if (forward) _round++;
        else _round--;
        
        // check if the test has finished. If so, inform the user
        if (_round > (totalRounds * 3 - 1)) {
            testFinishedPanel.SetActive(true);
            return;
        }
        
        Debug.Log("Updating round to round " + _round);
        roundText.text = "ROUND " + (_round + 1);

        // first deactivate the current placed objects
        foreach (var o in objectsToPlace) if(o != null) o.SetActive(false);
        
        // in here change characteristics of the audio according to the set of rounds this round corresponds to
        // first set --> sphere with nothing + sphere with reverb and hrtf
        if (_round % 3 == 0) {
            // sphere has both reverb and hrtf
            objectsToPlace[0] = go_reverbHrtf;
            // sphere 1 has nothing
            objectsToPlace[1] = go_none;
        }
        // second set --> both spheres with hrtf, one sphere with reverb
        else if (_round % 3 == 1) {
            objectsToPlace[0] = go_reverbHrtf;
            objectsToPlace[1] = go_hrtf;
        }
        // third set --> both spheres with reverb, one with hrtf
        else if (_round % 3 == 2) {
            objectsToPlace[0] = go_reverbHrtf;
            objectsToPlace[1] = go_reverb;
        }

        for (int i = 0; i < objectsToPlace.Length; i++) {
            if(!objectsToPlace[i].activeSelf) objectsToPlace[i].SetActive(true);
            objectsToPlace[i].transform.position = _spawnPosition[_round + i * totalRounds];
            // objectsToPlace[i].GetComponent<AudioSource>().Play();

            int loc = _round + i * totalRounds * 3;
            objectsToPlace[i].GetComponent<Renderer>().sharedMaterial.color = _colors[loc];
            Debug.Log("Position of object " + objectsToPlace[i].name + "at round " + _round + ": " +
                      objectsToPlace[i].transform.position + " | With color " +
                      objectsToPlace[i].GetComponent<Renderer>().sharedMaterial.color);
        }
    }
}