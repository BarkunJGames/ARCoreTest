using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleARCore;

public class ARController2 : MonoBehaviour {

    //This script will provide the player controls for AR Portal 2
    public Camera leCamera;
    public float dist = 1f;
    public GameObject spawnObject;
    internal GameObject portal;

    float spawnTimer = 0f;
    // Use this for initialization
    void Start () {
        portal = Instantiate(spawnObject, Vector3.zero, spawnObject.transform.rotation);
        portal.SetActive(false);
        spawnTimer = 0f;
	}
	
	// Update is called once per frame
	void Update () {
        if (portal.activeInHierarchy)
        {
            spawnTimer = 0f;
            return;
        }

        Vector3 goToPos = Vector3.zero;

        if (Application.isEditor)
        {
            if (!Input.GetMouseButton(0))
            {
                spawnTimer = 0f;
                return;
            }

            goToPos = Input.mousePosition;
        } else
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


        //INSERT OBJECT INTO 3D WORLD BASED ON TOUCH POSITION
        Vector3 touchFar = leCamera.ScreenToWorldPoint(new Vector3(goToPos.x, goToPos.y, leCamera.farClipPlane));
        Vector3 touchClose = leCamera.ScreenToWorldPoint(new Vector3(goToPos.x, goToPos.y, leCamera.nearClipPlane));
        Vector3 direction = Vector3.Normalize(touchFar - touchClose);

        Vector3 Pos = touchClose + (direction * dist);

        Vector3 Rot = leCamera.transform.eulerAngles;
        Rot = new Vector3(0f, Rot.y, 0f);

        portal.SetActive(true);
        portal.transform.position = Pos;
        portal.transform.eulerAngles = spawnObject.transform.eulerAngles + Rot;
    }
}
