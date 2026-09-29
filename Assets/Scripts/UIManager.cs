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

    private void Awake() {
        if(Instance != null) {
            Debug.LogError("Sahnede birden fazla UIManager var!");
        }
        Instance = this;
    }

    private void Start() {
        LevelManager.Instance.OnScoreChanged += LevelManager_OnScoreChanged;
        Player.Instance.GetMelee().OnParried += ChangeSkillProgress;
        Player.Instance.OnSkillUsed += ChangeSkillProgress;
        baseObject.OnDied += BaseObject_OnDied;

        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        UpdateScore(0);
        HideGameOverPanel();
    }

    private void BaseObject_OnDied() {
        EndGame();
    }

    private void ChangeSkillProgress(object sender, EventArgs e) {
        OnSkillProgressChanged?.Invoke(Player.Instance.CurrentSkillPoint / (float) Player.Instance.MaxSkillPoint);
    }

    private void LevelManager_OnScoreChanged(int newScore) {
        UpdateScore(newScore);
    }

    private void UpdateScore(int score) {
        scoreText.SetText(score.ToString());
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
        Player.Instance.OnSkillUsed -= ChangeSkillProgress;
        baseObject.OnDied -= BaseObject_OnDied;
    }
}
