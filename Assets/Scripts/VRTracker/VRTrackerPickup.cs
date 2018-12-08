using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;

public class VRTrackerPickup : MonoBehaviour {

	List <GameObject> currentCollisions = new List <GameObject> ();
	List <GameObject> selectedObjectsUsingGravity = new List <GameObject> ();
	List <GameObject> selectedObjectsNotUsingGravity = new List <GameObject> ();

	public event Action OnGrabed;  // Called when Object is grabbed by the Wand
	public event Action OnReleased;    // Called when Object is released by the Wand

	public Vector3 positionOffset = new Vector3 (0.3f, 0f, 0f);

	// Use this for initialization
	void Start () {
		if (transform.parent.parent.GetComponent<NetworkIdentity>() != null && !transform.parent.parent.GetComponent<NetworkIdentity>().isLocalPlayer) {
			return;
		}
		this.transform.parent.GetComponent<VRTrackerTag> ().OnDown += OnButtonPressed;
		this.transform.parent.GetComponent<VRTrackerTag> ().OnUp += OnButtonReleased;
	}

	// Update is called once per frame
	void Update () {
		if (transform.parent.parent.GetComponent<NetworkIdentity>() != null && !transform.parent	.parent.GetComponent<NetworkIdentity>().isLocalPlayer) {
			return;
		}

		foreach (GameObject obj in selectedObjectsUsingGravity) {
			if (obj != null) {
				Vector3 pos = this.transform.parent.position + this.transform.parent.rotation * positionOffset;
				obj.transform.position = pos;
				obj.transform.rotation = this.transform.rotation;
			}
		}
		foreach (GameObject obj in selectedObjectsNotUsingGravity) {
			if (obj != null) {
				Vector3 pos = this.transform.parent.position + this.transform.parent.rotation * positionOffset;
				obj.transform.position = pos;
				obj.transform.rotation = this.transform.rotation;
			}
		}
	}

	void OnTriggerEnter (Collider col)
	{
		if (col.gameObject.name != "Body") {
			//Debug.Log ("Collision with " + col.gameObject.name);
			currentCollisions.Add (col.gameObject);
		}
	}

	void OnTriggerExit (Collider col) {
		// TODO: Check that object is not being moved, if so unassigned transform parent
		currentCollisions.Remove (col.gameObject);
	}

	private void OnButtonPressed(){
		//Debug.Log ("Button Pressed");
		foreach (GameObject obj in currentCollisions) {
			if (obj != null) {
				if (obj.GetComponent<Rigidbody> ().useGravity) {
					obj.GetComponent<Rigidbody> ().useGravity = false;
					selectedObjectsUsingGravity.Add (obj);
				} else
					selectedObjectsNotUsingGravity.Add (obj);

				obj.GetComponent<Rigidbody> ().useGravity = false;
			}
		}

		// Execute functions linked to this action
		if (OnGrabed != null)
			OnGrabed();
	}

	private void OnButtonReleased(){
		foreach (GameObject obj in selectedObjectsUsingGravity) {
			if (obj != null) {
				Debug.Log ("Set Gravity Back");
				obj.GetComponent<Rigidbody> ().useGravity = true;
			
			}

		}

		// Execute functions linked to this action
		if (OnReleased != null)
			OnReleased();

		selectedObjectsUsingGravity.Clear ();
		selectedObjectsNotUsingGravity.Clear ();
	}

}
