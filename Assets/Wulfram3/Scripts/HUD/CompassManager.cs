using Com.Wulfram3;
using UnityEngine;
using UnityEngine.UI;

public class CompassManager : MonoBehaviour {

    public RawImage compass;

    void Update()
    {
        var degree = PlayerManager.LocalPlayerInstance.transform.localEulerAngles.y / 360f;
        degree = degree - 0.5f;
        if (degree < 0)
            degree++;
        compass.uvRect = new Rect(degree, 0, 1, 1);
    }
}
