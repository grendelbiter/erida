using UnityEngine;

/// <summary>
/// Applies drag to rigidbody depending on its orientation.
/// </summary>
public class SideDependentDrag : MonoBehaviour
{
    public Vector3 DragCoeffs;

    void FixedUpdate()
    {
        var rb = GetComponent<Rigidbody>();
        var rv = transform.InverseTransformDirection(rb.velocity);
        var drag = new Vector3(rv.x * DragCoeffs.x,
                               rv.y * DragCoeffs.y,
                               rv.z * DragCoeffs.z);
        rb.AddRelativeForce(-drag * rb.mass);
    }

}
