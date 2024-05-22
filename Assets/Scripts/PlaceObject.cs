using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

public class PlaceObject : MonoBehaviour
{
    [SerializeField]
    private GameObject Prefab;
    private ARRaycastManager ARRaycastManager;
    private ARPlaneManager ARPlaneManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private bool objectPlaced = false; // Flag to track if object is already placed

    [SerializeField]
    private float distanceFromCamera = 10.0f; // Adjust this value to change the distance

    private void Start()
    {
        ARRaycastManager = GetComponent<ARRaycastManager>();
        ARPlaneManager = GetComponent<ARPlaneManager>();
    }

    private void OnEnable()
    {
        EnhancedTouch.TouchSimulation.Enable();
        EnhancedTouch.EnhancedTouchSupport.Enable();
        EnhancedTouch.Touch.onFingerDown += FingerDown;
    }

    private void OnDisable()
    {
        EnhancedTouch.TouchSimulation.Disable();
        EnhancedTouch.EnhancedTouchSupport.Disable();
        EnhancedTouch.Touch.onFingerDown -= FingerDown;
    }

    private void FingerDown(EnhancedTouch.Finger finger)
    {
        if (objectPlaced) return; // If object is already placed, return

        if (finger.index > 0)
        {
            return;
        }

        if (ARRaycastManager.Raycast(finger.currentTouch.screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose pose = hits[0].pose; // Get the first hit pose
            Vector3 newPosition = pose.position + pose.forward * distanceFromCamera; // Adjust the position
            Instantiate(Prefab, newPosition, pose.rotation);

            // Disable plane detection
            ARPlaneManager.enabled = false;

            objectPlaced = true; // Set the flag to true
        }
    }
}
