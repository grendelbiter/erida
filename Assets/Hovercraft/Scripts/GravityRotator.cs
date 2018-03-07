using UnityEngine;

public class GravityRotator : MonoBehaviour
{
    void OnTriggerEnter()
    {
        Physics.gravity = new Vector3(0, 0, 9.81f);
    }
    void OnTriggerExit()
    {
        Physics.gravity = new Vector3(0, -9.81f, 0);
    }
}
