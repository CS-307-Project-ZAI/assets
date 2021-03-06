﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System;
using System.IO;

public class PathFinding : MonoBehaviour {
    PathRequestManager requestManager;
    public Grid grid;

    private void Awake() {
        requestManager = GetComponent<PathRequestManager>();
        grid = GetComponent<Grid>();
    }
   
    public void StartFindPath(Vector3 startPos, Vector3 targetPos) {
        StartCoroutine(FindPath(startPos, targetPos));
        
    }

	public Vector3[] quickCheck(Vector3 startPos, Vector3 targetPos) {
		//Raycast to quick-check path
		float dist = getEuclideanDistance(startPos, targetPos);
		Vector3 vect = new Vector3 (targetPos.x - startPos.x, targetPos.y - startPos.y, 0);
		//UnityEngine.Debug.DrawRay(startPos, vect, Color.red, 1);
		if (!Physics2D.Raycast (startPos, vect, dist, grid.unwalkableMask)) {
			Vector3[] waypoints = new Vector3[1];
			waypoints [0] = targetPos;
			return waypoints;
		}
		return null;
	}

    IEnumerator FindPath(Vector3 startPos, Vector3 targetPos) {
		//Using A* to find a path
        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);
		bool shortcut = false;
       	//UnityEngine.Debug.Log("startnode walkable =" + startNode.walkable + " endNode walkable = " + targetNode.walkable);
        if (startNode.walkable && targetNode.walkable) {
			//	UnityEngine.Debug.Log ("No straightline...performing A-star");
			Heap<Node> openSet = new Heap<Node> (grid.MaxSize);
			HashSet<Node> closedSet = new HashSet<Node> ();
			openSet.Add (startNode);

			while (openSet.Count > 0) {
				Node currentNode = openSet.RemoveFirst ();
				closedSet.Add (currentNode);

				if (currentNode == targetNode) {
					pathSuccess = true;
					break;
				}

				foreach (Node neighbour in grid.GetNeighbours(currentNode)) {
					if (!neighbour.walkable || closedSet.Contains (neighbour)) {
						continue;
					}

					int newMovementCostToNeighbour = currentNode.gCost + getDistance (currentNode, neighbour);
					if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains (neighbour)) {
						neighbour.gCost = newMovementCostToNeighbour;
						neighbour.hCost = getDistance (neighbour, targetNode);

						neighbour.parent = currentNode;

						if (!openSet.Contains (neighbour)) {
							openSet.Add (neighbour);
						} 
						/*
					else {
						openSet.UpdateItem (neighbour);
					}
					*/
					}
				}
			}
        }
        yield return null;

        if (pathSuccess) {
            waypoints = RetracePath(startNode, targetNode);
        }
        requestManager.FinishedProcessingPath(waypoints, pathSuccess);

    }

    Vector3[] RetracePath(Node startNode, Node endNode) {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

		while (currentNode != startNode) {
            path.Add(currentNode);
            currentNode = currentNode.parent;
            //UnityEngine.Debug.Log("gridX=" + currentNode.gridX + " gridY=" + currentNode.gridY + " transformPos=" + startNode.worldPosition + " nodeWorldPos=" + currentNode.worldPosition);
        }
        
        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);
        return waypoints;
    }

    Vector3[] SimplifyPath(List<Node> path) {
        List<Vector3> waypoints = new List<Vector3> ();
        Vector2 directionOld = Vector2.zero;

        for (int i = 1; i < path.Count; i++){
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            if (directionNew != directionOld) {
            	waypoints.Add(path[i-1].worldPosition);
            }
            directionOld = directionNew;
        }
        return waypoints.ToArray();
    }

    int getDistance(Node nodeA, Node nodeB) {
        int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (distX > distY) {
            return 14 * distY + 10 * (distX - distY);
        }
        return 14 * distX + 10 * (distY - distX);
    }

	public float getEuclideanDistance(Vector3 pos1, Vector3 pos2) {
		return Mathf.Sqrt (Mathf.Pow (pos2.x - pos1.x, 2) + Mathf.Pow (pos2.y - pos1.y, 2));
	}
}
