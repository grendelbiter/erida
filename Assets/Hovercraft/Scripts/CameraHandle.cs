using UnityEngine;

/// <summary>
/// Handles camera following car and mouse orbit
/// </summary>
public class CameraHandle : MonoBehaviour
{
    /// <summary>
    /// Transform that camera should follow
    /// </summary>
    public Transform Tracked;
    /// <summary>
    /// Follow smoothing
    /// </summary>
    public float Smoothing=1000;
    /// <summary>
    /// Mouse orbit sensitivity
    /// </summary>
    public float RotSensitivity=1;

    /// <summary>
    /// Transform that rotates camera (this is to decouple Tracked rotation from cam rotation)
    /// </summary>
    public Transform CameraRotation;
    /// <summary>
    /// Delay until camera returns to default rotation when mouse is not touched
    /// </summary>
    public float ReturnRotationDelay = 3;
    /// <summary>
    /// Time to (smoothly) return camera to default position
    /// </summary>
    public float ReturnRotationTime = 1;

    private float m_LastRotationTime;
    private Quaternion m_ReturnFrom;

    void Update()
    {
        // follow tracked target, wrt both position and rotation
        transform.position = Vector3.Lerp(transform.position, Tracked.position, Time.smoothDeltaTime * Smoothing);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, Tracked.rotation.eulerAngles.y, 0),
                                                 Time.smoothDeltaTime * Smoothing);

        // mouse movement this frame
        var x = Input.GetAxis("Mouse X");
        var y = Input.GetAxis("Mouse Y");

        if (Mathf.Approximately(x, 0) && Mathf.Approximately(y, 0))
        {
            if (Time.time - m_LastRotationTime > ReturnRotationDelay)
            {
                // if mouse never moved for ReturnRotationDelay, return camera back to default rotation
                // return must be smooth, so use slerp over some time instead of snapping
                var t = Mathf.Clamp01((Time.time - m_LastRotationTime - ReturnRotationDelay) / ReturnRotationTime);
                CameraRotation.localRotation = Quaternion.Slerp(m_ReturnFrom, Quaternion.identity, t);
            }
        }
        else
        {
            // remember time to track return
            m_LastRotationTime = Time.time;
            // rotate camera according to mouse movement
            var e = CameraRotation.localRotation.eulerAngles;
            e.x = Mathf.Clamp(-Mathf.DeltaAngle(e.x, 0) - y * RotSensitivity, -10,70); // clamp vertical rotation to prevent gimbal lock
            e.y += x * RotSensitivity;
            e.z = 0;
            m_ReturnFrom = CameraRotation.localRotation = Quaternion.Euler(e);
        }
    }
}
