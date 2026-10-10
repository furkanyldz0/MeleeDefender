using DG.Tweening;
using UnityEngine;

// Tam ekran arayüz sayfalarının (menü, ayarlar, oyun sonu...) temel sınıfı.
// Alt sınıflar Build() içinde içeriğini kodla kurar.
public abstract class UIScreen : MonoBehaviour {

    public bool IsVisible { get; private set; }

    protected RectTransform Root { get; private set; }
    protected CanvasGroup Group { get; private set; }
    protected UIManager Manager { get; private set; }

    public void Initialize(UIManager manager) {
        Manager = manager;
        Root = (RectTransform)transform;
        Root.Stretch();
        Group = UIFactory.Group(this);

        Build();

        IsVisible = false;
        Group.alpha = 0f;
        Group.interactable = false;
        Group.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    protected abstract void Build();

    protected virtual void OnShow() { }
    protected virtual void OnHide() { }

    public void Show() {
        gameObject.SetActive(true);
        IsVisible = true;

        Group.DOKill();
        Group.interactable = true;
        Group.blocksRaycasts = true;
        Group.DOFade(1f, 0.2f).SetUpdate(true).SetLink(gameObject);

        OnShow();
    }

    public void Hide() {
        if (!IsVisible) return;

        IsVisible = false;
        Group.DOKill();
        Group.interactable = false;
        Group.blocksRaycasts = false;
        Group.DOFade(0f, 0.15f).SetUpdate(true).SetLink(gameObject).OnComplete(() => gameObject.SetActive(false));

        OnHide();
    }

    // Overlay + ortalanmış içerik paneli oluşturan kısayol
    protected RectTransform CreateOverlay(Color overlayColor) {
        UnityEngine.UI.Image overlay = UIFactory.Image("Overlay", Root, overlayColor);
        overlay.rectTransform.Stretch();
        overlay.raycastTarget = true; // arkadaki oyuna tıklanmasın

        RectTransform content = UIFactory.Rect("Content", Root);
        content.Stretch();
        return content;
    }
}
