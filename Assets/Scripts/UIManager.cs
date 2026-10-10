using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Yeni arayüzün kökü. Tüm ekranlar kodla, ayrı bir "GameUI" canvas'ında kurulur.
// Sahnedeki eski skor yazısı ve oyun sonu paneli artık kullanılmıyor, başlangıçta gizlenir.
public class UIManager : MonoBehaviour
{
    public event Action<float> OnSkillProgressChanged;

    public static UIManager Instance { get; private set; }

    [Header("Eski arayüz (gizlenir)")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject gameOverPanel;

    private const float GameOverDelay = 1.3f;

    private Canvas canvas;
    private CanvasGroup legacyCanvasGroup;

    private HUDView hud;
    private FloatingTextLayer floatingText;
    private BannerView banner;
    private LevelUpView levelUp;
    private PauseView pause;
    private GameOverView gameOver;
    private MainMenuView mainMenu;
    private ArmoryView armory;
    private SettingsView settings;
    private LeaderboardView leaderboard;

    private CanvasGroup toast;
    private TextMeshProUGUI toastText;
    private Sequence toastSequence;

    private UIScreen settingsReturnScreen;
    private Coroutine gameOverRoutine;

    private void Awake() {
        if(Instance != null) {
            Debug.LogError("Sahnede birden fazla UIManager var!");
        }
        Instance = this;
    }

    private void Start() {
        Player.Instance.GetMelee().OnParried += ChangeSkillProgress;
        PlayerStats.Instance.OnStatsChanged += PlayerStats_OnStatsChanged;
        GameFlow.Instance.OnStateChanged += GameFlow_OnStateChanged;
        GameFlow.Instance.OnRunStarted += GameFlow_OnRunStarted;
        RunProgression.Instance.OnPerkChoicesOffered += RunProgression_OnPerkChoicesOffered;
        Loc.OnLanguageChanged += Loc_OnLanguageChanged;

        HideLegacyUI();
        BuildUI();
        ApplyState(GameFlow.Instance.State);
        ChangeSkillProgress(this, EventArgs.Empty);
    }

    private void HideLegacyUI() {
        if (scoreText != null) {
            // Canvas'ı yazı gizlenmeden önce al (pasif objede bulunamaz)
            Canvas legacyCanvas = scoreText.canvas;
            if (legacyCanvas != null) {
                legacyCanvasGroup = UIFactory.Group(legacyCanvas);
            }
            scoreText.gameObject.SetActive(false);
        }
        if (gameOverPanel != null) {
            gameOverPanel.SetActive(false);
        }
    }

    #region Kurulum

    private void BuildUI() {
        GameObject canvasObject = new GameObject("GameUI", typeof(RectTransform));
        canvasObject.layer = LayerMask.NameToLayer("UI");

        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = UITheme.ReferenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
        Transform root = canvasObject.transform;

        // Sıralama önemli: sonra eklenen üstte çizilir
        hud = CreateScreen<HUDView>("HUD", root);
        floatingText = UIFactory.Rect("FloatingText", root).gameObject.AddComponent<FloatingTextLayer>();
        floatingText.Initialize(hud);
        banner = UIFactory.Rect("Banner", root).gameObject.AddComponent<BannerView>();
        banner.Initialize();
        levelUp = CreateScreen<LevelUpView>("LevelUp", root);
        pause = CreateScreen<PauseView>("Pause", root);
        gameOver = CreateScreen<GameOverView>("GameOver", root);
        mainMenu = CreateScreen<MainMenuView>("MainMenu", root);
        armory = CreateScreen<ArmoryView>("Armory", root);
        settings = CreateScreen<SettingsView>("Settings", root);
        leaderboard = CreateScreen<LeaderboardView>("Leaderboard", root);
        BuildToast(root);

        hud.Bind();
        floatingText.Bind();
        banner.Bind();
    }

    private T CreateScreen<T>(string name, Transform parent) where T : UIScreen {
        T screen = UIFactory.Rect(name, parent).gameObject.AddComponent<T>();
        screen.Initialize(this);
        return screen;
    }

    private void BuildToast(Transform root) {
        Image pill = UIFactory.BorderedPanel("Toast", root, UITheme.PanelLight, UITheme.PanelBorder, 2f, 0.85f);
        pill.rectTransform.Place(new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 110f), new Vector2(520f, 58f));
        toastText = UIFactory.Text("Text", pill.transform, "", 24f, UITheme.Text);
        toastText.rectTransform.Stretch();
        toast = UIFactory.Group(pill);
        toast.alpha = 0f;
        toast.blocksRaycasts = false;
    }

    // Dil değişince tüm metinler yeniden kurulur
    private void Loc_OnLanguageChanged() {
        bool settingsOpen = settings.IsVisible;
        UIScreen returnScreen = settingsReturnScreen;

        DOTween.Kill(canvas.gameObject);
        Destroy(canvas.gameObject);
        BuildUI();
        ApplyState(GameFlow.Instance.State);

        if (settingsOpen) {
            if (returnScreen is PauseView) {
                OpenSettings(pause);
            }
            else {
                OpenSettings(mainMenu);
            }
        }
    }

    #endregion

    #region Durumlar

    private void GameFlow_OnStateChanged(GameState state) {
        // İç içe durum değişikliklerinde (ör. seviye atlama ardı ardına) her zaman güncel durumu uygula
        ApplyState(GameFlow.Instance.State);
    }

    private void ApplyState(GameState state) {
        if (hud == null) return; // arayüz henüz kurulmadı

        bool inRun = state == GameState.Playing || state == GameState.LevelUp || state == GameState.Paused;
        SetVisible(hud, inRun);

        if (legacyCanvasGroup != null) {
            legacyCanvasGroup.alpha = inRun ? 1f : 0f;
        }

        SetVisible(levelUp, state == GameState.LevelUp && levelUp.IsVisible);
        SetVisible(pause, state == GameState.Paused);

        if (state != GameState.Paused && state != GameState.MainMenu) {
            settings.Hide();
        }
        if (state != GameState.MainMenu) {
            armory.Hide();
            leaderboard.Hide();
            mainMenu.Hide();
        }

        if (state == GameState.MainMenu && !GameFlow.Instance.IsAutoStarting && !armory.IsVisible && !settings.IsVisible && !leaderboard.IsVisible && !mainMenu.IsVisible) {
            mainMenu.Show();
        }

        if (state == GameState.GameOver) {
            if (gameOverRoutine != null) StopCoroutine(gameOverRoutine);
            gameOverRoutine = StartCoroutine(ShowGameOverDelayed());
        }
        else {
            gameOver.Hide();
        }
    }

    // Kalenin yıkılışı görünsün diye panel biraz gecikmeli açılır
    private IEnumerator ShowGameOverDelayed() {
        yield return new WaitForSecondsRealtime(GameOverDelay);
        gameOver.Show();
        gameOverRoutine = null;
    }

    private static void SetVisible(UIScreen screen, bool visible) {
        if (visible && !screen.IsVisible) screen.Show();
        else if (!visible && screen.IsVisible) screen.Hide();
    }

    private void GameFlow_OnRunStarted() {
        hud.SetGold(0, true);
        ChangeSkillProgress(this, EventArgs.Empty);
    }

    private void RunProgression_OnPerkChoicesOffered(int level, List<PerkDefinition> choices) {
        levelUp.ShowChoices(level, choices);
    }

    #endregion

    #region Gezinme

    private void Update() {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        if (settings.IsVisible) {
            CloseSettings();
        }
        else if (armory.IsVisible) {
            CloseArmory();
        }
        else if (leaderboard.IsVisible) {
            CloseLeaderboard();
        }
        else if (GameFlow.Instance.State == GameState.Playing || GameFlow.Instance.State == GameState.Paused) {
            GameFlow.Instance.TogglePause();
        }
    }

    public void OpenArmory() {
        mainMenu.Hide();
        armory.Show();
    }

    public void CloseArmory() {
        armory.Hide();
        if (GameFlow.Instance.State == GameState.MainMenu) {
            mainMenu.Show();
        }
    }

    public void OpenLeaderboard() {
        mainMenu.Hide();
        leaderboard.Show();
    }

    public void CloseLeaderboard() {
        leaderboard.Hide();
        if (GameFlow.Instance.State == GameState.MainMenu) {
            mainMenu.Show();
        }
    }

    public void OpenSettings(UIScreen returnTo) {
        settingsReturnScreen = returnTo;
        returnTo.Hide();
        settings.Show();
    }

    public void CloseSettings() {
        settings.Hide();

        if (GameFlow.Instance.State == GameState.Paused) {
            pause.Show();
        }
        else if (GameFlow.Instance.State == GameState.MainMenu) {
            mainMenu.Show();
        }
    }

    public void ShowToast(string message) {
        toastText.text = message;
        toastSequence?.Kill();

        toast.alpha = 0f;
        toast.transform.localScale = Vector3.one * 0.9f;
        toastSequence = DOTween.Sequence().SetUpdate(true).SetLink(toast.gameObject);
        toastSequence.Append(toast.DOFade(1f, 0.15f));
        toastSequence.Join(toast.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
        toastSequence.AppendInterval(1.4f);
        toastSequence.Append(toast.DOFade(0f, 0.3f));
    }

    #endregion

    #region Yetenek çubuğu (eski SkillBarUI bu olayı dinliyor)

    private void ChangeSkillProgress(object sender, EventArgs e) {
        float progress = Player.Instance.CurrentSkillPoint / (float) Player.Instance.MaxSkillPoint;
        OnSkillProgressChanged?.Invoke(Mathf.Clamp01(progress));
    }

    private void PlayerStats_OnStatsChanged() {
        // "Odak" perki gereken savuşturma sayısını düşürür, çubuğu güncelle
        ChangeSkillProgress(this, EventArgs.Empty);
    }

    #endregion

    // Sahnedeki eski Restart butonu hâlâ bu metodu çağırabilir
    public void RestartGame() {
        GameFlow.Instance.RestartRun();
    }

    private void OnDestroy() {
        Loc.OnLanguageChanged -= Loc_OnLanguageChanged;

        if (Player.Instance != null) Player.Instance.GetMelee().OnParried -= ChangeSkillProgress;
        if (PlayerStats.Instance != null) PlayerStats.Instance.OnStatsChanged -= PlayerStats_OnStatsChanged;
        if (GameFlow.Instance != null) {
            GameFlow.Instance.OnStateChanged -= GameFlow_OnStateChanged;
            GameFlow.Instance.OnRunStarted -= GameFlow_OnRunStarted;
        }
        if (RunProgression.Instance != null) RunProgression.Instance.OnPerkChoicesOffered -= RunProgression_OnPerkChoicesOffered;

        toastSequence?.Kill();
    }
}
