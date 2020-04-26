using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Object = System.Object;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

[RequireComponent(typeof(ARRaycastManager))]
public class ArManager : MonoBehaviour {
    [SerializeField] private GameObject[] goToPlace;
    private GameObject[] _spawnedGo;
    private ARRaycastManager _arRaycastManager;
    private Vector2 _touchPos;
    
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    public Button myButton;

    private void Start() {
        _arRaycastManager = gameObject.GetComponent<ARRaycastManager>();
        _spawnedGo = new GameObject[2]; //initialising two gameobjects

        Button nextSceneButton = myButton.GetComponent<Button>(); //initialising button
        nextSceneButton.onClick.AddListener(goToNextScene); //adding goToNextScene function when it is pressed

    }
    
    private void Update() {
        if (!CheckTouchPosition(out Vector2 _touchPos)) {
            return;
        }

        if (_arRaycastManager.Raycast(_touchPos, hits, TrackableType.PlaneWithinPolygon)) {
            var hitPose = hits[0].pose;
            
            for (int i = 0; i < goToPlace.Length; i++) {
                if (_spawnedGo[i] == null)
                {
                    _spawnedGo[i] = Instantiate(goToPlace[i], hitPose.position, hitPose.rotation);
                }
                else
                {
                    var x_new = i * 5;
                    _spawnedGo[i].transform.position = hitPose.position + new Vector3(x_new, 0, 0);
                }
            }
        }
    } 

    private Vector3 placeSpheres()
    {
        // these positions are just default ones, for testing purposes
        var xPos = 120; 
        var yPos = 300;
        var height = 100;

        var xPosNew = 100;
        var yPosNew = 100;
        
        //TODO: create an array with a length of 10 with vector3 coordinates for the next position of the spheres
        
        for (int i = 0; i < _spawnedGo.Length; i++)
        {
            if (_spawnedGo[i] == null)
            {
                return _spawnedGo[i].transform.position = new Vector3(xPos, yPos, height);
            }
            else
            {
                return _spawnedGo[i].transform.position = _spawnedGo[i-1].transform.position + new Vector3(xPosNew, yPosNew, 0);
            }
        }
        return default;
    }

    public void goToNextScene()
    {
        placeSpheres(); //calls placeSphere function when proceeding to the next scene
        
        //TODO: add signifier gameobjects (10 squares) + functionality to set the next one true (coloured) and the rest false (uncoloured) 
        
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