using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameLoader: MonoBehaviour
{
    [Header("bitch")]
    public Button btn_load_game;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        btn_load_game.onClick.AddListener(LoadGame);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadGame() {
        SceneManager.LoadScene("mapnew", LoadSceneMode.Single);
    }
}
