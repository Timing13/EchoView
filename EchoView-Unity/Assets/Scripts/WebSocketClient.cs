using System;
using UnityEngine;

using NativeWebSocket;

public class WebSocketClient : MonoBehaviour
{
	public GameObject particleGameObject;

    [SerializeField]
	private string IPAdress; //The IP Adress of the Server you want to connect to

    [SerializeField]
	private int Port; //The Port your WebSocket Connection will "talk" to

	private WebSocket _webSocket;

	private bool _connected;

	private ParticleSystem _particleSystem;

	void Start()
	{
		initWebSocket();
		_particleSystem = particleGameObject.GetComponent<ParticleSystem>();
		Debug.Log("Started");
	}

	void Update()
	{
		_webSocket.DispatchMessageQueue();
    }

	private async void OnApplicationQuit() //Closes Websocket Connection Correctly when app is closed
	{
		await _webSocket.Close();
	}
	
	private void initWebSocket() //Starts WebSocket Client Connection
	{
		Debug.Log($"ws://{IPAdress}:{Port}");
		_webSocket = new WebSocket($"ws://{IPAdress}:{Port}");
		_webSocket.Connect();

		_webSocket.OnOpen += WebSocket_OnOpen;
		_webSocket.OnError += WebSocket_OnError;
		_webSocket.OnClose += WebSocket_OnClose;
		_webSocket.OnMessage += WebSocket_OnMessage;

	}

	private void WebSocket_OnOpen() //Alerts on console when WebSocket Connection is Successfull
	{
		Debug.Log("Connecion opened!");
		string message = "Device name: " + SystemInfo.deviceName + " | Device Mac Address: " + SystemInfo.deviceUniqueIdentifier;
		SendWebSocketMessage(message);
	}

	private void WebSocket_OnError(string error) //Alerts on console when WebSocket Connection is Unsuccessfull
	{
		Debug.Log($"Error: {error}");
	}

	private void WebSocket_OnClose(WebSocketCloseCode closeCode) //Alerts on console when WebSocket Connection is Closed
	{
		Debug.Log("Connection closed!");
	}

	private void WebSocket_OnMessage(byte[] data) //Receives webSocket message and handles it respectively depending on what it contains
	{
		string socketMessage = System.Text.Encoding.UTF8.GetString(data);
		Debug.Log("Received message from server: ");
		Debug.Log(socketMessage);

		try
		{
			string splitMessage = socketMessage.Split(':')[1].Split("}")[0];
			Debug.Log(splitMessage);
			int potentiometerValue = Int32.Parse(splitMessage);
			Debug.Log(potentiometerValue);
			potentiometerValue -= 2500;
			_particleSystem.Emit(potentiometerValue % 100);
		}
		catch (Exception e)
		{
			Debug.Log(e.Message);
		}
	}

	public async void SendWebSocketMessage(string text) //Sends Websocket Message to Server for the ESP32 to receive
	{
		Debug.Log("Sending message to server: " + text);

		if (_webSocket.State == WebSocketState.Open)
		{
			// Sending plain text socket
			await _webSocket.SendText(text);
		}
	}
}
