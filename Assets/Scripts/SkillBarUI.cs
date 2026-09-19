using UnityEngine;
using UnityEngine.UI;

public class SkillBarUI : MonoBehaviour
{
    [SerializeField] private Image barImage;
    [SerializeField] private ProjectileWaveSkill skill;

    private void Start() {
        UIManager.Instance.OnSkillProgressChanged += UIManager_OnSkillProgressChanged;
        skill.OnSkillProgressChanged += Skill_OnSkillProgressChanged;
    }

    private void Skill_OnSkillProgressChanged(float progressNormalized) {
        ChangeProgress(progressNormalized);
    }

    private void UIManager_OnSkillProgressChanged(float progressNormalized) {
        ChangeProgress(progressNormalized);
    }

    private void ChangeProgress(float progressNormalized) {
        barImage.fillAmount = progressNormalized;
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
    }

}
