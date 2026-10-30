using System;
using UnityEngine;

public class Staff : MonoBehaviour {
    public event EventHandler<OnShotEventArgs> OnAttackStarted;
    public event EventHandler<OnShotEventArgs> OnShotCountChanged;
    public event EventHandler OnAttackFinished;

    public class OnShotEventArgs : EventArgs {
        public int remainingCount; // Kalan mermi sayýsý
        public int totalCount;     // Serinin toplam mermi sayýsý
    }

    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private Transform fireTransform;

    [SerializeField] private AudioClip spellCastClip;

    private Vector3 bulletDirection;
    private float speedMultiplier = 1f;
    private int remainingCount;
    private int totalCount;

    public void PrepareToShoot(float speedMultiplier, Vector3 direction, int totalAttackCount) {
        this.speedMultiplier = speedMultiplier;
        bulletDirection = direction;
        totalCount = totalAttackCount;
        remainingCount = totalAttackCount;

        OnAttackStarted?.Invoke(this, new OnShotEventArgs {
            remainingCount = remainingCount,
            totalCount = totalCount
        });
    }

    // Animator'daki Animation Event tarafýndan çaðrýlýr
    public void Shoot() { //atýþ aralýðý ve atýþ sayýsý takibi, yönetimi artýk bu fonksiyona emanet
        if (remainingCount <= 0)
            return;

        var bullet = Instantiate(bulletPrefab, fireTransform.position, Quaternion.LookRotation(bulletDirection));
        bullet.Setup(speedMultiplier);

        SoundFXManager.Instance.PlaySoundFXClip(spellCastClip, transform, 0.5f);

        remainingCount--;

        OnShotCountChanged?.Invoke(this, new OnShotEventArgs {
            remainingCount = remainingCount,
            totalCount = totalCount
        });

        if (remainingCount <= 0) {
            OnAttackFinished?.Invoke(this, EventArgs.Empty);
        }
    }
}