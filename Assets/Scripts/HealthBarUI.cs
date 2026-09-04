using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image barImage;
    [SerializeField] private GameObject hasHealthBarGameObject;

    private IHasHealthBar hasHealthBar;

    private void Start() {
        hasHealthBar = hasHealthBarGameObject.GetComponent<IHasHealthBar>();

        if(hasHealthBar == null) {
            Debug.LogError(this + "IHasHealthBar interface'ini uygulamýyor!");
        }

        hasHealthBar.OnHealthChanged += HasHealthBar_OnHealthChanged;
    }

    private void HasHealthBar_OnHealthChanged(object sender, IHasHealthBar.OnHealthChangedEventArgs e) {
        barImage.fillAmount = e.currentHealthNormalized;
        if(e.currentHealthNormalized == 0f || e.currentHealthNormalized == 1f) {
            Hide();
        }
        else {
            Show();
        }
    }

    private void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

    private void OnDestroy() {
        hasHealthBar.OnHealthChanged -= HasHealthBar_OnHealthChanged;
    }
}
