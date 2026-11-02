using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    public static PauseMenuUI Instance { get; private set; } //isgamepaused'a eriþmek için singleton yaptýk

    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject soundSettingsPanel;
    [SerializeField] private GameObject controlPanel;

    [SerializeField] private AudioClip buttonSoundClip;
    [SerializeField] private AudioClip buttonBackSoundClip;

    public bool IsGamePaused { get; private set; }

    // Aktif olan menülerin geçmiþini tutacaðýmýz yýðýn
    private Stack<GameObject> menuStack = new Stack<GameObject>();

    private void Awake() {
        if (Instance != null) {
            Debug.LogError("Sahnede birden fazla PauseMenuUI Instance'i var!");
            Destroy(gameObject); // Çakýþmayý önlemek için fazlalýðý yok et
            return;
        }
        Instance = this;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (IsGamePaused) {
                GoBack(); // Oyundayken Escape'e basýlýrsa bir önceki menüye dön veya oyuna dön
            }
            else {
                Pause(); // Oyun akýyorsa duraklat
            }
        }
    }

    private void Pause() {
        IsGamePaused = true;
        Time.timeScale = 0f;
        OpenMenu(pauseMenuPanel); // Ana menüyü aç
        PlaySound(buttonBackSoundClip);
    }

    private void Resume() {
        IsGamePaused = false;
        Time.timeScale = 1f;
        CloseAllMenus(); // Ekrandaki her þeyi temizle
        PlaySound(buttonSoundClip);
    }

    // HERHANGÝ BÝR MENÜYÜ AÇMAK ÝÇÝN BU METODU KULLAN
    private void OpenMenu(GameObject menuToOpen) {
        // Her menü açýlýþýnda buton üzerindeki görünmez fare/klavye odaðýný temizle
        if (EventSystem.current != null) {
            EventSystem.current.SetSelectedGameObject(null);
        }

        // Eðer halihazýrda açýk bir menü varsa, onu gizle
        if (menuStack.Count > 0) {
            menuStack.Peek().SetActive(false);
        }

        // Yeni menüyü aç ve yýðýna ekle
        menuToOpen.SetActive(true);
        menuStack.Push(menuToOpen);
    }

    // BÝR ÖNCEKÝ MENÜYE DÖNMEK ÝÇÝN BU METODU KULLAN
    private void GoBack() {
        // Her geri dönüþte buton üzerindeki görünmez fare/klavye odaðýný temizle
        if (EventSystem.current != null) {
            EventSystem.current.SetSelectedGameObject(null);
        }

        if (menuStack.Count > 0) {
            // Þu anki menüyü yýðýndan çýkar ve kapat
            GameObject currentMenu = menuStack.Pop();
            currentMenu.SetActive(false);
            PlaySound(buttonBackSoundClip);

            // Eðer yýðýnda hala menü kaldýysa (örneðin ayarlarý kapatýnca ana menüye dönmek)
            if (menuStack.Count > 0) {
                menuStack.Peek().SetActive(true);
            }
            // Yýðýnda menü kalmadýysa oyuna dön
            else {
                Resume();
            }
        }
    }

    // TÜM MENÜLERÝ KAPATMAK ÝÇÝN YARDIMCI METOT
    private void CloseAllMenus() {
        while (menuStack.Count > 0) {
            menuStack.Pop().SetActive(false);
        }
    }

    // --- UI BUTONLARINDAN ÇAÐRILACAK METOTLAR ---

    public void Btn_ShowSoundSettings() {
        OpenMenu(soundSettingsPanel);
        PlaySound(buttonSoundClip);
    }

    public void Btn_ShowControlPanel() {
        OpenMenu(controlPanel);
        PlaySound(buttonSoundClip);
    }

    public void Btn_Back() {
        GoBack();
    }

    public void Btn_Resume() {
        Resume();
    }

    public void Btn_ExitMainMenu() {
        PlaySound(buttonBackSoundClip);
        Time.timeScale = 1f; // Ana menüye dönerken zamaný düzeltmeyi unutmayýn
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    // Ses kodunu tekrar etmemek için yardýmcý metot
    private void PlaySound(AudioClip clip) {
        if (clip != null && SoundFXManager.Instance != null) {
            SoundFXManager.Instance.PlaySoundFXClip(clip, Camera.main.transform, 0.7f);
        }
    }
}


