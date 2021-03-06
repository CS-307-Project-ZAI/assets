﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

	public float maxDistance = 20.0f;

	[HideInInspector]
	public Vector3 direction;
	public bool kill = false;

	public PersonController owner;
	public int damage = 10;

	public float passthrough = 0.0f;
	
	// Update is called once per frame
	public void GMUpdate () {
		//Check if out of bounds
		if (Mathf.Abs (owner.transform.position.x - transform.position.x) > maxDistance 
			|| Mathf.Abs (owner.transform.position.y - transform.position.y) > maxDistance) {
			kill = true;
			return;
		}
		getMovement ();
	}

	void getMovement() {
		transform.position += direction * Time.deltaTime;
	}

	void OnTriggerEnter2D(Collider2D col) {
		if (col.gameObject.tag == "Enemy") {
			EnemyController e = col.gameObject.GetComponent<EnemyController> ();
			e.applyDamage (damage, owner);
			if ((Random.Range(0, 100) / 100.0) > passthrough) {
				kill = true;
			}
		} else if (col.gameObject.tag == "Block") {
			kill = true;
		} else if (col.gameObject.tag == "Pylon") {
			col.gameObject.SendMessage ("ApplyDamage", damage);
			kill = true;
		}
	}
}
