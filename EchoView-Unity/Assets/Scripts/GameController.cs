using System.Collections;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject webSocketClientManager;

    public GameObject demoScriptObject;
    
    private WebSocketClient _webSocketClient;

    private DemoScript _demoScript;

    private const int TIMEOUT = 5; 

    private Coroutine _currentCoroutine;

    void Start()
    {
        _webSocketClient = webSocketClientManager.GetComponent<WebSocketClient>();
        _demoScript = demoScriptObject.GetComponent<DemoScript>();
        _webSocketClient.StartGameMode();
        _demoScript.StartGameMode();
    }

    public void OnButtonPressed()
    {
        if (_currentCoroutine != null)
            return;

        _webSocketClient.SetGameModeGuessing(true);
        _demoScript.SetGameModeGuessing(true);
        _currentCoroutine = StartCoroutine(StopGameModeGuessing());
    }

    IEnumerator StopGameModeGuessing()
    {
        yield return new WaitForSeconds(TIMEOUT);
        _webSocketClient.SetGameModeGuessing(false);
        _demoScript.SetGameModeGuessing(false);
        _currentCoroutine = null;
    }
}
