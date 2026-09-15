using System;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI scoreText;

    private void Awake() {
        if(Instance != null) {
            Debug.LogError("Sahnede birden fazla UIManager var!");
        }
        Instance = this;
    }

    private void Start() {
        LevelManager.Instance.OnScoreChanged += Instance_OnScoreChanged;

        UpdateScore(0);
    }

    private void Instance_OnScoreChanged(int newScore) {
        UpdateScore(newScore);
    }

    private void UpdateScore(int score) {
        scoreText.SetText(score.ToString());
    }




    private void OnDestroy() {
        LevelManager.Instance.OnScoreChanged -= Instance_OnScoreChanged;
    }
}
