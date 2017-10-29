﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum BoardElement {
	solid,
	wall,
	empty,
	bomb
};

static class BoardElementExtensions {
	public static bool canPass(this BoardElement value) {
		return value != BoardElement.solid && value != BoardElement.wall && value != BoardElement.bomb;
	}
}

public class BoardSpawn : MonoBehaviour {

	public static int BOARD_WIDTH = 13;
	public static int BOARD_HEIGHT = 11;

	public Transform brick;
	public Transform wall;
	public Transform startingPoint;
	public Transform bomb;

	public Dictionary<int, BoardElement> board;

	public int DoSomething() {
		return 42;
	}

	public BoardElement elementForPosition(Vector2 position) {
		BoardElement result;
		board.TryGetValue (BOARD_WIDTH * (int)position.y + (int)position.x, out result);
		return result;
	}

	public void addBomb(Vector2 position) {
		int boardIndex = (int)position.y * BOARD_WIDTH + (int)position.x;
		if (board [boardIndex] == BoardElement.empty) {
			NetworkServer.SendToAll (432, new SomethingMessage (boardIndex));
			NetworkManager.singleton.client.Send(432, new SomethingMessage (boardIndex));
//			NetworkServer.SendToClient (1, 432, new SomethingMessage (boardIndex));
			board [boardIndex] = BoardElement.bomb;
			Transform obj = Instantiate (bomb, new Vector3 (position.x, position.y, this.transform.position.z), Quaternion.identity);
			obj.parent = this.transform;
		}
	}

	public void addBomb(int index) {
		if (board [index] == BoardElement.empty) {
			board [index] = BoardElement.bomb;
			Transform obj = Instantiate (bomb, new Vector3 (index % BOARD_WIDTH, index / BOARD_WIDTH, this.transform.position.z), Quaternion.identity);
			obj.parent = this.transform;
		}
	}

	// Use this for initialization
	void Start () {
		board = new Dictionary<int, BoardElement> ();
		for (int row = 0; row < BOARD_HEIGHT; row++) {
			for (int column = 0; column < BOARD_WIDTH; column++) {
				BoardElement element;
				if (row % 2 == 1 && column % 2 == 1) {
					element = BoardElement.solid;
				} else {
					element = BoardElement.wall;
				}
				board.Add (row * BOARD_WIDTH + column, element);
			}
		}
			
		board[5 * BOARD_WIDTH + 5] = BoardElement.wall;

		setupStartingPoints ();

		for (int row = 0; row < BOARD_HEIGHT; row++) {
			for (int column = 0; column < BOARD_WIDTH; column++) {
				BoardElement element;
				board.TryGetValue (row * BOARD_WIDTH + column, out element);
				if (element != BoardElement.empty) {
					Transform field = element == BoardElement.solid ? wall : brick;
					Transform obj = Instantiate (field, new Vector3 (column, row, this.transform.position.z), Quaternion.identity);
					obj.parent = this.transform;
				}

			}
		}

	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void setupStartingPoints() {
		Vector2[] startingPositions = new Vector2[4] { new Vector2(0,0), new Vector2(12, 0), new Vector2(12, 10), new Vector2(0, 10) } ;
		foreach (Vector2 point in startingPositions) {
			makeRoomAtPoint (point);
			Transform obj = Instantiate (startingPoint, new Vector3 (point.x, point.y, this.transform.position.z - 1), Quaternion.identity);
			obj.parent = this.transform;
		}
	}

	private void makeRoomAtPoint(Vector2 point) {
		Vector2[] points = new Vector2[5] { new Vector2(0,0), new Vector2(0, -1), new Vector2(0, 1), new Vector2(1, 0), new Vector2(-1, 0) } ;
		foreach (Vector2 aroundPoint in points) {
			Vector2 clearPoint = point + aroundPoint;
			board[(int)clearPoint.y * BOARD_WIDTH + (int)clearPoint.x] = BoardElement.empty;
		}
	}
}
