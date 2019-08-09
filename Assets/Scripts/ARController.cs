using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using GoogleARCore;

public class ARController : MonoBehaviour {

    //The goal of this script is to create the player controller for AR Portal 1

    public Camera leCamera;

    public GameObject SearchUI;
    public RectTransform searchIcon;

    public GameObject spawnObject;
    GameObject dropPoint;
    PostcardPortal lePort;

    List<DetectedPlane> allPlanes = new List<DetectedPlane>();

    bool isQuiting = false;
    float spawnTimer = 0f;
    // Use this for initialization
    void Start () {
        dropPoint = new GameObject();
        GameObject newPort = Instantiate(spawnObject);
        lePort = newPort.GetComponent<PostcardPortal>();
        newPort.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {

        // Search UI - Is it needed
        Session.GetTrackables<DetectedPlane>(allPlanes);
        bool searchBool = true;

        for (int i = 0; i < allPlanes.Count; i++)
        {
            if (allPlanes[i].TrackingState == TrackingState.Tracking)
            {
                searchBool = false;
                break;
            }
        }

        searchingUI(searchBool);

        Vector3 goToPos = Vector3.zero;

        if (Application.isEditor)
        {
            if (!Input.GetMouseButton(0))
            {
                spawnTimer = 0f;
                return;
            }

            goToPos = Input.mousePosition;
        }
        else
        {
            //Check if Touch is happening

            if (Input.touchCount < 1)
            {
                spawnTimer = 0f;
                return;
            }

            Touch touch = Input.GetTouch(0);

            goToPos = touch.position;
        }

        if (spawnTimer <= 0.05f)
        {
            spawnTimer += Time.deltaTime;
            return;
        }

        //Check Raycast based on where Player taps
        TrackableHit leHit;
        TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon | TrackableHitFlags.FeaturePointWithSurfaceNormal;

        if (Frame.Raycast(goToPos.x, goToPos.y, raycastFilter, out leHit))
        {
            if ((leHit.Trackable is DetectedPlane) && Vector3.Dot(leCamera.transform.position - leHit.Pose.position, leHit.Pose.rotation * Vector3.up) < 0)
            {
                //Object is on opposite side of Plane
                return;
            } else
            {

                var anchor = leHit.Trackable.CreateAnchor(leHit.Pose);

                dropPoint.transform.parent = anchor.transform;
                dropPoint.transform.position = anchor.transform.position;
                dropPoint.transform.tag = "Portal";
                lePort.gameObject.SetActive(true);
                lePort.onDropPoint(dropPoint.transform.position, leHit.Pose.rotation);
                lePort.transform.parent = anchor.transform;
                GameObject.FindObjectOfType<RoomScript>().transform.parent = anchor.transform;
                Destroy(GameObject.Find("Plane Generator"));
                Destroy(gameObject);
            }
        }
    }

    void searchingUI(bool activate)
    {
        SearchUI.SetActive(activate);
        if (activate)
        {
            searchIcon.localEulerAngles += new Vector3(0f, 0f, 120f * Time.deltaTime);
        }
    }
}
