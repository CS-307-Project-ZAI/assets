using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonController : MonoBehaviour {

	public float moveSpeed = 0.2f;
	public List<Weapon> weapons;
	public int health = 50;
	public GameManager gm;
	public float rotationFix = 0.0f;
	public string personName = "Person";
    public Dictionary<string, int> itemDic = new Dictionary<string, int>();

	protected Vector3[] path = null;
	protected int targetIndex;
	public PathRequestManager.PathRequest request = null;

	[HideInInspector]
	public bool reloading = false;
	public bool kill = false;
	public GameObject targetTag = null;
	public float pathFindTimer = 0.0f;
	public float pathRefreshTime = .01f;
	public bool followingPath = false;
	public bool getNextPoint = false;
	public int currentWeapon = 0;
	public float attackTimer = 0.0f;
	public bool performingAction = false;
	public List<PersonController> othersInfluenced = new List<PersonController> ();
	public bool interrupted = false;

	protected void Start() {
        itemDic.Add ("cloth", 0);
        itemDic.Add ("wood", 0);
		itemDic.Add ("stone", 0);
        itemDic.Add ("metal", 0);
	}

	protected void fireWeapon() {
		attackTimer += Time.deltaTime;
		if (attackTimer > weapons [currentWeapon].fireRate && weapons [currentWeapon].currentLoaded > 0) {
			weapons [currentWeapon].SendMessage ("fireWeapon");
			attackTimer = 0.0f;
		}
	}

	protected void fireWeaponAt(Vector3 point) {
		attackTimer += Time.deltaTime;
		if (attackTimer > weapons [currentWeapon].fireRate && weapons [currentWeapon].currentLoaded > 0) {
			weapons[currentWeapon].fireWeaponAt(point);
			attackTimer = 0.0f;
		}
	}

	public void applyDamage(int dmg, PersonController from) {
		this.health -= dmg;
		if (this.health < 0) {
			this.health = 0;
		}
		if (this.tag == "Player") {
			gm.ui.updateHealthBar ();
		}
		aliveCheck ();
		if (kill) {
			if (from.tag == "Player" && this.tag == "Enemy") {
				PlayerController pc = (PlayerController)from;
				pc.questLog.addKill (1);
			}
		}
	}

	public virtual void aliveCheck() {
		if (health <= 0) {
			kill = true;
		}
	}

	IEnumerator FollowPath() {
		if (path.Length > 0) {
			followingPath = true;
			targetIndex = 0;
			Vector3 currentWaypoint = path [0];
			while (true) {
				if (performingAction) {
					yield return null;
				}
				if ((Vector3)transform.position == currentWaypoint) {
					targetIndex++;
					if (targetIndex >= path.Length) {
						targetIndex = 0;
						path = null;
						getNextPoint = true;
						yield break;
					}
					currentWaypoint = path [targetIndex];
				}

				transform.position = Vector3.MoveTowards (transform.position, currentWaypoint, moveSpeed * Time.deltaTime);
				yield return null;
			}
		}
	}

	public void OnPathFound(Vector3[] newPath, bool pathSuccessful) {
		if (pathSuccessful) {
			path = newPath;
			targetIndex = 0;
			StopCoroutine ("FollowPath");
			StartCoroutine ("FollowPath");
		}
	}

	public void OnDrawGizmos() {
		if (path != null) {
			for (int i = targetIndex; i < path.Length; i ++) {
				Gizmos.color = Color.black;
				//Gizmos.DrawCube((Vector3)path[i], Vector3.one *.5f);

				if (i == targetIndex) {
					Gizmos.DrawLine(transform.position, path[i]);
				}
				else {
					Gizmos.DrawLine(path[i-1],path[i]);
				}
			}
		}
	}

	public float euclideanDistance(Vector3 pos, Vector3 target) {
		return Mathf.Sqrt (Mathf.Pow(target.x - pos.x, 2) + Mathf.Pow(target.y - pos.y, 2));
	}

	public int getBuildRate() {
		if (transform.gameObject.tag == "Ally") {
			AllyController ac = transform.GetComponent<AllyController> ();
			return ac.stats.buildRate;
		} else if (transform.gameObject.tag == "Player") {
			PlayerController pc = transform.GetComponent<PlayerController> ();
			return pc.buildRate;
		}
		return 0;
	}

	public Weapon addWeapon(string wepName) {
		string load = "Weapons/" + wepName;
		Weapon wep = Resources.Load (load, typeof(Weapon)) as Weapon;
		wep = (Weapon)Instantiate (wep);
		wep.owner = this;
		load = "AmmoTypes/" + wep.ammoType;
		wep.bullet = Resources.Load (load, typeof(Bullet)) as Bullet;
		this.weapons.Add (wep);
		wep.transform.parent = this.transform;
		if (transform.gameObject.tag == "Player") {
			gm.ui.addWepImg (this.weapons.IndexOf (wep), wep.weaponName);
		}
		return wep;
	}

	public void removeWeapon(Weapon w) {

	}
}