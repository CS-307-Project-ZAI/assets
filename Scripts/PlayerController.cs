using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : PersonController {

	public List<AllyController> allies;
	public bool buildingRotation = false;
	public int buildRate = 1;
    public QuestLog questLog;
	public bool enoughMaterials = false;
	public bool checkMaterials = true;
	public Dictionary<string, int> playerInventory = null;
	public int buildingSelected = 1;

    new void Start() {
		base.Start ();
		if (playerInventory == null) {
			playerInventory = itemDic;
		}
		questLog = GetComponent<QuestLog>();
		questLog.questLogOwner = this;
	}

	// GMUpdate is called by the GameManager once per frame
	public void GMUpdate () {
		if (gm.gameOver) {
			return;
		}
		getMovement ();
		getRotation ();
		foreach (Weapon w in weapons) {
			w.ControlledUpdate ();
		}
		getActions ();
    }

	void getMovement() {
		float moveX = Input.GetAxisRaw ("Horizontal");
		float moveY = Input.GetAxisRaw ("Vertical");
		Vector3 movement = new Vector3 (moveX * moveSpeed, moveY * moveSpeed, 0);
		transform.position += Vector3.ClampMagnitude(movement, moveSpeed) * Time.deltaTime;
	}

	void getRotation() {
		Vector3 mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		transform.rotation = Quaternion.LookRotation (Vector3.forward, mousePos - transform.position);
		transform.Rotate (new Vector3 (0, 0, rotationFix));
	}

	void getActions() {
		if (gm.playerMode == "Combat") { //Get actions for Combat mode
			//Changing Weapons
			if (Input.GetKeyDown (KeyCode.Alpha1) && weapons.Count > 0 && currentWeapon != 0) {
				reloading = false;
				weapons [currentWeapon].SendMessage ("interruptReload");
				currentWeapon = 0;
				gm.ui.selectWeapon (0);
				return;
			} else if (Input.GetKeyDown (KeyCode.Alpha2) && weapons.Count > 1 && currentWeapon != 1) {
				reloading = false;
				weapons [currentWeapon].SendMessage ("interruptReload");
				currentWeapon = 1;
				gm.ui.selectWeapon (1);
				return;
			} else if (Input.GetKeyDown (KeyCode.Alpha3) && weapons.Count > 2 && currentWeapon != 2) {
				reloading = false;
				weapons [currentWeapon].SendMessage ("interruptReload");
				currentWeapon = 2;
				gm.ui.selectWeapon (2);
				return;
			} else if (Input.GetKeyDown (KeyCode.Alpha4) && weapons.Count > 3 && currentWeapon != 3) {
				reloading = false;
				weapons [currentWeapon].SendMessage ("interruptReload");
				currentWeapon = 3;
				gm.ui.selectWeapon (3);
				return;
			} else if (Input.GetKeyDown (KeyCode.Alpha5) && weapons.Count > 4 && currentWeapon != 4) {
				reloading = false;
				weapons [currentWeapon].SendMessage ("interruptReload");
				currentWeapon = 4;
				gm.ui.selectWeapon (4);
				return;
			} else if (Input.GetKeyDown (KeyCode.Alpha6) && weapons.Count > 5 && currentWeapon != 5) {
				reloading = false;
				weapons [currentWeapon].SendMessage ("interruptReload");
				currentWeapon = 5;
				gm.ui.selectWeapon (5);
				return;
			} else if (Input.GetKeyDown (KeyCode.Q) && weapons.Count > 1) {
				//Select previous weapon
				reloading = false;
				weapons [currentWeapon].SendMessage ("interruptReload");
				currentWeapon = (currentWeapon - 1);
				if (currentWeapon < 0) {
					currentWeapon = weapons.Count - 1;
				}
				gm.ui.selectWeapon (currentWeapon);
				return;
			} else if (Input.GetKeyDown (KeyCode.E) && weapons.Count > 1) {
				//Select next weapon
				reloading = false;
				weapons [currentWeapon].SendMessage ("interruptReload");
				currentWeapon = (currentWeapon + 1) % weapons.Count;
				gm.ui.selectWeapon (currentWeapon);
				return;
			}
			if (!reloading) {
				if (Input.GetMouseButton (0)) {
					fireWeapon ();
				} else {
					if (attackTimer < weapons [currentWeapon].fireRate) {
						attackTimer += Time.deltaTime;
					} else {
						attackTimer = weapons [currentWeapon].fireRate;
					}
				}

				if ((Input.GetKeyDown (KeyCode.R) || (Input.GetMouseButton (0) && weapons [currentWeapon].currentLoaded == 0))
				    && (weapons [currentWeapon].ammoPool > 0 || weapons [currentWeapon].ammoPool == -1)
					&& weapons[currentWeapon].currentLoaded != weapons[currentWeapon].clipSize
				    && weapons [currentWeapon].clipSize != -1) {
					reloading = true;
				}
			} else {
				if (Input.GetMouseButton (0) && weapons [currentWeapon].currentLoaded > 0) {
					reloading = false;
					weapons [currentWeapon].SendMessage ("interruptReload");
					fireWeapon ();
				}
			}
			if (Input.GetMouseButtonDown (1)) { //Player right-clicks in Combat mode
				GameObject obj = gm.getClickedObject (2);
				if (obj != null) {
					if (obj.tag == "Enemy") {
						//Target enemy
						Debug.Log("Enemy targeted");
						gm.targetEnemy (obj.GetComponent<EnemyController> ());
					}
				}
			}
		} else if (gm.playerMode == "Command") { //Get actions for Command mode
			if (Input.GetMouseButtonDown (0)) {
				if (gm.setWaypoints && gm.selectedAlly != null) {
					Vector3 mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
					gm.selectedAlly.addWaypoint (mousePos);
				} else {
					GameObject obj = gm.getClickedObject (0);
					if (obj != null) {
						if (obj.tag == "Ally") {
							gm.selectedAlly = obj.GetComponent<AllyController> ();
						} else if (!EventSystem.current.IsPointerOverGameObject ()) {
							gm.deselectAlly ();
						}
					} else if (!EventSystem.current.IsPointerOverGameObject ()) {
						gm.deselectAlly ();
					}
				}
			} else if (Input.GetMouseButtonDown (1)) {
				if (gm.selectedAlly != null) {
					if (gm.setWaypoints) {
						Debug.Log ("Removing waypoint");
						GameObject obj = gm.getClickedObject (0);
						if (obj != null) {
							if (obj.tag == "Waypoint") {
								gm.selectedAlly.movePoints.Remove (obj.transform.position);
								Destroy (obj);
							}
						}
					} else {
						gm.selectedAlly.commandMove (Camera.main.ScreenToWorldPoint (Input.mousePosition));
					}
				}
			} else if (Input.GetKeyDown (KeyCode.Q) && gm.selectedAlly != null) {
				gm.selectedAlly.removeLastWaypoint ();
			} else if (Input.GetKeyDown (KeyCode.E) && gm.selectedAlly != null) {
				gm.toggleWaypoints ();
			}
		} else if (gm.playerMode == "Build") { //Get actions for Build mode
			//Will Change Building according to UI Building Tier selected in the dropdown
			if (Input.GetKeyDown (KeyCode.Alpha1) && buildingSelected != 1) {
				gm.ui.forceBuildingChange (0);
				checkMaterials = true;
				return;
			} else if (Input.GetKeyDown (KeyCode.Alpha2) && buildingSelected != 2) {
				gm.ui.forceBuildingChange (1);
				checkMaterials = true;
				return;
			} else if (Input.GetKeyDown (KeyCode.Alpha3) && buildingSelected != 3) {
				gm.ui.forceBuildingChange (2);
				checkMaterials = true;
				return;
			} else if (Input.GetKeyDown (KeyCode.Alpha4) && buildingSelected != 4) {
				gm.ui.forceBuildingChange (3);
				checkMaterials = true;
				return;
			}

			if (gm.devMode) {
				if (Input.GetKeyDown (KeyCode.U)) {
					playerInventory ["cloth"]++;
					checkMaterials = true;
				}
				if (Input.GetKeyDown (KeyCode.I)) {
					playerInventory ["wood"]++;
					checkMaterials = true;
				}
				if (Input.GetKeyDown (KeyCode.O)) {
					playerInventory ["stone"]++;
					checkMaterials = true;
				}
				if (Input.GetKeyDown (KeyCode.P)) {
					playerInventory ["metal"]++;
					checkMaterials = true;
				}
			}

			if (Input.GetKeyDown(KeyCode.R)) {
				buildingRotation = !buildingRotation;
			}
			if (Input.GetKeyDown (KeyCode.E) || (gm.build && Input.GetMouseButtonDown(1))) {
				gm.toggleBuild (false);
			}
			if (Input.GetKeyDown (KeyCode.Q)) {
				gm.toggleBuildDestroy ();
			}

			if (checkMaterials) {
				enoughMaterials = false;
				Building temp = null;
				switch (buildingSelected) {
				case 1:
					temp = Resources.Load ("Buildings/Tier1Wall", typeof(Building)) as Building;
					break;
				case 2:
					temp = Resources.Load ("Buildings/Tier2Wall", typeof(Building)) as Building;
					break;
				case 3:
					temp = Resources.Load ("Buildings/Tier3Wall", typeof(Building)) as Building;
					break;
				case 4:
					temp = Resources.Load ("Buildings/Toolbench", typeof(Building)) as Building;
					break;
				}
				enoughMaterials = checkBuildingMaterials (temp);
				checkMaterials = false;
			}
				

			if (gm.build && Input.GetMouseButtonDown (0) && gm.ui.pd.checkPlacement()) {
				Vector3 mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
				string loadBuilding = "Buildings/";
				switch (buildingSelected) {
				case 1:
					loadBuilding += "Tier1Wall";
					break;
				case 2:
					loadBuilding += "Tier2Wall";
					break;
				case 3:
					loadBuilding += "Tier3Wall";
					break;
				case 4:
					loadBuilding += "Toolbench";
					break;
				}

				Building newBuilding = Resources.Load (loadBuilding, typeof(Building)) as Building;
				newBuilding = (Building)Instantiate (newBuilding, new Vector3 (mousePos.x, mousePos.y, 0), 
					Quaternion.LookRotation (Vector3.forward, mousePos - transform.position));
				newBuilding.gm = gm;
				gm.buildings.Add (newBuilding);
				if (buildingRotation) {
					newBuilding.transform.Rotate (new Vector3 (0, 0, 90.0f));
				}
				gm.toggleBuild (true);

				//Clear resources that have been consumed
				spendMaterials(newBuilding);
				checkMaterials = true;
			}

			if (gm.buildDestroy && Input.GetMouseButtonDown (0)) {
				GameObject obj = gm.getClickedObject (1);
				if (obj != null) {
					if (obj.tag == "Building") {
						gm.buildings.Remove((Building) obj.GetComponent<Building>());
						Destroy (obj);
						gm.recreateGrid ();
					}
				}
			}
		}

        if (Input.GetKeyDown(KeyCode.F))
        {
            AllyController a = getMeleeAlly();
            if (a != null)
            {
                handleQuestInput(a);
            }
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            printQuestLog();
        }
    }

	private void spendMaterials(Building b) {
		for (int i = 0; i < b.materialsNeeded.Length; i++) {
			playerInventory [Building.materialNames [i]] -= b.materialsNeeded [i];
		}
	}

	private bool checkBuildingMaterials(Building b) {
		if (b == null) {
			return false;
		}
		for (int i = 0; i < b.materialsNeeded.Length; i++) {
			//Check each Material in the Building requirements
			if (playerInventory [Building.materialNames [i]] < b.materialsNeeded[i]) {
				return false;
			}
		}
		return true;
	}

    public void printQuestLog()
    {
        foreach (Quest q in questLog.quests)
        {
            Debug.Log(q.getLogString());
        }
    }

    public AllyController getMeleeAlly()
    {
        foreach (AllyController a in gm.people)
        {
            if (Vector3.Magnitude(a.transform.position - transform.position) <= 3f)
            {
                if (a.questToGive != null && a.leader == null)
                {
                    return a;
                }
            }
        }
        return null;
    }

    private void handleQuestInput(AllyController a)
    {
        if (questLog.quests.Contains(a.questToGive))
        {
            questLog.turninQuest(a, a.questToGive.getQuestID());
        }
        else
        {
            a.assignQuest(questLog);
        }
    }

    public void addAlly(AllyController ally) {
		allies.Add (ally);
		ally.leader = this;
		ally.stats.mode = "Standstill";
	}

	void removeAlly(AllyController ally) {
		allies.Remove (ally);
	}

	public override void aliveCheck() {
		if (health <= 0 && !gm.gameOver) {
			health = 0;
			gm.endGame ();
		}
	}
		
	public void addItem(string key, int num) {
		playerInventory [key] += num;
		checkMaterials = true;
	}
}