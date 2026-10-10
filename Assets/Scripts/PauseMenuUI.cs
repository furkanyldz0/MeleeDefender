using UnityEngine;

// Duraklatma artık GameFlow + UIManager (PauseView) tarafından yönetiliyor.
// Bu bileşen sahnedeki eski panelin butonları çalışmaya devam etsin diye duruyor.
public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;

    private void Start() {
        if (pauseMenuUI != null) {
            pauseMenuUI.SetActive(false);
        }
    }

    public void Resume() {
        if (GameFlow.Instance != null && GameFlow.Instance.State == GameState.Paused) {
            GameFlow.Instance.TogglePause();
        }
    }

    public void Pause() {
        if (GameFlow.Instance != null && GameFlow.Instance.State == GameState.Playing) {
            GameFlow.Instance.TogglePause();
        }
    }

}
