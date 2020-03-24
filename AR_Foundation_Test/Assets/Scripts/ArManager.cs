using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
public class ArManager : MonoBehaviour {
    [SerializeField] private GameObject goToPlace;
    private GameObject _spawnedGo;
    private ARRaycastManager _arRaycastManager;
    private Vector2 _touchPos;
    
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private void Start() {
        _arRaycastManager = gameObject.GetComponent<ARRaycastManager>();
        
    }

    private void Update() {
        if (!CheckTouchPosition(out Vector2 _touchPos)) {
            return;
        }

        if (_arRaycastManager.Raycast(_touchPos, hits, TrackableType.PlaneWithinPolygon)) {
            var hitPose = hits[0].pose;
            if (_spawnedGo == null) {
                _spawnedGo = Instantiate(goToPlace, hitPose.position, hitPose.rotation);
            }
            else {
                _spawnedGo.transform.position = hitPose.position;
            }
        }
    }

    private bool CheckTouchPosition(out Vector2 _touchPos) {
        if (Input.touchCount > 0) {
            _touchPos = Input.GetTouch(0).position;
            return true;
        }

        _touchPos = default;
        return false;
    }
}