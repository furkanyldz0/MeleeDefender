using UnityEngine;

public class StaffVisual : MonoBehaviour
{
    private const string ATTACK = "Attack";
    private const string ATTACK_READY = "AttackReady";
    private const string ATTACK_COUNT = "AttackCount";

    [SerializeField] private Staff staff;
    private Animator staffAnimator;

    private void Start() {
        staffAnimator = GetComponent<Animator>();

        staff.OnAttackStarted += Staff_OnAttackStarted;
        staff.OnShotCountChanged += Staff_OnShotCountChanged;
    }

    private void Staff_OnAttackStarted(object sender, Staff.OnShotEventArgs e) {
        if (e.totalCount == 1) {
            staffAnimator.SetTrigger(ATTACK);
            return;
        }

        staffAnimator.SetInteger(ATTACK_COUNT, e.remainingCount);
        staffAnimator.SetTrigger(ATTACK_READY);
    }

    private void Staff_OnShotCountChanged(object sender, Staff.OnShotEventArgs e) {
        if (e.totalCount > 1) {
            staffAnimator.SetInteger(ATTACK_COUNT, e.remainingCount);
        }
    }

    private void OnDestroy() {
        staff.OnAttackStarted -= Staff_OnAttackStarted;
        staff.OnShotCountChanged -= Staff_OnShotCountChanged;
    }
}
