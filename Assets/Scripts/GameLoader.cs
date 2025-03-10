using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameLoader: MonoBehaviour
{
    public Button btnLoadGame;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        btnLoadGame.onClick.AddListener(LoadGame);
    }


    public void LoadGame() {
        SceneManager.LoadScene("mapnew", LoadSceneMode.Single);
    }
}
