using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BombScript : NetworkBehaviour {

	float elapsedTime = 0;
	public int flameLength = 1;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (((int)elapsedTime) % 2 == 0) {
			float vec = 1 + elapsedTime - Mathf.Floor (elapsedTime);
			transform.localScale = new Vector3 (vec, vec, 1);
		} else {
			float vec = 2 - elapsedTime + Mathf.Floor (elapsedTime);
			transform.localScale = new Vector3 (vec, vec, 1);
		}
		elapsedTime += Time.deltaTime;

		if (elapsedTime > 3) {
			CmdDetonate ();
		}
	}

	[Command]
	void CmdDetonate() {
		Debug.Log ("Detonate yo xD");
		BoardSpawn board = GameObject.Find("Board(Clone)").GetComponent<BoardSpawn>();
		board.detonateBomb (transform, flameLength);
	}


}
