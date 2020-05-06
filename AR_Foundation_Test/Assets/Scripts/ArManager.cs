using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;



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

    private void Start()
    {
        _arRaycastManager = gameObject.GetComponent<ARRaycastManager>();

        _spawnedGo = new GameObject[2];

    }



    private void Update()
    {
        // Debug.Log("Time Since Loaded : " + Time.timeSinceLevelLoad);
     
        if (!CheckTouchPosition(out _touchPos)) {
            return;
        }
        if (Input.touchCount > 0) {
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out hit, 1000))
                {
                    if (hit.transform.CompareTag("TagTry"))
                    {
                        gameObject.GetComponent<AudioSource>().Play();
                    }
                }
            }
        }
        if (!CheckTouchPosition(out _touchPos))
        {
            return;
        }
        

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

    public void playSound()
    {
        audioSource.clip = audioClip;
        audioSource.Play();
    }

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