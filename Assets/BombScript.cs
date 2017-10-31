using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombScript : MonoBehaviour {

	float elapsedTime = 0;

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
			BoardSpawn board = GameObject.Find("Board").GetComponent<BoardSpawn>();
			board.detonateBomb (transform, 2);
		}
	}
}
