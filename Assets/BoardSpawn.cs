using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum BoardElement {
	solid,
	wall,
	empty,
	bomb,
	fire,
	bonus_bomb,
	bonus_flame
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
	public Transform bonus_flame;
	public Transform bonus_bomb;

	private bool gameEnded = false;

	public Dictionary<int, BoardElement> board;
	public Dictionary<int, GameObject> gameObjects;

	public int DoSomething() {
		return 42;
	}

	public BoardElement elementForPosition(Vector2 position) {
		BoardElement result;
		bool found = board.TryGetValue (BOARD_WIDTH * (int)position.y + (int)position.x, out result);
//		if (found == false) {
//			return BoardElement.empty;
//		}
		return result;
	}

	public GameObject addBomb(Vector2 position, SimpleMove player) {
		int index = (int)position.y * BOARD_WIDTH + (int)position.x;
		if (board [index] == BoardElement.empty) {
			board [index] = BoardElement.bomb;
			Transform obj = Instantiate (alternateBomb, new Vector3 (index % BOARD_WIDTH, index / BOARD_WIDTH, this.transform.position.z), Quaternion.identity);
			obj.GetComponent<BombScript> ().owner = player;
			NetworkServer.Spawn(obj.gameObject);
			obj.parent = this.transform;
			gameObjects.Add (index, obj.gameObject);
			return obj.gameObject;
		}
		return null;
	}

//	public void bombFromNetwork(int index) {
//		if (board [index] == BoardElement.empty) {
//			board [index] = BoardElement.bomb;
//			Transform obj = Instantiate (bomb, new Vector3 (index % BOARD_WIDTH, index / BOARD_WIDTH, this.transform.position.z), Quaternion.identity);
//			obj.parent = this.transform;
//		}
//	}

	public void detonateBomb(Transform bomb, int power) {
		board[(int)bomb.transform.position.y * BOARD_WIDTH + (int)bomb.transform.position.x] = BoardElement.empty;
		bomb.GetComponent<BombScript> ().detonate ();
		Destroy (bomb.gameObject);
		int bombIndex = (int)bomb.position.y * BOARD_WIDTH + (int)bomb.position.x;
		gameObjects.Remove (bombIndex);

		int row = (int)bomb.position.y;
		int column = (int)bomb.position.x;

		List<Vector3> list = new List<Vector3> ();
		list.Add (bomb.position);
		Transform parentObj = Instantiate (fireGroup, bomb.position, Quaternion.identity);
		parentObj.parent = this.transform;

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
					DestroyObject (gameObj);
					Transform bonus = Instantiate (bonus_bomb, new Vector3 (targetColumn, targetRow, this.transform.position.z + 1), Quaternion.identity);
					Transform obj = Instantiate (fireWall, new Vector3 (targetColumn, targetRow, this.transform.position.z), Quaternion.identity);
					obj.parent = this.transform;
					bonus.parent = this.transform;
					NetworkServer.Spawn (obj.gameObject);
					NetworkServer.Spawn (bonus.gameObject);
					gameObjects [index] = bonus.gameObject;
					DestroyObject (obj.gameObject, 1);
					BoardElement dropElement = BoardElement.bonus_bomb;
					board [index] = dropElement;
					RpcUpdateBoard (dropElement, index);
					break;
				} else if (element == BoardElement.solid) {
					break;
				} else if (element == BoardElement.bomb) {
					gameObjects.TryGetValue (index, out gameObj);
					BombScript bombObj = gameObj.GetComponent<BombScript> ();
					detonateBomb (gameObj.transform, bombObj.owner.flameLength);
				}
				list.Add (new Vector3 ((int)bomb.position.x + (1 + i) * xdir , bomb.position.y + (i + 1) * ydir));
			}
		}

		foreach (Vector3 position in list) {
			Transform obj = Instantiate (fire, position, Quaternion.identity);
			obj.tag = "Fire";
			obj.parent = parentObj.transform;
			NetworkServer.Spawn(obj.gameObject);
		}
			
	}

	// Use this for initialization
	void Start () {
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

		board = new Dictionary<int, BoardElement> ();
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
			
		RpcTransferBoard (boardString ());

	}

	[ClientRpc]
	public void RpcTransferBoard(string boardString)
	{
		if (isServer) {
			Debug.Log ("Server received");
		} else {
			board = setupBoardFromString (boardString);
			Debug.Log ("Client received");
		}
		Debug.Log ("Received board yo!" + boardString);
	}

	[ClientRpc]
	public void RpcUpdateBoard(BoardElement element, int index)
	{
		if (!isServer) {
			board [index] = element;
			Debug.Log ("Client received index = " + index);
 		}
		//		this.board = board;
	}

	// Update is called once per frame
	void Update () {
		if (isServer) {
			GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
			if (gameEnded) { return; }
			if (players.Length < 2) {
				gameEnded = true;
				Invoke("resolveGame", 2);
			}

			GameObject[] fires = GameObject.FindGameObjectsWithTag ("Fire");
			foreach (GameObject player in players) {
				foreach (GameObject fire in fires) {
					if (Mathf.RoundToInt(fire.transform.position.x) == Mathf.RoundToInt(player.transform.position.x) &&
						Mathf.RoundToInt(fire.transform.position.y) == Mathf.RoundToInt(player.transform.position.y)) {
						Debug.Log ("DIE !" + player.name);
						DestroyObject (player.gameObject);
					}
				}
			}
		}
	}

	void resolveGame() {
		GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
		Debug.Log("Player " + players[0].name + " wins yo!");
		DestroyObject (players [0].gameObject);
		DestroyObject (this.gameObject);
		MyNetworkManager networkManager = GameObject.Find("Network").GetComponent<MyNetworkManager>();
		networkManager.reset ();
	}

	[ClientRpc]
	public void RpcPlayerWon(string player) {
		if (isServer) {
		}
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

	private string boardString() {
		string result = "";
		for (int i = 0; i < BOARD_WIDTH * BOARD_HEIGHT; i++) {
			BoardElement element;
			board.TryGetValue (i, out element);
			result += element + "$";
		}
		return result;
	}

	private Dictionary<int, BoardElement> setupBoardFromString(string boardString) {

		Dictionary<int, BoardElement> board = new Dictionary<int, BoardElement> ();
		string[] elements = boardString.Split ('$');
		for (int i = 0; i < elements.Length; i++) {
			string elem = elements [i];
			if (elem == "wall") {
				board.Add (i, BoardElement.wall);
			} else if (elem == "solid") {
				board.Add (i, BoardElement.solid);
			} else if (elem == "empty") {
				board.Add (i, BoardElement.empty);
			}
		}
		return board;
	}

	public void omnomnom(Vector2 position, string player) {
		int index = (int)position.y * BOARD_WIDTH + (int)position.x;
		GameObject gameObj;
		if (gameObjects.TryGetValue (index, out gameObj)) {
			if (board [index] == BoardElement.bonus_flame) {
				GameObject.Find(player).GetComponent<SimpleMove>().RpcUpgradeFlame();
			} else if (board [index] == BoardElement.bonus_bomb) {
				GameObject.Find(player).GetComponent<SimpleMove>().RpcUpgradeBombs();
			}
			Debug.Log ("Jemy! = " + gameObj);
			gameObjects.Remove (index);
			board [index] = BoardElement.empty;
			DestroyObject (gameObj);
			RpcUpdateBoard (BoardElement.empty, index);

		}
	}
}
