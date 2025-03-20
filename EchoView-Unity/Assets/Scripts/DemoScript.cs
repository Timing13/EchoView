using UnityEngine;
using UnityEngine.SceneManagement;

public class DemoScript : MonoBehaviour
{
    public GameObject leftLight;
    public GameObject rightLight;
    private GameObject _leftPlane;
    private GameObject _rightPlane;
    private UVScroller_C _leftScroller;
    private UVScroller_C _rightScroller;
    
    private const float BASE_SPEED = -1f;
    private const float FAST_SPEED = BASE_SPEED - 0.5f;
    private const float SLOW_SPEED = BASE_SPEED + 0.5f;

    private bool _demoStarted;
    private bool _directionUp;
    private bool _directionDown;
    private bool _gameMode;
    private bool _gameModeGuessing;
    
    // Start is called before the first frame update
    void Start()
    {
        _leftPlane = leftLight.transform.GetChild(0).gameObject;
        _rightPlane = rightLight.transform.GetChild(0).gameObject;
        _leftScroller = _leftPlane.GetComponent<UVScroller_C>();
        _rightScroller = _rightPlane.GetComponent<UVScroller_C>();
        _demoStarted = false;
        _directionUp = false;
        _directionDown = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_demoStarted)
            return;

        if (_gameMode && !_gameModeGuessing)
            return;

        var result = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick);
        
        if (result is { x: 0, y: 0 })
            return;

        float speed;
        Color color;
        if (result.y < -0.34)
        {
            speed = SLOW_SPEED;
            color = Color.green;
            // Debug.Log("Setting color green");
        }
        else if (result.y < 0.34)
        {
            speed = BASE_SPEED;
            color = Color.yellow;
            // Debug.Log("Setting color yellow");
        }
        else
        {
            speed = FAST_SPEED;
            color = Color.red;
            // Debug.Log("Setting color red");
        }

        if (_directionUp)
        {
            rightLight.SetActive(true);
            leftLight.SetActive(true);
            _leftScroller.MainoffsetY = SLOW_SPEED;
            _rightScroller.MainoffsetY = SLOW_SPEED;
            _leftPlane.GetComponent<Renderer>().material.color = Color.green;
            _rightPlane.GetComponent<Renderer>().material.color = Color.green;
        }
        else if (_directionDown)
        {
            rightLight.SetActive(true);
            leftLight.SetActive(true);
            _leftScroller.MainoffsetY = speed;
            _rightScroller.MainoffsetY = speed;
            _leftPlane.GetComponent<Renderer>().material.color = color;
            _rightPlane.GetComponent<Renderer>().material.color = color;
        }
        else if (result.x > 0)
        {
            rightLight.SetActive(true);
            leftLight.SetActive(false);
            _rightScroller.MainoffsetY = speed;
            _rightPlane.GetComponent<Renderer>().material.color = color;
        }
        else
        {
            rightLight.SetActive(false);
            leftLight.SetActive(true);
            _leftScroller.MainoffsetY = speed;
            _leftPlane.GetComponent<Renderer>().material.color = color;
        }
    }

    public void ToggleDemo()
    {
        _demoStarted = !_demoStarted;
    }

    public void SetDirectionUp(bool active)
    {
        _directionUp = active;
    }

    public void SetDirectionDown(bool active)
    {
        _directionDown = active;
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

    public void ChangeScene()
    {
        SceneManager.LoadScene("GameScene");
    }
}
