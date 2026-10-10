using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// Arayüzde sık kullanılan kısa animasyonlar (duraklatmada da çalışsın diye SetUpdate(true))
public static class UITweens {

    public static Tween PopIn(Transform target, float from = 0.92f, float duration = 0.28f) {
        target.DOKill();
        target.localScale = Vector3.one * from;
        return target.DOScale(1f, duration).SetEase(Ease.OutBack).SetUpdate(true).SetLink(target.gameObject);
    }

    public static Tween Pulse(Transform target, float strength = 0.12f, float duration = 0.25f) {
        target.DOKill(true);
        return target.DOPunchScale(Vector3.one * strength, duration, 6, 0.6f).SetUpdate(true).SetLink(target.gameObject);
    }

    public static Tween Fade(Graphic graphic, float to, float duration) {
        graphic.DOKill();
        return graphic.DOFade(to, duration).SetUpdate(true).SetLink(graphic.gameObject);
    }
}
