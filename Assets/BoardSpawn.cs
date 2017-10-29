using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BoardElement {
	solid,
	wall,
	empty
};

public class BoardSpawn : MonoBehaviour {

	public Transform brick;
	public Transform wall;

	public Dictionary<int, BoardElement> board;

	public int DoSomething() {
		return 42;
	}

	// Use this for initialization
	void Start () {
		board = new Dictionary<int, BoardElement> ();
		for (int row = 0; row < 11; row++) {
			for (int column = 0; column < 13; column++) {
				BoardElement element;
				if (row % 2 == 1 && column % 2 == 1) {
					element = BoardElement.solid;
				} else {
					element = BoardElement.wall;
				}
				board.Add (row * 13 + column, element);
			}
		}

		board[5 * 13 + 5] = BoardElement.wall;

		for (int row = 0; row < 11; row++) {
			for (int column = 0; column < 13; column++) {
				BoardElement element;
				board.TryGetValue (row * 13 + column, out element);
				Transform field = element == BoardElement.solid ? wall : brick;
				Transform obj = Instantiate (field, new Vector3 (column, row, this.transform.position.z), Quaternion.identity);
				obj.parent = this.transform;
			}
		}

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
