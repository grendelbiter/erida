using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGunFire : MonoBehaviour {

public GameObject muzzleflashPrefab;
public Transform minigunOutput;
public GameObject explosionPrefab;
public Transform raycastOrigin;
public RaycastHit hit;
public float bulletSpread;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Muzzleflash ()
	{
		GameObject muzzleflashInstance;
		muzzleflashInstance = Instantiate (muzzleflashPrefab, minigunOutput.position, minigunOutput.rotation) as GameObject;
		GameObject explosionInstance;
		Vector3 fireDirection = raycastOrigin.forward;
		Quaternion fireRotation = Quaternion.LookRotation(fireDirection);
		Quaternion randomRotation = Random.rotation;

		fireRotation = Quaternion.RotateTowards(fireRotation, randomRotation, Random.Range(0.0f, bulletSpread));

		//Debug.DrawRay (raycastOrigin.position, raycastOrigin.up, Color.green, 250f, true);

		if (Physics.Raycast (raycastOrigin.position, fireRotation.eulerAngles, out hit, 250f)) {
			explosionInstance = Instantiate (explosionPrefab, hit.point, hit.transform.rotation) as GameObject;
		}
	}
}
