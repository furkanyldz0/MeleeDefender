using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public event Action<float> OnSkillProgressChanged;

    public static UIManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI scoreText;

    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI panelScoreText;
    [SerializeField] private TextMeshProUGUI panelBestScoreText;

    [SerializeField] private Base baseObject;

    private int bestScore;
    private Vector3 originalScoreTextPosition;

    private void Awake() {
        if(Instance != null) {
            Debug.LogError("Sahnede birden fazla UIManager var!");
        }
        Instance = this;
    }

    private void Start() {
        LevelManager.Instance.OnScoreChanged += LevelManager_OnScoreChanged;
        Player.Instance.GetMelee().OnParried += ChangeSkillProgress;
        //Player.Instance.OnSkillUsed += ChangeSkillProgress;
        baseObject.OnDied += BaseObject_OnDied;

        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        UpdateScore(0);
        //HideGameOverPanel();

        originalScoreTextPosition = scoreText.transform.localPosition;
    }

    private void BaseObject_OnDied() {
        EndGame();
    }

    private void ChangeSkillProgress(object sender, EventArgs e) {
        OnSkillProgressChanged?.Invoke(Player.Instance.CurrentSkillPoint / (float) Player.Instance.MaxSkillPoint);
    }

    private void LevelManager_OnScoreChanged(int newScore) {
        UpdateScore(newScore);

        AnimateScoreText();
    }

    private void UpdateScore(int score) {
        scoreText.SetText(score.ToString());
    }
    private void AnimateScoreText() {
        // Üst üste hýzlý skor kazanýlýrsa animasyonlar birikip kaymasýn diye önce aktif iþlemi bitir
        scoreText.transform.DOComplete();

        // Skoru Y ekseninde 15 birim yukarý zýplat (YOYO ile geri yerine döner)
        // SetEase(Ease.OutQuad) yukarý çýkarken yavaþlamasýný, aþaðý düþerken hýzlanmasýný saðlar
        scoreText.transform.DOLocalMoveY(originalScoreTextPosition.y + 15f, 0.1f)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.OutSine)
            .OnComplete(() => {
                // Her ihtimale karþý animasyon bitince orijinal konumuna kesin olarak oturt
                scoreText.transform.localPosition = originalScoreTextPosition;
            });
    }

    private void EndGame() {
        panelScoreText.text = scoreText.text;

        int score = GetScoreAsInt();
        if ((score > bestScore)) {
            bestScore = score;

            PlayerPrefs.SetInt("BestScore", bestScore);
            PlayerPrefs.Save();
        }
        panelBestScoreText.SetText(bestScore.ToString());

        ShowGameOverPanel();
    }

    private void ShowGameOverPanel() {
        gameOverPanel.SetActive(true);
    }

    private void HideGameOverPanel() {
        gameOverPanel.SetActive(false);
    }

    public void RestartGame() {
        StartCoroutine(DelayRestart(0.1f));
    }

    private IEnumerator DelayRestart(float duration) {
        yield return new WaitForSeconds(duration);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public int GetScoreAsInt() {
        // Trim() ile baþtaki ve sondaki görünmez boþluklarý temizliyoruz
        if (int.TryParse(scoreText.text.Trim(), out int sayi)) {
            return sayi;
        }

        Debug.LogWarning($"Metin sayýya çevrilemedi: '{scoreText.text}'");
        return 0; // Hata durumunda varsayýlan deðer
    }
    private void OnDestroy() {
        LevelManager.Instance.OnScoreChanged -= LevelManager_OnScoreChanged;
        Player.Instance.GetMelee().OnParried -= ChangeSkillProgress;
        //Player.Instance.OnSkillUsed -= ChangeSkillProgress;
        baseObject.OnDied -= BaseObject_OnDied;

        scoreText.transform.DOKill();
    }
}
