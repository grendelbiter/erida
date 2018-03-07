using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankController : MonoBehaviour {

public float acceleration;
public float rotationRate;
public float turnRotationAngle;
public float turnRotationSeekSpeed;
public Rigidbody rb;

private float rotationVelocity;
private float groundAngleVelocity;

	// Use this for initialization
	void Start () {

		rb = GetComponent<Rigidbody> ();
		
	}

	void FixedUpdate ()
	{

		if (Physics.Raycast (transform.position, -transform.up, 3f)) {

			rb.drag = 1;

			Vector3 forwardForce = transform.forward * acceleration * Input.GetAxis ("Vertical");

			forwardForce = forwardForce * Time.deltaTime * rb.mass;

			rb.AddForce (forwardForce);
		} else {

			rb.drag = 0;
		}

		Vector3 turnTorque = Vector3.up * rotationRate * Input.GetAxis ("Horizontal");

		turnTorque = turnTorque * Time.deltaTime * rb.mass;
		rb.AddTorque (turnTorque);

		Vector3 newRotation = transform.eulerAngles;
		newRotation.z = Mathf.SmoothDampAngle(newRotation.z, Input.GetAxis ("Horizontal") * -turnRotationAngle, ref rotationVelocity, turnRotationSeekSpeed);
		transform.eulerAngles = newRotation;

	}
}
