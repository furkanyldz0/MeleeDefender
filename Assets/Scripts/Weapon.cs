using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private Transform fireTransform;

    public void Shoot() {
        var bullet = Instantiate(bulletPrefab, fireTransform.position, Quaternion.LookRotation(fireTransform.forward));
    }

}
