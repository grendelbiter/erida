using Com.Wulfram3;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEngine.UI;

public class CompassManager : MonoBehaviour {

    public RawImage compass;

    void Update()
    {
        var degree = PlayerManager.LocalPlayerInstance.transform.localEulerAngles.y / 360f;
        compass.uvRect = new Rect(degree, 0, 1, 1);
       //Logger.Log("Player Pointing at " + degree);
        Logger.Log("Player Pointing at " + PlayerManager.LocalPlayerInstance.transform.localEulerAngles.y);
    }
}
