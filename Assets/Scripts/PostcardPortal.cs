using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class PostcardPortal : MonoBehaviour {

    //This script is to provide the behaviour for the portal in AR Portal 2 - This will be a portal falling from the sky, landing on the ground, and letting the player walk in and out of it

    float yLength;
    bool doneLanding = false;
    Camera selectedCamera;
    RoomScript leRoom;

    float distanceFromCircle = 0.5f;
    float pushPortal = 0.75f;
    bool wrongSide = false;

    public MeshRenderer[] insideMeshs;
    public MeshRenderer[] outsideMeshs;
    public List<Material> fadeMats = new List<Material>();

    float maxDist = 1f;
    float distCalc;

    // Use this for initialization
    void OnEnable () {
        if (selectedCamera == null)
        {

            initialStart();
        }
	}
	
	// Update is called once per frame
	void Update () {
		if (!doneLanding)
        {
            return;
        }

        if (isInside())
        {
            if (wrongSide)
            {
                return;
            }
            leRoom.inRoom = !leRoom.inRoom;
            if (leRoom.inRoom)
            {
                transform.position -= transform.forward * pushPortal;
            } else
            {
                transform.position += transform.forward * pushPortal;
            }
            SetMaterials(leRoom.inRoom);
            SetPostcard(leRoom.inRoom);
            wrongSide = true;
        } else
        {
            float trans = Mathf.Clamp((Vector3.Distance(transform.position, selectedCamera.transform.position) - distanceFromCircle - 0.25f) * distCalc, 0f, 1f);
            for (int i = 0; i < fadeMats.Count; i++)
            {
                Color OriginalCol = fadeMats[i].GetColor("_TintColor");
                fadeMats[i].SetColor("_TintColor", new Vector4(OriginalCol.r, OriginalCol.g, OriginalCol.b, trans));
            }
            wrongSide = false;

        }
	}

    void initialStart()
    {
        selectedCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        yLength = GetComponent<MeshRenderer>().bounds.extents.y / transform.localScale.y;
        leRoom = FindObjectOfType<RoomScript>();
        SetMaterials(false);
        SetPostcard(false);
        distCalc = 1f / (maxDist - distanceFromCircle);
    }

    public void onDropPoint(Vector3 pos, Quaternion rot)
    {
        transform.rotation = rot;
        Vector3 endy = pos;
        endy += transform.up * yLength * transform.localScale.y;
        StartCoroutine(dropTransition(endy, pos));
    }

    IEnumerator dropTransition(Vector3 end, Vector3 origPos)
    {
        Vector3 start = end + (Vector3.up * 10f);
        float initialXScale = transform.localScale.x;
        float initialYScale = transform.localScale.y;
        transform.localScale = new Vector3(initialXScale * 0.5f, initialYScale * 2f, transform.localScale.z);
        Vector3 tempEnd = origPos + (transform.up * yLength * transform.localScale.y);
        float OriginalRoomScale = leRoom.transform.localScale.x;

        float Timer = 0f;
        float ySpeed = 0f;

        while (Timer < 1f)
        {
            if (leRoom.PortalSeen)
            {
                leRoom.enabled = false;
                leRoom.transform.localScale = new Vector3(OriginalRoomScale * 0.5f, OriginalRoomScale * 2f, leRoom.transform.localScale.z);
            }
            Timer += Time.deltaTime * 4f;
            transform.position = Vector3.Lerp(start, tempEnd, Timer);

            if (ySpeed == 0f)
            {
                ySpeed = Vector3.Distance(transform.position, start);
            }
            yield return null;
        }

        float friction = 1.2f;
        float strength = 1f;
        float diff = (transform.localScale.y / initialYScale);
        float startDiff = diff;
        float xRatio = 99999f;
        float yRatio = 2f;

        while (xRatio != yRatio)
        {
            float currentPos = diff;
            ySpeed /= friction;
            ySpeed += (currentPos) * Time.deltaTime * strength;
            diff -= ySpeed;

            float newRatio = diff;

            if (newRatio < 0f)
            {
                newRatio = (startDiff + diff) / startDiff;

          } else {
                newRatio += 1f;
            }


            yRatio = newRatio;
            xRatio = 1f / newRatio;

            transform.localScale = new Vector3(initialXScale * xRatio, initialYScale * yRatio, transform.localScale.z);
            leRoom.transform.localScale = new Vector3(OriginalRoomScale * xRatio, OriginalRoomScale * yRatio, leRoom.transform.localScale.z);
            transform.position = origPos + (transform.up * yLength * transform.localScale.y);

            yRatio = Mathf.Round(yRatio * 1000f) / 1000f;
            xRatio = Mathf.Round(xRatio * 1000f) / 1000f;

            yield return null;
        }
            leRoom.enabled = true;
            leRoom.transform.localScale = new Vector3(OriginalRoomScale, OriginalRoomScale, leRoom.transform.localScale.z);


        transform.localScale = new Vector3(initialXScale, initialYScale, transform.localScale.z);
        transform.position = end;
        doneLanding = true;
    }

    bool isInside()
    {
        bool correct = true;
        Transform returnTo = selectedCamera.transform.parent;
        selectedCamera.transform.parent = transform;
        if (Mathf.Abs(selectedCamera.transform.localPosition.x) > 0.5f || Mathf.Abs(selectedCamera.transform.localPosition.y) > 0.5f)
        {
            correct = false;
        }

        if (Mathf.Abs(selectedCamera.transform.localPosition.z) * transform.localScale.z > distanceFromCircle)
        {
            correct = false;
        } else
        {
            if ((leRoom.inRoom && selectedCamera.transform.localPosition.z < 0f) || (!leRoom.inRoom && selectedCamera.transform.localPosition.z > 0f))
            {
                wrongSide = true;
            }
        }
        selectedCamera.transform.parent = returnTo;

        return correct;
    }

    void SetMaterials(bool fullRender)
    {
        var stencilTest = fullRender ? CompareFunction.NotEqual : CompareFunction.Equal;
        Shader.SetGlobalInt("_StencilTest", (int)stencilTest);
    }

    void SetPostcard(bool insideRoom)
    {
        if (insideRoom)
        {
            for (int i = 0; i < insideMeshs.Length; i++)
            {
                insideMeshs[i].enabled = true;
            }

            for (int i = 0; i < outsideMeshs.Length; i++)
            {
                outsideMeshs[i].enabled = false;
            }
        } else
        {
            for (int i = 0; i < insideMeshs.Length; i++)
            {
                insideMeshs[i].enabled = false;
            }

            for (int i = 0; i < outsideMeshs.Length; i++)
            {
                outsideMeshs[i].enabled = true;
            }
        }
    }
}
