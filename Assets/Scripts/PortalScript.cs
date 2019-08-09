using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class PortalScript : MonoBehaviour {

    // The goal of this script is to create the Portal for AR Portal 1 - this portal will behave similarily to the portals seen  in Rick and Morty where a circular portal will appear 
    // where the player taps, the player walks through it and the portal will close behind them

    //Opening portal
    Vector3 fullSize;
    Coroutine openPortal_C;
    LineRenderer line;
    bool DoneOpening;
    public float maxLiquidLength = 0.4f;
    Camera selectedCamera;

    //Walking Through Portal
    float distanceFromCircle = 0.5f;

    //Closing Portal
    RoomScript leRoom;
    bool isDone;
    Coroutine closePortal_C;

    //Portal Effect
    float maxDist = 1f;
    float distCalc;
    List<Transform> portalCovers = new List<Transform>();
    List<Material> portalMats = new List<Material>();
    Image[] glowEffect;
    public GameObject rayPrefab;
    List<RayScript> rays = new List<RayScript>();

	// Use this for initialization
	void OnEnable () {
		if (fullSize == null || fullSize == Vector3.zero)
        {
            initialStart();
        }
        DoneOpening = false;
        isDone = false;

        openPortal_C = StartCoroutine(openPortal());

        for (int i = 0; i < portalMats.Count; i++)
        {
            portalMats[i].SetColor("_TintColor", new Vector4(1f, 1f, 1f, 1f));
        }

        for (int i = 0; i < glowEffect.Length; i++)
        {
            glowEffect[i].color = new Vector4(1f, 1f, 1f, 0f);
        }

    }
	
	// Update is called once per frame
	void Update () {

        for (int i = 0; i < portalCovers.Count; i++)
        {
            portalCovers[i].localEulerAngles += new Vector3(0f, 45f * Time.deltaTime, 0f);
        }

        if (!DoneOpening || isDone)
        {
            return;
        }

        if (isInside())
        {
            leRoom.inRoom = !leRoom.inRoom;
            Vector3 dis = transform.position - selectedCamera.transform.position;
            transform.position -= dis;
            SetMaterials(leRoom.inRoom);
            isDone = true;
            closePortal_C = StartCoroutine(closePortal());
        } else
        {
            float trans = Mathf.Clamp((Vector3.Distance(transform.position, selectedCamera.transform.position) - distanceFromCircle - 0.05f) * distCalc, 0f, 1f);
            for (int i = 0; i < portalMats.Count; i++)
            {
                portalMats[i].SetColor("_TintColor", new Vector4(1f, 1f, 1f, trans));
            }
            for (int i = 0; i < glowEffect.Length; i++)
            {
                glowEffect[i].color = new Vector4(1f, 1f, 1f, 1f - trans);
            }
        }
	}

    void initialStart()
    {
        fullSize = transform.localScale;
        line = GetComponent<LineRenderer>();
        selectedCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        leRoom = FindObjectOfType<RoomScript>();

        SetMaterials(false);

        distCalc = 1f / (maxDist - distanceFromCircle);
        
        for (int i = 0; i < transform.childCount; i++)
        {
            portalCovers.Add(transform.GetChild(i));
            portalMats.Add(portalCovers[i].GetComponent<Renderer>().material);
        }

        glowEffect = FindObjectsOfType<Image>();

        for (int i = 0; i< 2; i++)
        {
            GameObject newRay = Instantiate(rayPrefab);
            rays.Add(newRay.GetComponent<RayScript>());
            newRay.SetActive(false);
        }
    }


    bool isInside()
    {
        bool correct = true;
        Transform returnTo = selectedCamera.transform.parent;
        selectedCamera.transform.parent = transform;
        if (Mathf.Abs(selectedCamera.transform.localPosition.x) > 0.5f || Mathf.Abs(selectedCamera.transform.localPosition.z) > 0.5f)
        {
            correct = false;
        }

        if (Mathf.Abs(selectedCamera.transform.localPosition.y) * transform.localScale.y > distanceFromCircle)
        {
            correct = false;
        }
        selectedCamera.transform.parent = returnTo;

        return correct;
    }

    IEnumerator openPortal()
    {
        transform.localScale = Vector3.zero;
        Vector3 startShoot = selectedCamera.transform.position - (selectedCamera.transform.up * Screen.height);
        Vector3 dir = Vector3.Normalize(transform.position - startShoot);
        line.SetPosition(0, startShoot);
        line.SetPosition(1, startShoot);



        while (Vector3.Distance(line.GetPosition(0), line.GetPosition(1)) < maxLiquidLength)
        {
            line.SetPosition(0, line.GetPosition(0) + dir);
            yield return null;
        }

        for (int i = 0; i < rays.Count; i++)
        {
            rays[i].transform.rotation = selectedCamera.transform.rotation;
        }
        startShoot = selectedCamera.transform.position - (selectedCamera.transform.up * 0.5f);
        rays[0].onShoot(startShoot, transform.position, 0.4f);

        Vector3 line0Start = line.GetPosition(0);
        Vector3 line1End = transform.position - (dir * maxLiquidLength);

        float Timer = 0f;

        while (Timer < 1f)
        {
            Timer += Time.deltaTime * 4f;

            line.SetPosition(0, Vector3.Lerp(line0Start, transform.position, Timer));
            Vector3 line1Pos = Vector3.Lerp(startShoot, line1End, Timer);
            line.SetPosition(1, line1Pos);

            yield return null;
        }

        rays[1].onShoot(startShoot, transform.position, 0.3f);

        Timer = 0f;

        while (Timer < 1f)
        {
            Timer += Time.deltaTime * 2f;
            transform.localScale = Vector3.Lerp(Vector3.zero, fullSize, Timer);


            if (line.GetPosition(1) != transform.position)
            {
                Vector3 line1Pos = Vector3.Lerp(line1End, transform.position, Timer * 4f);
                line.SetPosition(1, line1Pos);
            }

            yield return null;
        }
        DoneOpening = true;
    }

    IEnumerator closePortal()
    {
        float Timer = 0f;

        while (Timer < 1f)
        {
            Timer += Time.deltaTime * 2f;
            transform.localScale = Vector3.Lerp(fullSize, Vector3.zero, Timer);
            for (int i = 0; i < glowEffect.Length; i++)
            {
                glowEffect[i].color = new Vector4(1f, 1f, 1f, 1f - Timer);
            }

            yield return null;
        }

        gameObject.SetActive(false);
    }

    void SetMaterials(bool fullRender)
    {
        var stencilTest = fullRender ? CompareFunction.NotEqual : CompareFunction.Equal;
        Shader.SetGlobalInt("_StencilTest", (int)stencilTest);
    }
}
