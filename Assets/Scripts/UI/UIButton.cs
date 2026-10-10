using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Üzerine gelince büyüyen, tıklayınca ses çıkaran buton
public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {

    public Button Button { get; private set; }
    public Image Background { get; private set; }
    public TextMeshProUGUI Label { get; private set; }

    private Action onClick;
    private bool isHovered;
    private float hoverScale = 1.05f;

    public void Setup(Button button, Image background, TextMeshProUGUI label, Action clickAction) {
        Button = button;
        Background = background;
        Label = label;
        onClick = clickAction;

        Button.onClick.AddListener(HandleClick);
    }

    public UIButton SetHoverScale(float scale) {
        hoverScale = scale;
        return this;
    }

    public void SetLabel(string text) {
        Label.text = text;
    }

    public void SetColor(Color color) {
        Background.color = color;
        float luminance = color.r * 0.299f + color.g * 0.587f + color.b * 0.114f;
        Label.color = luminance > 0.6f ? UITheme.TextDark : UITheme.Text;
    }

    public void SetInteractable(bool interactable) {
        Button.interactable = interactable;
        if (!interactable) {
            isHovered = false;
            AnimateScale(1f);
        }
    }

    private void HandleClick() {
        if (AudioManager.Instance != null) AudioManager.Instance.Play(Sfx.UIClick, 1f, 0.02f);
        onClick?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (!Button.interactable) return;

        isHovered = true;
        AnimateScale(hoverScale);
        if (AudioManager.Instance != null) AudioManager.Instance.Play(Sfx.UIHover, 1f, 0.05f);
    }

    public void OnPointerExit(PointerEventData eventData) {
        isHovered = false;
        AnimateScale(1f);
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (!Button.interactable) return;
        AnimateScale(hoverScale * 0.94f);
    }

    public void OnPointerUp(PointerEventData eventData) {
        AnimateScale(isHovered ? hoverScale : 1f);
    }

    private void AnimateScale(float scale) {
        transform.DOKill();
        transform.DOScale(scale, 0.12f).SetEase(Ease.OutQuad).SetUpdate(true).SetLink(gameObject);
    }

    private void OnDisable() {
        isHovered = false;
        transform.DOKill();
        transform.localScale = Vector3.one;
    }
}
