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
	private float BarrelXRot;
	public float BarrelXrotMin;
	public float BarrelXrotMax;
	public float BarrelTurnRate;

	//Projectile
	public Rigidbody projectile;
	public Transform projectileSpawn;
	public float ProjectileForce;
	public float CannonForce;
	public GameObject MuzzleFlash;

	//Camera Control
	public GameObject FirstPersoncam;
	public GameObject ThirdPersoncam;
	private bool ToggleCam;

	//Lights
	public bool SpotlightsToggle;
	public GameObject Spotlights;

    void Start ()
	{
		Cursor.lockState= CursorLockMode.Locked;
		Cursor.visible = false;
		rb = GetComponent<Rigidbody>();	
	}

    void Update ()
	{
		//Rotate Tank with Mouse
		float rotation;
		rotation = Input.GetAxis ("Mouse X");
		Movement.Thrust = Input.GetAxis ("Vertical");
		Strafing.Thrust = Input.GetAxis ("Horizontal");
		Orientation.Turn = rotation * 2;

		//Unlock Cursor with Esc
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

		//Set Hover Height with Q and Y
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

		//Rotate Barrel Up and Down
		BarrelXRot += Input.GetAxis ("Mouse Y") * BarrelTurnRate;
		BarrelXRot = Mathf.Clamp (BarrelXRot, BarrelXrotMin, BarrelXrotMax);
		Barrel.localEulerAngles = new Vector3 (BarrelXRot, Barrel.localEulerAngles.y, Barrel.localEulerAngles.z);

		//Fire Projectiles and Instantiate Muzzle Flash and Add Force and Play Sound
		if (Input.GetButtonDown ("Fire2")) {
			Rigidbody projectileInstance;
			projectileInstance = Instantiate (projectile, projectileSpawn.position, projectileSpawn.rotation) as Rigidbody;
			projectileInstance.AddForce (projectileSpawn.forward * ProjectileForce);
			rb.AddForce (-transform.forward * CannonForce);
			Physics.IgnoreCollision(projectileInstance.GetComponent<Collider>(),GetComponent<Collider>());
			GameObject muzzleflashInstance;
			muzzleflashInstance = Instantiate (MuzzleFlash, projectileSpawn.position, projectileSpawn.rotation) as GameObject;
		}

		//Toggle Lights
		if (Input.GetKeyDown (KeyCode.L)) {
			SpotlightsToggle = !SpotlightsToggle;
			Spotlights.SetActive(SpotlightsToggle);
		}

		//Toggle First Person/Third Person Cam
		if (Input.GetKeyDown (KeyCode.V)) {
			ToggleCam = !ToggleCam;
			FirstPersoncam.SetActive(ToggleCam);

			ThirdPersoncam.SetActive(!ToggleCam);

				
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

		}
		
	}

	
}