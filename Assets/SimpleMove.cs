using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SimpleMove : NetworkBehaviour {

	public BoardSpawn board;
	public Transform destinationDebug;

	void Start () {
		board = GameObject.Find("Board").GetComponent<BoardSpawn>();
//		destinationDebug = Instantiate (destinationDebug, new Vector3 (0, 0, this.transform.position.z), Quaternion.identity);
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
		if (destination.y > transform.position.y + 0.01f) {
			destinationFieldY = (int)Mathf.Floor (destination.y + 1);
		} else if (destination.y < transform.position.y - 0.01f) {
			destinationFieldY = (int)Mathf.Floor (destination.y);
		} else {
			destinationFieldY = (int)currentField.y;
		}

		int destinationFieldX;
		if (destination.x > transform.position.x + 0.01f) {
			destinationFieldX = (int)Mathf.Floor (destination.x + 1);
		} else if (destination.x < transform.position.x - 0.01f) {
			destinationFieldX = (int)Mathf.Floor (destination.x);
		} else {
			destinationFieldX = (int)currentField.x;
		}
			
		BoardElement destinationYElement = board.elementForPosition (new Vector2 (currentField.x, destinationFieldY));
		if (!destinationYElement.canPass() && destinationFieldY != (int)currentField.y) {
			if (destination.y > transform.position.y) {
				destination.y = Mathf.Min (destination.y, transform.position.y);
			} else {
				destination.y = Mathf.Max (destination.y, transform.position.y);
			}
		}
			
		BoardElement destinationXElement = board.elementForPosition (new Vector2 (destinationFieldX, currentField.y));
		if (!destinationXElement.canPass() && destinationFieldX != (int)currentField.x) {
			if (destination.x > transform.position.x) {
				destination.x = Mathf.Min (destination.x, transform.position.x);
			} else {
				destination.x = Mathf.Max (destination.x, transform.position.x);
			}
		}

		destination.x = Mathf.Max (destination.x, 0);
		destination.y = Mathf.Max (destination.y, 0);
		transform.position = destination;

		if (Input.GetButton ("Fire1")) {
			board.addBomb (currentField);
		}
	}
}
