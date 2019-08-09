using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayScript : MonoBehaviour {

    //The goal of this script is to create the "ray effect" of AR Portal 1
    Coroutine shootCo;

    public void onShoot(Vector3 start, Vector3 end, float Time1)
    {
        gameObject.SetActive(true);
        shootCo = StartCoroutine(shoot(start, end, Time1));
    }

    IEnumerator shoot(Vector3 start, Vector3 end, float Time1)
    {
        float newTime = 1f / Time1;
        float timer = 0f;
        
        while (timer < 1f)
        {
            timer += Time.deltaTime * newTime;
            transform.position = Vector3.Lerp(start, end, timer);
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
