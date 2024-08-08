using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    void Start()
    {
    }

    void Update()
    {
        
    }

    public void StartTwoPlayerGame()
    {
        SceneManager.LoadScene("Party");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
