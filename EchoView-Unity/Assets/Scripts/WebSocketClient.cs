using System;
using UnityEngine;

using NativeWebSocket;

public class WebSocketClient : MonoBehaviour
{
	public GameObject leftLight;
    public GameObject rightLight;
    private GameObject _leftPlane;
    private GameObject _rightPlane;
    private UVScroller_C _leftScroller;
    private UVScroller_C _rightScroller;
    
    private const float BASE_SPEED = -1f;
    private const float FAST_SPEED = BASE_SPEED - 0.25f;
    private const float SLOW_SPEED = BASE_SPEED + 0.25f;
	private const float VERY_FAST_SPEED = BASE_SPEED - 0.5f;
    private const float VERY_SLOW_SPEED = BASE_SPEED + 0.5f;

	private float oldAmplitude = 0f;

    [SerializeField]
	private string IPAdress; //The IP Adress of the Server you want to connect to

    [SerializeField]
	private int Port; //The Port your WebSocket Connection will "talk" to

	private WebSocket _webSocket;

	private bool _connected;

	private bool _isDemoRunning;

	private bool _gameMode = false;
	private bool _gameModeGuessing;

	void Start()
	{
		initWebSocket();
		_leftPlane = leftLight.transform.GetChild(0).gameObject;
        _rightPlane = rightLight.transform.GetChild(0).gameObject;
        _leftScroller = _leftPlane.GetComponent<UVScroller_C>();
        _rightScroller = _rightPlane.GetComponent<UVScroller_C>();
        leftLight.SetActive(false);
        rightLight.SetActive(false);
        _isDemoRunning = false;
        _gameModeGuessing = false;
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
		Debug.Log("[WebSocketClient] Received message from server: ");
		Debug.Log("[WebSocketClient] " + socketMessage);

		if (_isDemoRunning)
			return;

		if (_gameMode && !_gameModeGuessing)
			return;

		try
		{
			string direction = socketMessage.Split('$')[0];
			string intensity = socketMessage.Split('$')[1];
			Debug.Log("[WebSocketClient] " + direction); Debug.Log("[WebSocketClient] " + intensity);
			int potentiometerValue = Int32.Parse(intensity);
			Debug.Log("[WebSocketClient] " + potentiometerValue);
			
			//Handler of direction feedback
			if (direction == "NORTH"){
				leftLight.SetActive(true);	
				rightLight.SetActive(true);	
				_leftPlane.GetComponent<Renderer>().material.color = Color.green;
				_rightPlane.GetComponent<Renderer>().material.color = Color.green;
			}
			else if (direction == "EAST") {
				leftLight.SetActive(false);	
				rightLight.SetActive(true);	
				_rightPlane.GetComponent<Renderer>().material.color = Color.yellow;
			} else if (direction == "WEST")
			{
				leftLight.SetActive(true);	
				rightLight.SetActive(false);	
				_leftPlane.GetComponent<Renderer>().material.color = Color.yellow;
			}
			else if (direction == "SOUTH"){
				leftLight.SetActive(true);	
				rightLight.SetActive(true);	
				_leftPlane.GetComponent<Renderer>().material.color = Color.red;
				_rightPlane.GetComponent<Renderer>().material.color = Color.red;
			}

			float hapticAmplitude = Convert.ToSingle(potentiometerValue) / 10;

			//Handler of intensity feedback
			if(potentiometerValue < 200){
				_leftScroller.MainoffsetY = VERY_SLOW_SPEED;
				_rightScroller.MainoffsetY = VERY_SLOW_SPEED;
			} else if(potentiometerValue < 400){
				_leftScroller.MainoffsetY = SLOW_SPEED;
				_rightScroller.MainoffsetY = SLOW_SPEED;
			} else if(potentiometerValue < 500){
				_leftScroller.MainoffsetY = BASE_SPEED;
				_rightScroller.MainoffsetY = BASE_SPEED;
			} else if(potentiometerValue < 600){
				_leftScroller.MainoffsetY = FAST_SPEED;
				_rightScroller.MainoffsetY = FAST_SPEED;
			} else if(potentiometerValue >= 600){
				_leftScroller.MainoffsetY = VERY_FAST_SPEED;
				_rightScroller.MainoffsetY = VERY_FAST_SPEED;
			}

			if(hapticAmplitude != oldAmplitude){
				oldAmplitude = hapticAmplitude;
				OVRInput.SetControllerVibration(hapticAmplitude, hapticAmplitude, OVRInput.Controller.RTouch);
				StartCoroutine(StopHapticFeedbackAfterDelay());
			}
		}
		catch (Exception e)
		{
			Debug.Log("[WebSocketClient] " + e.Message);
		}
	}

	private System.Collections.IEnumerator StopHapticFeedbackAfterDelay()
    {
        yield return new WaitForSeconds(1.0f);		//Haptic duration
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch); // Disattiva la vibrazione
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

	public void ToggleDemo()
	{
		_isDemoRunning = !_isDemoRunning;
	}

	public void StartGameMode()
	{
		_gameMode = true;
	}

	public void SetGameModeGuessing(bool status)
	{
		_gameModeGuessing = status;
		if (!status)
		{
			leftLight.SetActive(false);
			rightLight.SetActive(false);
		}
	}
}
