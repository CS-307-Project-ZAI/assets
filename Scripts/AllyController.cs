﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllyController : PersonController {

	public PlayerController leader = null;
	public string aggression = "Defensive";

	public List<Vector3> movePoints;
	public List<GameObject> waypoints;
	public Vector3 targetPos;
	public Vector3 actionPoint;
	public bool cyclic = true;
	public float waitTime;
	[Range(0,2)]
	public float easeAmount;
	float nextMoveTime;
	float percentBetweenPoints;
	int fromPoint = 0;
	int toPoint = 0;

	bool onPath = false;
	public float actionDelay = 0.5f;
	float actionTimer = 0.0f;
	public Vector3 previousPosition;
	bool positionFix = false;
	public float flightDistance = 1.0f;

	public Attributes stats;

    public Quest questToGive = null;

    // Use this for initialization
    new void Start () {
		base.Start ();

		stats = (Attributes)Instantiate(gm.Attribute);
		stats.setOwner(this);
		targetPos = transform.position;
		previousPosition = transform.position;
		if (stats.mode == "Points") {
			positionFix = true;
			onPath = false;
		}

        Quest.initRandomQuest(this);
    }

    // Update is called once per frame
    public void GMUpdate () {
		if (kill) {
			return;
		}
		if (stats.prevMode != stats.mode) { 
			stats.prevMode = stats.mode;

			StopCoroutine ("FollowPath");
			if (stats.mode == "Points") {
				positionFix = true;
				onPath = false;
				Debug.Log (toPoint);
				targetPos = movePoints [toPoint];
				PathRequestManager.RequestPath (this, transform.position, targetPos, OnPathFound);
			}
		}
		getActions ();
		if (!performingAction) {
			getMovement ();
			getRotation ();
		} else {
			transform.rotation = Quaternion.LookRotation (Vector3.forward, actionPoint - transform.position);
			transform.Rotate (new Vector3 (0, 0, rotationFix));
		}
		foreach (Weapon w in weapons) {
			w.ControlledUpdate ();
		}
		stats.GMUpdate ();
	}

	void getMovement() {
		pathFindTimer += (pathFindTimer >= pathRefreshTime ? 0.0f : Time.deltaTime);
		switch (stats.mode) {
		case "Command":
			if (targetPos != transform.position) {
				if (pathFindTimer >= pathRefreshTime) {
					pathFindTimer = 0.0f;
					PathRequestManager.RequestPath (this, transform.position, targetPos, OnPathFound);
				}
			}
			break;
		case "Points":
			if (movePoints.Count > 0) {
				//Check distance to next point
				if (getNextPoint) {
					getNextPoint = false;
					fromPoint++;
					fromPoint %= movePoints.Count;
					toPoint = (fromPoint + 1) % movePoints.Count;
					targetPos = movePoints [toPoint];
					PathRequestManager.RequestPath (this, transform.position, targetPos, OnPathFound);
				} else if (gm.recheckPaths) {
					PathRequestManager.RequestPath (this, transform.position, targetPos, OnPathFound);
				} else if (pathFindTimer >= pathRefreshTime) {
					pathFindTimer = 0.0f;
					//PathRequestManager.RequestPath (this, transform.position, targetPos, OnPathFound);
				}
			} else {
				targetPos = transform.position;
				followingPath = false;
				StopCoroutine ("FollowPath");
			}
			break;
		case "Wander":
			break;
		}
	}

	void getRotation() {
		switch (stats.mode) {
		case "Command":
			transform.rotation = Quaternion.LookRotation (Vector3.forward, actionPoint - transform.position);
			transform.Rotate (new Vector3 (0, 0, rotationFix));
			break;
		case "Points":
			transform.rotation = Quaternion.LookRotation (Vector3.forward, targetPos);
			transform.Rotate (new Vector3 (0, 0, rotationFix));
			break;
		case "Wander":
			break;
		}
	}

	void getActions() {
		if (weapons.Count > 0) {
			if (stats.closestEnemy != null) {
				if (!performingAction && onPath) {
					previousPosition = transform.position;
				}
				performingAction = true;
				if (stats.Panicked) {
					Vector3 direction = Vector3.ClampMagnitude(stats.influenceOfNPCs * 200, 0.2f * stats.speed) * Time.deltaTime;
					transform.position += direction;
					return;
				} else {
					actionPoint = stats.closestEnemy.transform.position;
					if (weapons[currentWeapon].currentLoaded > 0) {
						if (actionTimer >= actionDelay) {
							fireWeaponAt(actionPoint);
						} else {
							actionTimer += Time.deltaTime;
						}
					} else {
						reloading = true;
					}
				}
			} else {
				if (performingAction) {
					if (Mathf.Abs(previousPosition.x - transform.position.x) > .001 || Mathf.Abs(previousPosition.y - transform.position.y) > .001) {
						positionFix = true;
						onPath = false;
						actionPoint = targetPos;
					}
				}
				performingAction = false;
				actionTimer = 0.0f;
			}
		}
	}

	public void becomeFollower(PlayerController p) {
		leader = p;
		questToGive = null;
	}

	public void assignQuest(QuestLog questLog) {
		if (questToGive == null) {
			Quest.initRandomQuest (this);
		}
		questLog.addQuest(questToGive);
	}

	public void commandMove(Vector3 pos) {
		stats.mode = "Command";
		targetPos = new Vector3(pos.x, pos.y, 0);
		PathRequestManager.RequestPath (this, transform.position, targetPos, OnPathFound);
	}

	public void addWaypoint(Vector3 pos) {
		Debug.Log ("Add Waypoint!");
		Vector3 newPoint = new Vector3 (pos.x, pos.y, 0);
		string load = "Other/Waypoint";
		GameObject newWaypoint = (GameObject) Instantiate(Resources.Load (load, typeof(GameObject)) as GameObject);
		newWaypoint.transform.position = newPoint;
		waypoints.Add (newWaypoint);
		movePoints.Add (newPoint);
	}

	public void removeWaypointByClick(Vector3 pos) {
		GameObject obj = gm.getClickedObject (0);
		if (obj.tag == "Waypoint") {
			movePoints.RemoveAt(waypoints.IndexOf (obj));
			waypoints.Remove (obj);
			Destroy (obj);
		}
	}

	public void removeLastWaypoint() {
		int lastIndex = waypoints.Count - 1;
		movePoints.RemoveAt(lastIndex);
		GameObject obj = waypoints [lastIndex];
		waypoints.RemoveAt(lastIndex);
		Destroy (obj);
	}
}
