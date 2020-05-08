using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;


[RequireComponent(typeof(ARRaycastManager))]
public class ArManager : MonoBehaviour
{
    [SerializeField] private GameObject[] goToPlace;
    private GameObject[] _spawnedGo;
    private ARRaycastManager _arRaycastManager;
    private Vector2 _touchPos;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    public AudioSource audioSource;
    public AudioClip audioClip;

    public Text roundText;
    /*
     * We have N rounds --> N = 10
     * We would then need to store the corridnates of each sphere 10 times in a vector3 array
     * Vector3 spheres = new Vector3 [N times amount of spheres] 
     * Populate the cordinates arrays with random positions
     * math.random
     *
     * int round = 0
     * Then each round
     * When the user clicks on "NEXT"... {round++.. updateRound() }
     *                                 updateround() would  _spawnedGo[i].transform.position = spheres[round + i*N]  .... roundTexttext = round + 1;
     */
    
    private void Start()
    {
        _arRaycastManager = gameObject.GetComponent<ARRaycastManager>();

        _spawnedGo = new GameObject[2];
        
    }


    private void Update()
    {
        // Debug.Log("Time Since Loaded : " + Time.timeSinceLevelLoad);

        if (!CheckTouchPosition(out _touchPos))
        {
            return;
        }

        // Play sound on hit
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out hit, 1000))
                {
                    if (hit.transform.CompareTag("Sphere"))
                    {
                        hit.transform.GetComponent<AudioSource>().Play();
                    }
                }
            }
        }


        //Spawning of spheres
        if (_arRaycastManager.Raycast(_touchPos, hits, TrackableType.PlaneWithinPolygon))
        {
            var hitPose = hits[0].pose;

            for (int i = 0; i < goToPlace.Length; i++)
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeNullComparison
                if (_spawnedGo[i] == null)
                {
                    _spawnedGo[i] = Instantiate(goToPlace[i], hitPose.position, hitPose.rotation);
                }
                else
                {
                    var xNew = i * 5;
                    _spawnedGo[i].transform.position = hitPose.position + new Vector3(xNew, 0, 0);
                }
            }
        }
    }


    // Checks where screen is touched
    private bool CheckTouchPosition(out Vector2 _touchPos)
    {
        if (Input.touchCount > 0)
        {
            _touchPos = Input.GetTouch(0).position;
            return true;
        }

        _touchPos = default;
        return false;
    }
}