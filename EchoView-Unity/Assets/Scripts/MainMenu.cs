using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void EchoViewChosen()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void MinigameChosen()
    {
        //Could potentially lead to a second ui choice where users could do something like
        //customize difficulty or presence of certain simulation elements.

        //For now: 
        SceneManager.LoadScene("GameScene");
    }

    public void LoadMenuScene()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
