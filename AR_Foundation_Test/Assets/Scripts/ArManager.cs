using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
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
    
    // array containing the objects to be place into the ar view
    [SerializeField] private GameObject[] objectsToPlace;
    
    // array of all the spawning positions for the objects to be placed
    private Vector3[] _spawnPosition;

    #endregion

    #region UI Elements

    // ui text corresponding to the current round
    [SerializeField] private Text roundText;
    
    #endregion

    private void Start() {
        // ar setup
        _arRaycastManager = gameObject.GetComponent<ARRaycastManager>();
        _arAnchorManager = gameObject.GetComponent<ARAnchorManager>();
        _arPlaneManager = gameObject.GetComponent<ARPlaneManager>();

        // scene setup
        _spawnPosition = new Vector3[totalRounds * 2];

        for (int i = 0; i < _spawnPosition.Length; i++) {
            _spawnPosition[i] = new Vector3(Random.Range(0, 3), Random.Range(0, 3), 1);
        }

        // deactivate the objects to place until the reference point has been set
        for (int i = 0; i < objectsToPlace.Length; i++) {
            objectsToPlace[i].SetActive(false);
        }
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
                var hitPose = hits[0].pose;
                _arAnchor = _arAnchorManager.AddAnchor(hitPose).gameObject;
                _isArAnchorSet = true;
                _arPlaneManager.detectionMode = PlaneDetectionMode.None;
                UpdateRound();
            }
        }
    }
    
    /// <summary>
    /// Method for the rounds and placement of spheres
    /// </summary>
    public void UpdateRound() {
        _round++;
        roundText.text = "ROUND " + _round + 1;
        for (int i = 0; i < objectsToPlace.Length; i++) {
            objectsToPlace[i].transform.position = _spawnPosition[_round + i * totalRounds];
        }
    }
}