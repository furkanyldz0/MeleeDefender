using System;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public event Action<float> OnSkillProgressChanged;

    public static UIManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI scoreText;

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

        UpdateScore(0);
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


    private void OnDestroy() {
        LevelManager.Instance.OnScoreChanged -= LevelManager_OnScoreChanged;
        Player.Instance.GetMelee().OnParried -= ChangeSkillProgress;
        Player.Instance.OnSkillUsed -= ChangeSkillProgress;
    }
}
