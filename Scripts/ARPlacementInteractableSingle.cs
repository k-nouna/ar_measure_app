using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.AR;
using UnityEngine.EventSystems;
public class ARPlacementInteractableSingle : ARBaseGestureInteractable
{
    [SerializeField]
    [Tooltip("A GameObject to place when a raycast from a user touch hits a plane.")]
    public GameObject placementPrefab;

    [SerializeField]
    [Tooltip("Callback event executed after object is placed.")]
    private ARObjectPlacementEvent onObjectPlaced;

    private GameObject placementObject;
    
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private static GameObject trackablesObject;

    bool IsPointOverUIObject(Vector2 pos)
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return false;

        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(pos.x, pos.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        return results.Count > 0;

    }

    protected override bool CanStartManipulationForGesture(TapGesture gesture)
    {
    
        if (IsPointOverUIObject(gesture.startPosition))
            return false;

        // Allow for test planes
        if (gesture.targetObject == null || gesture.targetObject.layer == 9)
            return true;


        return false;
    }
    public void DestroyPlacementObject()
    {
        Destroy(placementObject);
    }

    protected override void OnEndManipulation(TapGesture gesture)
    {
        if(Input.touchCount == 0)
        {
            return;
        }

        var touch = Input.GetTouch(0);
        var touchPosition = touch.position;
        bool isOverUI = IsPointOverUIObject(touchPosition);

        if (gesture.isCanceled)
            return;

        // If gesture is targeting an existing object we are done.
        // Allow for test planes
        if (gesture.targetObject != null && gesture.targetObject.layer != 9)
            return;

        //Checking if touch is over UI element
        if (isOverUI)
        {
            Debug.Log("UI is touched");
        }

        Debug.Log(isOverUI);
        // if (!isOverUI && GestureTransformationUtility.Raycast(gesture.StartPosition, hits, TrackableType.PlaneWithinPolygon))
        // Raycast against the location the player touched to search for planes.
        if (GestureTransformationUtility.Raycast(gesture.startPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            var hit = hits[0];
            
            // Use hit pose and camera pose to check if hittest is from the
            // back of the plane, if it is, no need to create the anchor.
            if (Vector3.Dot(Camera.main.transform.position - hit.pose.position, hit.pose.rotation * Vector3.up) < 0)
                return;
            
            //if(placementObject == null)
            //{
                placementObject = Instantiate(placementPrefab, hit.pose.position, hit.pose.rotation);

                // Create anchor to track reference point and set it as the parent of placementObject.
                // TODO: this should update with a reference point for better tracking.
                var anchorObject = new GameObject("PlacementAnchor");
                anchorObject.transform.position = hit.pose.position;
                anchorObject.transform.rotation = hit.pose.rotation;
                placementObject.transform.parent = anchorObject.transform;

                // Find trackables object in scene and use that as parent
                if (trackablesObject == null)
                    trackablesObject = GameObject.Find("Trackables");
                if (trackablesObject != null)
                    anchorObject.transform.parent = trackablesObject.transform;
            //}
        }
    }
}
