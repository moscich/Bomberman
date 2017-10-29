using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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

	private bool server = false;

	// Use this for initialization
	void Start () {
//		client.RegisterHandler (MsgType.Command, MessageReceived);
	}

	// Update is called once per frame
	void Update () {
		
	}

	public override void OnClientConnect (NetworkConnection conn)
	{
		base.OnClientConnect (conn);

		Debug.Log ("Client Connected " + conn.connectionId);
		NetworkManager.singleton.client.RegisterHandler(432, MessageReceived);
		if (!server) {
			conn.Send (666, new SomethingMessage ());
		}
//		NetworkServer.SendToClient (conn.connectionId, 666, new SomethingMessage (1));
//		NetworkManager.singleton.client.Send(432, new SomethingMessage (211));
	}

	public override void OnServerReady (NetworkConnection conn)
	{
		server = true;
		Debug.Log ("Serv Ready " + conn.connectionId);
		NetworkClient client = NetworkManager.singleton.client;
		client.RegisterHandler(666, Connected);
		base.OnServerReady (conn);
		NetworkServer.RegisterHandler (432, MessageReceived);
//		conn.Send(666, new SomethingMessage (1));

	}

	public void MessageReceived(NetworkMessage netMsg) {
		SomethingMessage hej = netMsg.ReadMessage<SomethingMessage>();
		Debug.Log ("COS POSZLO!!!" + hej.someInt);
		BoardSpawn board = GameObject.Find("Board").GetComponent<BoardSpawn>();
		board.addBomb (hej.someInt);
	}

	public void Connected(NetworkMessage netMsg) {
		Debug.Log ("Connected! XDXDXD");
	}

	public override void OnServerAddPlayer (NetworkConnection conn, short playerControllerId)
	{
		base.OnServerAddPlayer (conn, playerControllerId);
		Debug.Log ("Player Added " + conn.connectionId);
	}
}
