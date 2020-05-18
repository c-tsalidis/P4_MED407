using System;
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

    [SerializeField] private AudioClip[] clips = new AudioClip[10];

    // array containing the objects to be placed into the ar view
    private GameObject[] objectsToPlace = new GameObject[2]; // two objects to be placed per round
    [SerializeField] private GameObject go_reverbHrtf;
    [SerializeField] private GameObject go_none;

    // parent / container of the instantiated spheres
    [SerializeField] private GameObject objectsSpawner;

    // is the scene set up?
    private bool _isSceneSetup = false;
    private bool isInMainMenuState = true;

    [SerializeField] private GameObject audioMeter;

    // array of all the spawning positions for the objects to be placed
    private Vector3[] _spawnPosition = new[] {
        new Vector3(3.0f, 1.0f, -1.0f),
        new Vector3(0.0f, 1.0f, 2.0f),
        new Vector3(-2.0f, 1.0f, 0.0f),
        new Vector3(2.0f, 1.0f, -3.0f),
        new Vector3(-2.0f, 1.0f, 1.0f),
        new Vector3(-2.0f, 1.0f, -3.0f),
        new Vector3(-2.0f, 1.0f, 2.0f),
        new Vector3(-1.0f, 1.0f, -0.0f),
        new Vector3(0.0f, 1.0f, 2.0f),
        new Vector3(2.0f, 1.0f, -3.0f),

        new Vector3(-1.0f, 1.0f, 1.0f),
        new Vector3(-3.0f, 1.0f, 2.0f),
        new Vector3(0.0f, 1.0f, -2.0f),
        new Vector3(0.0f, 1.0f, 1.0f),
        new Vector3(0.0f, 1.0f, -3.0f),
        new Vector3(2.0f, 1.0f, 1.0f),
        new Vector3(1.0f, 1.0f, 3.0f),
        new Vector3(-2.0f, 1.0f, 3.0f),
        new Vector3(3.0f, 1.0f, 1.0f),
        new Vector3(-1.0f, 1.0f, 1.0f),
    };

    // array containing all the colors of the game objects
    private Color[] _colors = new[] {
        Color.magenta, Color.blue, Color.blue, Color.magenta, Color.blue, Color.magenta, Color.magenta, Color.blue,
        Color.magenta, Color.magenta, 

        Color.blue, Color.magenta, Color.magenta, Color.blue, Color.magenta, Color.blue, Color.blue, Color.magenta,
        Color.blue, Color.blue, 
    };

    private int[] spawningOrders = new[] {0, 1, 1, 0, 0, 1, 0, 0, 1, 1,};

    // spawn delay time between the spawning of objects
    [SerializeField] private float delaySpawnTime = 3.0f;
    private bool _delayTimeHasPassed = true;

    // the output audio mixer for resonance audio --> Resonance audio mixer
    [SerializeField] private AudioMixer resonanceAudioMixer;

    #endregion

    #region UI Elements

    // ui text corresponding to the current round
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private GameObject testFinishedPanel;
    [SerializeField] private GameObject sceneSetupPanel;

    #endregion


    private void Start() {
        // ar setup
        _arRaycastManager = gameObject.GetComponent<ARRaycastManager>();
        _arAnchorManager = gameObject.GetComponent<ARAnchorManager>();
        _arPlaneManager = gameObject.GetComponent<ARPlaneManager>();
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


        if (!_isArAnchorSet) {
            if (_arRaycastManager.Raycast(_touchPos, hits, TrackableType.PlaneWithinPolygon)) {
                _isArAnchorSet = true;
                UpdateRound(true);
            }

            if (_arPlaneManager.trackables.count > 0) {
                _isArAnchorSet = true;
                UpdateRound(true);
            }
        }

    }

    public void StartArScene() => isInMainMenuState = false;
    

    private void SetUpObjectsToPlace() {
        if(isInMainMenuState) return;
        go_reverbHrtf = Instantiate(go_reverbHrtf, Vector3.up, Quaternion.identity);
        go_reverbHrtf.transform.SetParent(objectsSpawner.transform);
        go_reverbHrtf.SetActive(false);

        go_none = Instantiate(go_none, Vector3.up, Quaternion.identity);
        go_none.transform.SetParent(objectsSpawner.transform);
        go_none.SetActive(false);

        // stop rendering the planes, but keep them active on scene (make them invisible)
        _arPlaneManager.planePrefab.GetComponent<MeshRenderer>().enabled = false;
        _arPlaneManager.planePrefab.GetComponent<ARPlaneMeshVisualizer>().enabled = false;
        
        sceneSetupPanel.SetActive(false);

        _isSceneSetup = true;
    }

    /// <summary>
    /// Method for the rounds and placement of spheres
    /// </summary>
    public void UpdateRound(bool forward) {
        if(isInMainMenuState) return;
        if (!_isSceneSetup) SetUpObjectsToPlace();
        if (!_delayTimeHasPassed) return;
        if (forward) _round++;
        else if (_round > 0) _round--;

        // check if the test has finished. If so, inform the user
        if (_round > (totalRounds - 1)) {
            testFinishedPanel.SetActive(true);
            return;
        }

        Debug.Log("Updating round to round " + _round);
        roundText.text = "ROUND " + (_round + 1);

        // first deactivate the current placed objects
        foreach (var o in objectsToPlace)
            if (o != null)
                o.SetActive(false);

        // in here change characteristics of the audio according to the set of rounds this round corresponds to
        // first set --> sphere with nothing + sphere with reverb and hrtf

        // sphere has both reverb and hrtf
        objectsToPlace[0] = go_reverbHrtf;
        // sphere 1 has nothing
        objectsToPlace[1] = go_none;

        for (int i = 0; i < objectsToPlace.Length; i++) {
            // update the transform position of both objects
            objectsToPlace[i].transform.position = _spawnPosition[_round + i * totalRounds];
            // update the audio clip of both objects
            objectsToPlace[i].GetComponent<AudioSource>().clip = clips[_round];

            int loc = _round + i * totalRounds;
            objectsToPlace[i].GetComponent<Renderer>().sharedMaterial.color = _colors[loc];

            Debug.Log("Position of object " + objectsToPlace[i].name + " | Index: " + i + " | Round: " + _round +
                      " --> " +
                      objectsToPlace[i].transform.position + " | Color " +
                      objectsToPlace[i].GetComponent<Renderer>().sharedMaterial.color);
        }

        StartCoroutine(ObjectSpawnWithDelay(spawningOrders[_round]));
    }

    public void ResetRound() {
        if (!_delayTimeHasPassed) {
            UpdateRound(true);
            UpdateRound(false);
        }
    }

    private IEnumerator ObjectSpawnWithDelay(int value) {
        _delayTimeHasPassed = false;
        int first, second = 0;
        if (value == 0) {
            first = 0;
            second = 1;
        }
        else {
            first = 1;
            second = 0;
        }

        if (!objectsToPlace[first].activeSelf) objectsToPlace[first].SetActive(true);
        yield return new WaitForSeconds(delaySpawnTime);
        if (!objectsToPlace[second].activeSelf) objectsToPlace[second].SetActive(true);
        _delayTimeHasPassed = true;
    }

    public void ResetScene() {
        _round = 0;
        _isSceneSetup = false;

        audioMeter.GetComponent<AudioMeter>().ResetCountDown();

        // reset the objects to place and the instantiated spheres
        Destroy(go_none);
        for (int i = 0; i < objectsToPlace.Length; i++) {
            objectsToPlace[i] = null;
            Destroy(objectsSpawner.transform.GetChild(i).gameObject);
        }
    }

    public void SpherePlaySound(string color) {
        if (!_isSceneSetup) return;
        Color colorToCompare = new Color();
        if (color == "Blue") colorToCompare = Color.blue;
        else if (color == "Magenta") colorToCompare = Color.magenta;
        foreach (var o in objectsToPlace) {
            if (o.GetComponent<MeshRenderer>().sharedMaterial.color == colorToCompare) {
                o.GetComponent<AudioSource>().Play();
            }
        }
    }

    public void QuitApp() {
        Application.Quit();
    }
    
}