using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
    public static PauseMenuUI Instance { get; private set; } //isgamepaused'a eriþmek için singleton yaptýk

    [SerializeField] private GameObject pauseMenuUI;

    public bool IsGamePaused { get; private set; }

    private void Awake() {
        if(Instance != null) {
            Debug.LogError("Sahnede birden fazla PauseMenuUI Instance'i var!");
        }
        Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (IsGamePaused) {
                Resume();
            }
            else {
                Pause();
            }
        }
    }

    public void Resume() {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        IsGamePaused = false;
    }

    public void Pause() {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        IsGamePaused = true;
    }

}


