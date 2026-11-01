using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject controlPanel;

    [SerializeField] private AudioClip buttonSoundClip;
    [SerializeField] private AudioClip buttonBackSoundClip;

    private bool isMenuPanelActive;

    private void Start() {
        ShowMenuPanel();
    }

    private void Update() {
        if (!isMenuPanelActive) {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                ShowMenuPanel();
            }
        }
    }

    public void ShowMenuPanel() {
        menuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        controlPanel.SetActive(false);

        isMenuPanelActive = true;
        Debug.Log("Main Menu");

        SoundFXManager.Instance.PlaySoundFXClip(buttonBackSoundClip, Camera.main.transform, 0.7f);
    }

    public void ShowSettingsPanel() {
        menuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        controlPanel.SetActive(false);

        isMenuPanelActive = false;

        SoundFXManager.Instance.PlaySoundFXClip(buttonSoundClip, Camera.main.transform, 0.7f);
    }

    public void ShowControlPanel() {
        menuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        controlPanel.SetActive(true);

        isMenuPanelActive = false;

        SoundFXManager.Instance.PlaySoundFXClip(buttonSoundClip, Camera.main.transform, 0.7f);
    }

    public void StartGame() {
        SoundFXManager.Instance.PlaySoundFXClip(buttonSoundClip, Camera.main.transform, 0.7f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
