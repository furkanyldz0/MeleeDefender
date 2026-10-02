using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SkillBarUI : MonoBehaviour
{
    [SerializeField] private Image barImage;
    [SerializeField] private ProjectileWaveSkill skill;

    [SerializeField] private ParticleSystem skillReadyEffect;
    [SerializeField] private ParticleSystem skillUsedEffect;

    private void Start() {
        UIManager.Instance.OnSkillProgressChanged += UIManager_OnSkillProgressChanged;
        skill.OnSkillProgressChanged += Skill_OnSkillProgressChanged;
        Player.Instance.OnSkillUsed += Player_OnSkillUsed;

        DisableEffects();
    }

    private void Player_OnSkillUsed(object sender, System.EventArgs e) {
        EnableSkillUsedEffect();
    }

    private void Skill_OnSkillProgressChanged(float progressNormalized) {
        ChangeProgress(progressNormalized);
    }

    private void UIManager_OnSkillProgressChanged(float progressNormalized) {
        ChangeProgress(progressNormalized);
    }

    private void ChangeProgress(float progressNormalized) {
        barImage.fillAmount = progressNormalized;

        if(progressNormalized == 1f) {
            EnableSkillReadyEffect();
            PlaySkillReadyAnimation();
        }
        else if(progressNormalized == 0f) {
            DisableEffects();
            PlaySkillEndedAnimation();
        }
    }

    private void PlaySkillReadyAnimation() {
        transform.DOKill();
        transform.DOScale(1.1f, 0.5f).SetEase(Ease.OutQuart);
    }

    private void PlaySkillEndedAnimation() {
        transform.DOKill();
        transform.DOScale(0.9f, 0.5f).SetEase(Ease.OutQuart);
    }

    private void EnableSkillReadyEffect() {
        skillReadyEffect.Play();
    }

    private void EnableSkillUsedEffect() {
        skillUsedEffect.Play();
    }

    private void DisableEffects() {
        skillReadyEffect.Stop();
        skillUsedEffect.Stop();
    }

    private void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

    private void OnDestroy() {
        UIManager.Instance.OnSkillProgressChanged -= UIManager_OnSkillProgressChanged;
        skill.OnSkillProgressChanged -= Skill_OnSkillProgressChanged;
        Player.Instance.OnSkillUsed -= Player_OnSkillUsed;
    }

}
