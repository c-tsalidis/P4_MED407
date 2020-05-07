using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.XR.ARFoundation;


public class Touched : MonoBehaviour
{
    
    [SerializeField] private TouchedObject[] touchedObjects;
    
    [SerializeField]
    private Color activeColor = Color.red;
    
    [SerializeField]
    private Color inactiveColor = Color.gray;
    
    private Vector2 touchPosition = default;
    
    
    // Start is called before the first frame update
    void Start()
    {
        

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            touchPosition = touch.position;
            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    TouchedObject touchedObject = hit.transform.GetComponent<TouchedObject>();
                    if (touchedObject != null)
                    {
                        ActionOnTouch(touchedObject);
                        Debug.Log("touched");
                    }
                }
            }
        }
        
    }

    void ActionOnTouch(TouchedObject selected)
    {
        foreach (TouchedObject current in touchedObjects)
        {
            MeshRenderer meshRenderer = current.GetComponent<MeshRenderer>();
            if (selected != current)
            {
                current.IsTouched = false;
                meshRenderer.material.color = inactiveColor;
                
            }
            else
            {
                current.IsTouched = true;
                meshRenderer.material.color = activeColor;
            }
        }
    }
}
