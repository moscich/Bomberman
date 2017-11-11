using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SomethingMessage: MessageBase
{
	[SerializeField]
	public int someInt;
	public SomethingMessage(int some) {
		someInt = some;
	}
	public SomethingMessage() {
	}
}

public class MyNetworkManager : NetworkManager {
	
	public Button buttonComponent;
	public Text textComponent;
	private bool server = false;
	private NetworkConnection serverConn;
	private List<NetworkConnection> connList;
	// Use this for initialization
	void Start () {
		buttonComponent.onClick.AddListener (HandleClick);

//		client.RegisterHandler (MsgType.Command, MessageReceived);
	}

	public void HandleClick()
	{
		GameController ctrl = GameObject.Find("GameController").GetComponent<GameController>();
		ctrl.gogoyo ();
		buttonComponent.gameObject.SetActive(false);
		textComponent.gameObject.SetActive(false);

		for (short i = 0; i < connList.Count; i++) {
			NetworkConnection conn = connList [i];
			GameObject obj = Instantiate (playerPrefab, new Vector3 (0, 0, 0), Quaternion.identity);
			obj.name = "Player " + conn.connectionId;
			NetworkServer.SpawnWithClientAuthority (obj, conn);
			NetworkServer.AddPlayerForConnection (conn, obj, i);
		}
//		foreach (NetworkConnection conn in connList) {
//			
//		}

	}

	public void reset() {
		if (server) {
			buttonComponent.gameObject.SetActive(true);
			textComponent.gameObject.SetActive(true);
			textComponent.text = connList.Count + " Players";
		}
	}

	// Update is called once per frame
	void Update () {
		
	}

	public override void OnClientConnect (NetworkConnection conn)
	{
		base.OnClientConnect (conn);
		Debug.Log ("Client Connected " + conn.connectionId);
//		NetworkManager.singleton.client.RegisterHandler(432, MessageReceived);

		if (!server) { 
			serverConn = conn;
			Debug.Log ("register Bomb ");
			NetworkManager.singleton.client.RegisterHandler(666, Bomb);
//			conn.Send (666, new SomethingMessage ());
		}
//		NetworkServer.SendToClient (conn.connectionId, 666, new SomethingMessage (1));
//		NetworkManager.singleton.client.Send(432, new SomethingMessage (211));
	}

	public void sendBomb(int position) {
		if (server) {
			Debug.Log ("Send from server ");
			NetworkServer.SendToAll (666, new SomethingMessage (position));
		} else {
			Debug.Log ("Send to server ");
			serverConn.Send (666, new SomethingMessage (position));
		}
	}

	public override void OnServerReady (NetworkConnection conn)
	{
		base.OnServerReady (conn);
		if (NetworkManager.singleton.client.connection == conn) {
			Debug.Log ("They are equal!");
		}

		if (!server) {
			NetworkServer.RegisterHandler (666, BombToServer);
			NetworkServer.RegisterHandler (432, MessageReceived);


		}
//		if (conn.connectionId == 1) {
//			GameController ctrl = GameObject.Find("GameController").GetComponent<GameController>();
//			ctrl.gogoyo ();
//		}
		server = true;
		Debug.Log ("Serv Ready " + conn.connectionId);
//		NetworkClient client = NetworkManager.singleton.client;




//		conn.Send(666, new SomethingMessage (1));

	}

	public override void OnStartHost ()
	{
		base.OnStartHost ();
		buttonComponent.gameObject.SetActive(true);
		textComponent.gameObject.SetActive(true);
		connList = new List<NetworkConnection> ();
	}

	public override void OnStopHost ()
	{
		base.OnStopHost ();
	}

	public override void OnServerConnect (NetworkConnection conn)
	{
		base.OnServerConnect (conn);
//		if (NetworkManager.singleton.client.connection == conn) {
//			Debug.Log ("They are equal! Connect");
//		}
		connList.Add(conn);
		textComponent.text = connList.Count + " Players";
		Debug.Log ("ON serv Conn = " + conn.connectionId);
	}

	public override void OnServerDisconnect (NetworkConnection conn)
	{
		base.OnServerDisconnect (conn);
		Debug.Log ("S Disconnect = " + conn.connectionId);
		connList.Remove (conn);
		textComponent.text = connList.Count + " Players";
	}

	public override void OnClientDisconnect (NetworkConnection conn)
	{
		base.OnClientDisconnect (conn);
		Debug.Log ("Disconnect = " + conn.connectionId);
	}

	public void MessageReceived(NetworkMessage netMsg) {
		SomethingMessage hej = netMsg.ReadMessage<SomethingMessage>();
		Debug.Log ("COS POSZLO!!!" + hej.someInt);
		BoardSpawn board = GameObject.Find("Board").GetComponent<BoardSpawn>();
		board.bombFromNetwork (hej.someInt);
	}

	public void Bomb(NetworkMessage netMsg) {
		Debug.Log ("Connected! XDXDXD");
		SomethingMessage hej = netMsg.ReadMessage<SomethingMessage>();
		addBombOnBoard (hej.someInt);
	}

	public void addBombOnBoard(int position) {
		BoardSpawn board = GameObject.Find("Board").GetComponent<BoardSpawn>();
		board.bombFromNetwork (position);
	}

	public void BombToServer(NetworkMessage netMsg) {
		Debug.Log ("Sending to All 1 xD");
		SomethingMessage hej = netMsg.ReadMessage<SomethingMessage>();
		addBombOnBoard (hej.someInt);
		Debug.Log ("Sending to All xD");
		NetworkServer.SendToAll (666, new SomethingMessage(hej.someInt) );
	}

	public override void OnServerAddPlayer (NetworkConnection conn, short playerControllerId)
	{
		base.OnServerAddPlayer (conn, playerControllerId);
		Debug.Log ("Player Added " + conn.connectionId);
	}
}
