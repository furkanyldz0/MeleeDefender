using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// Doluluk çubuğu: dolgu anında güncellenir, arkasındaki beyaz "iz" gecikmeli olarak takip eder
// (kaybedilen canın görünmesi için).
public class UIBar : MonoBehaviour {

    public Image Fill { get; private set; }

    private Image trail;
    private RectTransform fillRect;
    private RectTransform trailRect;
    private float value = 1f;

    public void Setup(Image fillImage, Image trailImage) {
        Fill = fillImage;
        trail = trailImage;
        fillRect = fillImage.rectTransform;
        trailRect = trailImage.rectTransform;

        trail.color = new Color(1f, 1f, 1f, 0.75f);
        SetValue(1f, true);
    }

    public void SetValue(float normalized, bool instant = false) {
        normalized = Mathf.Clamp01(normalized);
        bool decreased = normalized < value;
        value = normalized;

        fillRect.DOKill();
        if (instant || !gameObject.activeInHierarchy) {
            fillRect.anchorMax = new Vector2(value, 1f);
            trailRect.DOKill();
            trailRect.anchorMax = new Vector2(value, 1f);
            UpdateVisibility();
            return;
        }

        fillRect.DOAnchorMax(new Vector2(value, 1f), 0.15f).SetUpdate(true).SetLink(gameObject).OnUpdate(UpdateVisibility);

        trailRect.DOKill();
        if (decreased) {
            trailRect.DOAnchorMax(new Vector2(value, 1f), 0.4f).SetDelay(0.25f).SetEase(Ease.OutQuad).SetUpdate(true).SetLink(gameObject);
        }
        else {
            trailRect.anchorMax = new Vector2(value, 1f);
        }
    }

    public void SetColor(Color color) {
        Fill.color = color;
    }

    // Çok küçük değerlerde dilimlenmiş sprite bozulmasın diye gizle
    private void UpdateVisibility() {
        Fill.enabled = fillRect.anchorMax.x > 0.005f;
    }
}
