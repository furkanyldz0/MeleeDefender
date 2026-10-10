using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

// Oyun hissini güçlendiren efektler: ekran sarsıntısı ve patlama parçaları.
public class FeedbackFX : MonoBehaviour {

    public static FeedbackFX Instance { get; private set; }

    private const int MaxDebrisPieces = 120;

    private Transform cameraTransform;
    private Vector3 cameraRestPosition;
    private Tween shakeTween;
    private float currentShakeStrength;

    private readonly Queue<Transform> debrisPool = new Queue<Transform>();
    private int activeDebris;
    private Transform debrisRoot;

    private void Awake() {
        if (Instance != null) {
            Debug.LogError("Sahnede birden fazla FeedbackFX var!");
        }
        Instance = this;
    }

    private void Start() {
        cameraTransform = Camera.main.transform;
        cameraRestPosition = cameraTransform.position;

        debrisRoot = new GameObject("Debris").transform;

        Enemy.OnAnyEnemyDied += Enemy_OnAnyEnemyDied;
        GameEvents.OnBaseDamaged += GameEvents_OnBaseDamaged;
        GameEvents.OnParry += GameEvents_OnParry;
        Boss.OnAnyBossDefeated += Boss_OnAnyBossDefeated;
        Player.Instance.OnSkillUsed += Player_OnSkillUsed;
    }

    #region Ekran sarsıntısı

    public void Shake(float strength, float duration) {
        if (!SaveSystem.Data.screenShake || cameraTransform == null) return;

        // Daha güçlü bir sarsıntı devam ederken zayıf olanı yok say
        if (shakeTween != null && shakeTween.IsActive() && strength < currentShakeStrength) return;

        shakeTween?.Kill();
        cameraTransform.position = cameraRestPosition;
        currentShakeStrength = strength;

        shakeTween = cameraTransform.DOShakePosition(duration, strength, 22, 90f, false, true)
            .SetUpdate(true)
            .SetLink(cameraTransform.gameObject)
            .OnComplete(() => {
                cameraTransform.position = cameraRestPosition;
                currentShakeStrength = 0f;
            });
    }

    #endregion

    #region Parçalar

    public void Debris(Vector3 position, Color color, int count, float force, float size = 0.18f) {
        Material material = VisualFactory.Solid(color);

        for (int i = 0; i < count && activeDebris < MaxDebrisPieces; i++) {
            Transform piece = GetDebrisPiece();
            piece.GetComponent<MeshRenderer>().sharedMaterial = material;
            piece.position = position + Random.insideUnitSphere * 0.25f;
            piece.rotation = Random.rotation;
            piece.localScale = Vector3.one * size * Random.Range(0.6f, 1.3f);

            Vector2 circle = Random.insideUnitCircle.normalized * Random.Range(0.4f, 1f) * force;
            Vector3 landing = new Vector3(position.x + circle.x, 0.05f, position.z + circle.y);
            float duration = Random.Range(0.45f, 0.8f);

            activeDebris++;
            Sequence sequence = DOTween.Sequence().SetLink(piece.gameObject);
            sequence.Append(piece.DOJump(landing, Random.Range(0.6f, 1.6f) * Mathf.Sqrt(force), 1, duration).SetEase(Ease.Linear));
            sequence.Join(piece.DORotate(Random.insideUnitSphere * 720f, duration, RotateMode.FastBeyond360));
            sequence.AppendInterval(Random.Range(0.2f, 0.6f));
            sequence.Append(piece.DOScale(0f, 0.3f).SetEase(Ease.InBack));
            sequence.OnComplete(() => ReturnDebrisPiece(piece));
        }
    }

    private Transform GetDebrisPiece() {
        while (debrisPool.Count > 0) {
            Transform pooled = debrisPool.Dequeue();
            if (pooled != null) {
                pooled.gameObject.SetActive(true);
                return pooled;
            }
        }

        Transform piece = VisualFactory.Part(PrimitiveType.Cube, debrisRoot, Vector3.zero, Vector3.one, VisualFactory.Solid(Color.white));
        piece.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return piece;
    }

    private void ReturnDebrisPiece(Transform piece) {
        activeDebris--;
        if (piece == null) return;

        piece.gameObject.SetActive(false);
        debrisPool.Enqueue(piece);
    }

    #endregion

    #region Olaylar

    private void Enemy_OnAnyEnemyDied(object sender, Enemy.OnAnyEnemyDiedEventArgs e) {
        Color color = new Color(0.85f, 0.1f, 0.1f);
        if (sender is Enemy enemy) {
            Renderer firstRenderer = enemy.GetComponentInChildren<MeshRenderer>(false);
            Transform body = enemy.transform.Find("Mesh");
            color = VisualFactory.GetBaseColor(body != null ? body.GetComponent<Renderer>() : firstRenderer, color);
        }

        Vector3 center = e.position + Vector3.up * 0.5f;
        Debris(center, color, 10, 1.8f);
        Debris(center, new Color(0.22f, 0.24f, 0.3f), 5, 1.4f, 0.14f);
        Shake(0.12f, 0.18f);
    }

    private void GameEvents_OnBaseDamaged(Vector3 position, float amount) {
        Shake(0.25f, 0.25f);
    }

    private void GameEvents_OnParry(Vector3 position) {
        Shake(0.05f, 0.08f);
    }

    private void Player_OnSkillUsed(object sender, System.EventArgs e) {
        Shake(0.2f, 0.3f);
    }

    private void Boss_OnAnyBossDefeated(Boss boss) {
        Shake(0.7f, 0.9f);
    }

    #endregion

    private void OnDestroy() {
        Enemy.OnAnyEnemyDied -= Enemy_OnAnyEnemyDied;
        GameEvents.OnBaseDamaged -= GameEvents_OnBaseDamaged;
        GameEvents.OnParry -= GameEvents_OnParry;
        Boss.OnAnyBossDefeated -= Boss_OnAnyBossDefeated;

        if (Player.Instance != null) {
            Player.Instance.OnSkillUsed -= Player_OnSkillUsed;
        }

        shakeTween?.Kill();
        if (debrisRoot != null) Destroy(debrisRoot.gameObject);
    }
}
