using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum BoardElement {
	solid,
	wall,
	empty,
	bomb,
	fire
};

static class BoardElementExtensions {
	public static bool canPass(this BoardElement value) {
		return value != BoardElement.solid && value != BoardElement.wall && value != BoardElement.bomb;
	}
}

public class BoardSpawn : NetworkBehaviour {

	public static int BOARD_WIDTH = 13;
	public static int BOARD_HEIGHT = 11;

	public Transform brick;
	public Transform wall;
	public Transform startingPoint;
	public Transform bomb;
	public Transform alternateBomb;
	public Transform fire;
	public Transform fireGroup;
	public Transform fireWall;

	public Dictionary<int, BoardElement> board;
	public Dictionary<int, GameObject> gameObjects;

	public int DoSomething() {
		return 42;
	}

	public BoardElement elementForPosition(Vector2 position) {
		BoardElement result;
		bool found = board.TryGetValue (BOARD_WIDTH * (int)position.y + (int)position.x, out result);
		if (found == false) {
			return BoardElement.empty;
		}
		return result;
	}

	public void addBomb(Vector2 position) {
		int index = (int)position.y * BOARD_WIDTH + (int)position.x;
		if (board [index] == BoardElement.empty) {
			board [index] = BoardElement.bomb;
			Transform obj = Instantiate (alternateBomb, new Vector3 (index % BOARD_WIDTH, index / BOARD_WIDTH, this.transform.position.z), Quaternion.identity);
			NetworkServer.Spawn(obj.gameObject);
			obj.parent = this.transform;
		}
//		int boardIndex = (int)position.y * BOARD_WIDTH + (int)position.x;
//		if (board [boardIndex] == BoardElement.empty) {
//			MyNetworkManager networkManager = GameObject.Find("Network").GetComponent<MyNetworkManager>();
//			networkManager.sendBomb (boardIndex);
//			board [boardIndex] = BoardElement.bomb;
//			Transform obj = Instantiate (bomb, new Vector3 (position.x, position.y, this.transform.position.z), Quaternion.identity);
//			obj.parent = this.transform;
//		}
	}

	public void bombFromNetwork(int index) {
		if (board [index] == BoardElement.empty) {
			board [index] = BoardElement.bomb;
			Transform obj = Instantiate (bomb, new Vector3 (index % BOARD_WIDTH, index / BOARD_WIDTH, this.transform.position.z), Quaternion.identity);
			obj.parent = this.transform;
		}
	}

	public void addBomb(int index) {
		if (board [index] == BoardElement.empty) {
			board [index] = BoardElement.bomb;
			Transform obj = Instantiate (alternateBomb, new Vector3 (index % BOARD_WIDTH, index / BOARD_WIDTH, this.transform.position.z), Quaternion.identity);
			obj.parent = this.transform;
		}
//		if (board [index] == BoardElement.empty) {
//			board [index] = BoardElement.bomb;
//			Transform obj = Instantiate (bomb, new Vector3 (index % BOARD_WIDTH, index / BOARD_WIDTH, this.transform.position.z), Quaternion.identity);
//			obj.parent = this.transform;
//		}
	}

	public void detonateBomb(Transform bomb, int power) {
		board[(int)bomb.transform.position.y * BOARD_WIDTH + (int)bomb.transform.position.x] = BoardElement.empty;
		Destroy (bomb.gameObject);

		int row = (int)bomb.position.y;
		int column = (int)bomb.position.x;

		List<Vector3> list = new List<Vector3> ();
		list.Add (bomb.position);
//		list.Add(new Vector3 (1 + (int)bomb.position.x, bomb.position.y));
//		list.Add(new Vector3 (2 + (int)bomb.position.x, bomb.position.y));
//		list.Add(new Vector3 (-1 + (int)bomb.position.x, bomb.position.y));
//		list.Add(new Vector3 (-2 + (int)bomb.position.x, bomb.position.y));
//
//		list.Add(new Vector3 ((int)bomb.position.x, bomb.position.y -1));
//		list.Add(new Vector3 ((int)bomb.position.x, bomb.position.y -2));

		Transform parentObj = Instantiate (fireGroup, bomb.position, Quaternion.identity);
		parentObj.parent = this.transform;

		// up 

		int[] directionsX = {1,-1,0,0};
		int[] directionsY = {0,0,1,-1};

		for (int dir = 0; dir < 4; dir++) {
			int xdir = directionsX [dir];
			int ydir = directionsY [dir];
			for (int i = 0; i < power; i++) {
				BoardElement element;
				GameObject gameObj;
				int targetRow = row + (1 + i) * ydir;
				int targetColumn = column + (1 + i) * xdir;
				int index = targetRow * BOARD_WIDTH + targetColumn;
				board.TryGetValue (index, out element);
				if (element == BoardElement.wall) {
					gameObjects.TryGetValue (index, out gameObj);
					Destroy (gameObj);
					Transform obj = Instantiate (fireWall, new Vector3 (targetColumn, targetRow, this.transform.position.z), Quaternion.identity);
					obj.parent = this.transform;
					NetworkServer.Spawn (obj.gameObject);
					Destroy (obj.gameObject, 1);
					board [index] = BoardElement.empty;
					break;
				} else if (element == BoardElement.solid) {
					break;
				}
				list.Add (new Vector3 ((int)bomb.position.x + (1 + i) * xdir , bomb.position.y + (i + 1) * ydir));
			}
		}

		foreach (Vector3 position in list) {
			Transform obj = Instantiate (fire, position, Quaternion.identity);
			obj.parent = parentObj.transform;
			NetworkServer.Spawn(obj.gameObject);
		}
			
	}

	// Use this for initialization
	void Start () {
		board = new Dictionary<int, BoardElement> ();
		if (!isServer) {
			return;
			}
		Debug.Log ("Start");
//		if (isLocalPlayer) {return;}
		Debug.Log ("Setup");


		gameObjects = new Dictionary<int, GameObject> ();
		//		for (int row = 0; row < BOARD_HEIGHT; row++) {
		//			for (int column = 0; column < BOARD_WIDTH; column++) {
		//				board.Add (row * BOARD_WIDTH + column, BoardElement.empty);
		//			}
		//		}

		CmdSetupBoard ();
	}
		
	[Command]
	void CmdSetupBoard () {

		for (int row = 0; row < BOARD_HEIGHT; row++) {
			for (int column = 0; column < BOARD_WIDTH; column++) {
				BoardElement element;
				if (row % 2 == 1 && column % 2 == 1) {
					element = BoardElement.solid;
				} else {
					if (Random.Range (0, 2) == 1) {
						element = BoardElement.wall;
					} else {
						element = BoardElement.empty;
					}
				}
				board.Add (row * BOARD_WIDTH + column, element);
			}
		}

		Debug.Log ("Setup Board");


		board[5 * BOARD_WIDTH + 5] = BoardElement.wall;

		setupStartingPoints ();

		for (int row = 0; row < BOARD_HEIGHT; row++) {
			for (int column = 0; column < BOARD_WIDTH; column++) {
				BoardElement element;

				bool elementFound = board.TryGetValue (row * BOARD_WIDTH + column, out element);
				if (elementFound && element != BoardElement.empty) {
					Transform field = element == BoardElement.solid ? wall : brick;
					Transform obj = Instantiate (field, new Vector3 (column, row, this.transform.position.z), Quaternion.identity);
					obj.parent = this.transform;
					NetworkServer.Spawn (obj.gameObject);
					gameObjects.Add (row * BOARD_WIDTH + column, obj.gameObject);
				}

			} 
		}

		RpcDamage (42);

	}

	[ClientRpc]
	public void RpcDamage(int board)
	{
		if (isServer) {
			Debug.Log ("Server received");
		} else {
			Debug.Log ("Client received");
		}
		Debug.Log ("Received board yo!" + board);
//		this.board = board;
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
