using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlasmaExplosion : MonoBehaviour {

public Transform ExplosionPrefab;


void OnCollisionEnter (Collision collsion)
	{
		ContactPoint contact = collsion.contacts [0];
		Quaternion rot = Quaternion.FromToRotation (Vector3.up, contact.normal);
		Vector3 pos = contact.point;
		Instantiate (ExplosionPrefab, pos, rot);
		Destroy (gameObject);
	}

	}

