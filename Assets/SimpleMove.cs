using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SimpleMove : NetworkBehaviour {

	public BoardSpawn board;

	// Use this for initialization
	void Start () {
//		transform.position = new Vector3 (6, 5, 0);
		board = GameObject.Find("Board").GetComponent<BoardSpawn>();
	}
	
	float speed = 1.0f;

	void Update() {

		if (!isLocalPlayer)
		{
			return;
		}

		Vector2 currentField = new Vector2 (Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));

		Vector3 move = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
		Vector3 destination = transform.position + (move * speed * Time.deltaTime);

		int destinationFieldY;
		if (destination.y > transform.position.y) {
			destinationFieldY = (int)Mathf.Floor (destination.y + 1);
		} else {
			destinationFieldY = (int)Mathf.Floor (destination.y);
		}

		int destinationFieldX;
		if (destination.x > transform.position.x) {
			destinationFieldX = (int)Mathf.Floor (destination.x + 1);
		} else {
			destinationFieldX = (int)Mathf.Floor (destination.x);
		}
			
		BoardElement destinationYElement = board.elementForPosition (new Vector2 (currentField.x, destinationFieldY));
		if (!destinationYElement.canPass()) {
			if (destination.y > transform.position.y) {
				destination.y = Mathf.Min (destination.y, transform.position.y);
			} else {
				destination.y = Mathf.Max (destination.y, transform.position.y);
			}
		}
			
		BoardElement destinationXElement = board.elementForPosition (new Vector2 (destinationFieldX, currentField.y));
		if (!destinationXElement.canPass()) {
			if (destination.x > transform.position.x) {
				destination.x = Mathf.Min (destination.x, transform.position.x);
			} else {
				destination.x = Mathf.Max (destination.x, transform.position.x);
			}
		}

		destination.x = Mathf.Max (destination.x, 0);
		destination.y = Mathf.Max (destination.y, 0);
		transform.position = destination;
	}
}
