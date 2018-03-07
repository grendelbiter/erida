using UnityEngine;
using System.Collections;

/// <summary>
/// Routes control from player input to hover engines
/// </summary>
public class HoverControl : MonoBehaviour
{
    /// <summary>
    /// Turning script
    /// </summary>
    public HoverOrientation Orientation;
    /// <summary>
    /// Movement script
    /// </summary>
    public MovementEngine Movement;
    //<summary>
    //Movement Script
    //</summary>
    public MovementEngine Strafing;
    //Rigidbody
    public Rigidbody rb;

    public float JumpForce;

	public float MaxAngleDrift;

	public HoverEngine[] Engines;

	//Barrel Stuff
	public Transform Barrel;
	private float BarrelZRot;
	public float BarrelZrotMin;
	public float BarrelZrotMax;
	public float BarrelTurnRate;

	//Projectile
	public Rigidbody projectile;
	public Transform projectileSpawn;


    void Start ()
	{
		Cursor.lockState= CursorLockMode.Locked;
		Cursor.visible = false;
		rb = GetComponent<Rigidbody>();	
	}

    void Update ()
	{
		float rotation;
		rotation = Input.GetAxis ("Mouse X");
		Movement.Thrust = Input.GetAxis ("Vertical");
		Strafing.Thrust = Input.GetAxis ("Horizontal");
		Orientation.Turn = rotation * 2;

		if (Input.GetKeyDown (KeyCode.Escape)) {
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

		if (Input.GetKeyDown (KeyCode.Q)) {
			foreach (HoverEngine Engine in Engines)
				if (Engine.MaxHeight == 10) {
					Engine.MaxHeight = 10;
				} else {
					Engine.MaxHeight = Engine.MaxHeight + 1;
				}
		}

		if (Input.GetKeyDown (KeyCode.Y)) {
			foreach (HoverEngine Engine in Engines)
				Engine.MaxHeight = Engine.MaxHeight - 1;
		}

		BarrelZRot += Input.GetAxis ("Mouse Y") * BarrelTurnRate;
		BarrelZRot = Mathf.Clamp (BarrelZRot, BarrelZrotMin, BarrelZrotMax);
		Barrel.eulerAngles = new Vector3 (Barrel.eulerAngles.x, Barrel.eulerAngles.y, BarrelZRot);

		if (Input.GetButtonDown("Fire2")) {
		Rigidbody projectileInstance;
		projectileInstance = Instantiate(projectile, projectileSpawn.position, projectileSpawn.rotation) as Rigidbody;
		projectileInstance.AddForce(projectileSpawn.forward * 5000);
		}

    }

    void FixedUpdate ()
	{
		// find force direction by rotating local up vector towards world up
        var up = transform.up;
        var grav = Physics.gravity.normalized;
        up = Vector3.RotateTowards(up, -grav, MaxAngleDrift*Mathf.Deg2Rad, 1);

		if (Input.GetButtonDown ("Jump")) {
		rb.AddForce (up *JumpForce);
//			{
//			StartCoroutine(SetHeight());
//			}
		}
		
	}

	
}