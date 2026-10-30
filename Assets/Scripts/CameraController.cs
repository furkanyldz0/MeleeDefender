using DG.Tweening;
using UnityEngine;

public class CameraController : MonoBehaviour {
    public static CameraController Instance { get; private set; }

    [SerializeField] private float meleeHitShakeDuration = 0.1f;
    [SerializeField] private float meleeHitShakeStrength = 0.1f;

    private void Awake() {
        if (Instance != null) {
            Debug.LogError("Sahnede birden fazla CameraController var!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ShakeCamera() {
        Transform camTransform = Camera.main.transform;

        // Üst üste çok hýzlý çaðrýlýrsa sarsýntýlarýn birikip kamerayý bozmasýný engeller
        camTransform.DOComplete();

        // Parametreler: Süre, Güç, Titreþim Sayýsý, Rastgelelik
        camTransform.DOShakePosition(meleeHitShakeDuration, meleeHitShakeStrength, 15, 90f, false, true);
    }
}