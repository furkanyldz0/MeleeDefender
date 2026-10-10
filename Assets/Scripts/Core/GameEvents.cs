using System;
using UnityEngine;

// Görsel/işitsel geri bildirim sistemlerinin (ses, hasar sayıları, ekran sarsıntısı)
// oyun koduna doğrudan bağlanmadan dinleyebileceği olaylar.
public static class GameEvents {

    public static event Action<Vector3, float, bool> OnDamageDealt; // konum, miktar, kritik mi
    public static event Action<Vector3> OnParry;
    public static event Action<Vector3, float> OnBaseDamaged;       // konum, miktar
    public static event Action<Vector3, int> OnGoldEarned;          // dünya konumu, miktar
    public static event Action<Vector3> OnBulletBounced;
    public static event Action<Vector3> OnShieldBlocked;

    public static void DamageDealt(Vector3 position, float amount, bool isCrit) => OnDamageDealt?.Invoke(position, amount, isCrit);
    public static void Parry(Vector3 position) => OnParry?.Invoke(position);
    public static void BaseDamaged(Vector3 position, float amount) => OnBaseDamaged?.Invoke(position, amount);
    public static void GoldEarned(Vector3 position, int amount) => OnGoldEarned?.Invoke(position, amount);
    public static void BulletBounced(Vector3 position) => OnBulletBounced?.Invoke(position);
    public static void ShieldBlocked(Vector3 position) => OnShieldBlocked?.Invoke(position);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() {
        OnDamageDealt = null;
        OnParry = null;
        OnBaseDamaged = null;
        OnGoldEarned = null;
        OnBulletBounced = null;
        OnShieldBlocked = null;
    }
}
