using UnityEngine;

// Kalkanın çarpışma kutusu. Mermiler IDamagable aradığı için kalkana çarpan mermiyi sahibine iletir.
// (Düşmanın kendi kökünde ikinci bir IDamagable olmasın diye ayrı objede duruyor.)
public class ShieldHitbox : MonoBehaviour, IDamagable {

    public EnemyShield Owner { get; set; }

    public float Health { get; set; } = 1f;

    public void Damage(float damageAmount) {
        if (Owner != null) {
            Owner.Block();
        }
    }
}
