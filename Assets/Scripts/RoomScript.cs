using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomScript : MonoBehaviour {
    //This script is to ensure that the player stays inside of the room of the portal while they are in it
    public bool mainRoom = true;
    internal bool inRoom = false;
    Camera MainCamera;
    Transform portal;

    static WebCamTexture backCam;

    float tilingX = 0f;
    DeviceOrientation lastOrientation;

    float startScale;

    internal bool PortalSeen = false;
    // Use this for initialization
    void Start () {
        MainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        startScale = transform.localScale.x * 0.5f;
    }
	
	// Update is called once per frame
	void Update () {
        

        if (inRoom)
        {
            float scaler = Mathf.Clamp(Vector3.Distance(transform.position, MainCamera.transform.position) + 0.5f, startScale, Mathf.Infinity) * 2f;
            transform.localScale = new Vector3(scaler, scaler, scaler);
            
        } else if (transform.localScale.x != startScale * 2f)
        {
            transform.localScale = new Vector3(startScale * 2f, startScale * 2f, startScale * 2f);
        }

        if (portal == null)
        {
            if (GameObject.FindGameObjectWithTag("Portal"))
            {
                portal = GameObject.FindGameObjectWithTag("Portal").transform;

            }
            return;
        }

        if (!portal.gameObject.activeInHierarchy)
        {
            PortalSeen = false;
            return;
        }

        if (!inRoom && !PortalSeen)
        {
            transform.position = portal.position;
            PortalSeen = true;
        }
    }



}
