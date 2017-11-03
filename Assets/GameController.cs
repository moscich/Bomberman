using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameController : NetworkBehaviour {

	public Transform board;
	// Use this for initialization
	void Start () {
//		CmdBegin ();

	}

	public void gogoyo() {
		Debug.Log ("GO Go Yo");
		Transform obj = Instantiate (board, new Vector3 (0, 0, this.transform.position.z), Quaternion.identity);
		NetworkServer.Spawn (obj.gameObject);
	}

	[Command]
	void CmdBegin() {
		
	}

	// Update is called once per frame
	void Update () {
		
	}
}
