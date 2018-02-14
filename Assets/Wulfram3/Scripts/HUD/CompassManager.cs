using Com.Wulfram3;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEngine.UI;

public class CompassManager : MonoBehaviour {

    public RawImage compass;
    public Transform player;

    void Update()
    {
        
        compass.uvRect = new Rect(PlayerManager.LocalPlayerInstance.transform.localEulerAngles.y / 360f, 0, 1, 1);

    }
}
