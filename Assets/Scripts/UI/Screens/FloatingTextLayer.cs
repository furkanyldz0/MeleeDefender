using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Dünya üzerinde beliren hasar sayıları ve HUD'daki altın sayacına uçan sikkeler.
public class FloatingTextLayer : MonoBehaviour {

    private const int MaxCoinsPerDrop = 6;

    private RectTransform rect;
    private Camera mainCamera;
    private HUDView hud;
    private Material sharedTextMaterial;

    private readonly Queue<TextMeshProUGUI> textPool = new Queue<TextMeshProUGUI>();
    private readonly Queue<RectTransform> coinPool = new Queue<RectTransform>();

    public void Initialize(HUDView hudView) {
        rect = (RectTransform)transform;
        rect.Stretch();
        hud = hudView;
        mainCamera = Camera.main;
    }

    public void Bind() {
        GameEvents.OnDamageDealt += GameEvents_OnDamageDealt;
        GameEvents.OnGoldEarned += GameEvents_OnGoldEarned;
        GameEvents.OnShieldBlocked += GameEvents_OnShieldBlocked;
    }

    private void OnDestroy() {
        GameEvents.OnDamageDealt -= GameEvents_OnDamageDealt;
        GameEvents.OnGoldEarned -= GameEvents_OnGoldEarned;
        GameEvents.OnShieldBlocked -= GameEvents_OnShieldBlocked;
    }

    private bool TryGetLocalPoint(Vector3 worldPosition, out Vector2 localPoint) {
        localPoint = Vector2.zero;
        if (mainCamera == null) return false;

        Vector3 screen = mainCamera.WorldToScreenPoint(worldPosition);
        if (screen.z < 0f) return false;

        return RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screen, null, out localPoint);
    }

    #region Hasar sayıları

    private void GameEvents_OnDamageDealt(Vector3 position, float amount, bool isCrit) {
        if (!SaveSystem.Data.damageNumbers) return;
        if (!TryGetLocalPoint(position + Vector3.up * 0.8f, out Vector2 local)) return;

        TextMeshProUGUI text = GetText();
        RectTransform rt = text.rectTransform;

        int rounded = Mathf.RoundToInt(amount);
        if (isCrit) {
            text.text = rounded + "<size=60%>\n" + Loc.T("crit") + "</size>";
            text.color = UITheme.Gold;
            text.fontSize = 44f;
        }
        else {
            text.text = rounded.ToString();
            text.color = Color.white;
            text.fontSize = 32f;
        }

        rt.anchoredPosition = local + new Vector2(Random.Range(-24f, 24f), Random.Range(0f, 16f));
        rt.localScale = Vector3.one * (isCrit ? 1.8f : 1.35f);
        text.alpha = 1f;

        Sequence sequence = DOTween.Sequence().SetLink(text.gameObject);
        sequence.Append(rt.DOScale(1f, 0.18f).SetEase(Ease.OutBack));
        sequence.Join(rt.DOAnchorPosY(rt.anchoredPosition.y + (isCrit ? 90f : 64f), 0.7f).SetEase(Ease.OutCubic));
        sequence.Insert(0.4f, text.DOFade(0f, 0.3f));
        sequence.OnComplete(() => ReturnText(text));
    }

    private void GameEvents_OnShieldBlocked(Vector3 position) {
        if (!TryGetLocalPoint(position + Vector3.up * 0.6f, out Vector2 local)) return;

        TextMeshProUGUI text = GetText();
        RectTransform rt = text.rectTransform;
        text.text = Loc.T("blocked");
        text.color = UITheme.Cyan;
        text.fontSize = 26f;
        text.alpha = 1f;
        rt.anchoredPosition = local + new Vector2(Random.Range(-16f, 16f), 0f);
        rt.localScale = Vector3.one * 1.3f;

        Sequence sequence = DOTween.Sequence().SetLink(text.gameObject);
        sequence.Append(rt.DOScale(1f, 0.15f).SetEase(Ease.OutBack));
        sequence.Join(rt.DOAnchorPosY(rt.anchoredPosition.y + 40f, 0.5f).SetEase(Ease.OutCubic));
        sequence.Insert(0.3f, text.DOFade(0f, 0.25f));
        sequence.OnComplete(() => ReturnText(text));
    }

    private TextMeshProUGUI GetText() {
        while (textPool.Count > 0) {
            TextMeshProUGUI pooled = textPool.Dequeue();
            if (pooled != null) {
                pooled.gameObject.SetActive(true);
                return pooled;
            }
        }

        TextMeshProUGUI text = UIFactory.Text("DamageNumber", rect, "", 32f, Color.white);
        text.rectTransform.Center(Vector2.zero, new Vector2(240f, 100f));

        // Tüm sayılar aynı gölgeli materyali paylaşır (her biri için kopya oluşmasın)
        if (sharedTextMaterial == null && text.fontSharedMaterial != null) {
            sharedTextMaterial = new Material(text.fontSharedMaterial);
            sharedTextMaterial.EnableKeyword(ShaderUtilities.Keyword_Underlay);
            sharedTextMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(0f, 0f, 0f, 0.85f));
            sharedTextMaterial.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.8f);
            sharedTextMaterial.SetFloat(ShaderUtilities.ID_UnderlayDilate, 0.4f);
            sharedTextMaterial.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0.3f);
        }
        if (sharedTextMaterial != null) {
            text.fontSharedMaterial = sharedTextMaterial;
        }
        return text;
    }

    private void ReturnText(TextMeshProUGUI text) {
        if (text == null) return;
        text.gameObject.SetActive(false);
        textPool.Enqueue(text);
    }

    #endregion

    #region Altın

    private void GameEvents_OnGoldEarned(Vector3 position, int amount) {
        int coinCount = Mathf.Clamp(amount / 3 + 1, 1, MaxCoinsPerDrop);
        int totalGold = GameFlow.Instance.Stats.goldEarned;

        if (!TryGetLocalPoint(position + Vector3.up * 0.6f, out Vector2 start)) {
            hud.SetGold(totalGold);
            return;
        }

        Vector2 target = rect.InverseTransformPoint(hud.GoldTarget.position);
        target += new Vector2(-hud.GoldTarget.rect.width * 0.5f + 32f, -hud.GoldTarget.rect.height * 0.5f);

        for (int i = 0; i < coinCount; i++) {
            RectTransform coin = GetCoin();
            coin.anchoredPosition = start;
            coin.localScale = Vector3.zero;

            Vector2 burst = start + Random.insideUnitCircle * 70f + Vector2.up * 30f;
            float flyDuration = Random.Range(0.45f, 0.65f);
            bool isFirst = i == 0;

            Sequence sequence = DOTween.Sequence().SetLink(coin.gameObject).SetUpdate(true);
            sequence.Append(coin.DOScale(1f, 0.15f).SetEase(Ease.OutBack));
            sequence.Join(coin.DOAnchorPos(burst, 0.25f).SetEase(Ease.OutQuad));
            sequence.AppendInterval(i * 0.04f);
            sequence.Append(coin.DOAnchorPos(target, flyDuration).SetEase(Ease.InCubic));
            sequence.Join(coin.DOScale(0.6f, flyDuration).SetEase(Ease.InQuad));
            sequence.OnComplete(() => {
                ReturnCoin(coin);
                if (isFirst && hud != null) hud.SetGold(GameFlow.Instance.Stats.goldEarned);
            });
        }

        ShowGoldText(start, amount);
    }

    private void ShowGoldText(Vector2 position, int amount) {
        TextMeshProUGUI text = GetText();
        RectTransform rt = text.rectTransform;
        text.text = "+" + amount;
        text.color = UITheme.Gold;
        text.fontSize = 26f;
        text.alpha = 1f;
        rt.anchoredPosition = position + new Vector2(0f, -10f);
        rt.localScale = Vector3.one;

        Sequence sequence = DOTween.Sequence().SetLink(text.gameObject).SetUpdate(true);
        sequence.Append(rt.DOAnchorPosY(position.y + 40f, 0.8f).SetEase(Ease.OutCubic));
        sequence.Insert(0.5f, text.DOFade(0f, 0.3f));
        sequence.OnComplete(() => ReturnText(text));
    }

    private RectTransform GetCoin() {
        while (coinPool.Count > 0) {
            RectTransform pooled = coinPool.Dequeue();
            if (pooled != null) {
                pooled.gameObject.SetActive(true);
                return pooled;
            }
        }

        RectTransform coin = UIFactory.CoinIcon(rect, 24f);
        coin.Center(Vector2.zero, new Vector2(24f, 24f));
        return coin;
    }

    private void ReturnCoin(RectTransform coin) {
        if (coin == null) return;
        coin.gameObject.SetActive(false);
        coinPool.Enqueue(coin);
    }

    #endregion
}
