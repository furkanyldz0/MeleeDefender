using System.Collections;
using UnityEngine;

public class DamageFlashAlt : MonoBehaviour {
    private Renderer[] allRenderers;
    private MaterialPropertyBlock propBlock;

    // URP veya HDRP kullanýyorsanýz "_BaseColor", Built-in (Standart) kullanýyorsanýz "_Color" olmalýdýr.
    private static readonly int ColorPropertyID = Shader.PropertyToID("_BaseColor");

    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float flashTime = 0.25f;
    [SerializeField] private AnimationCurve flashSpeedCurve;

    private Coroutine activeFlashCoroutine;

    void Start() {
        // Ana obje ve tüm alt objelerindeki Renderer bileþenlerini (MeshRenderer/SkinnedMeshRenderer) listeye al
        allRenderers = GetComponentsInChildren<Renderer>();
        propBlock = new MaterialPropertyBlock();
    }

    // Hasar alýndýðýnda bu fonksiyonu çaðýrýn
    public void CallDamageFlash() {
        // Eðer halihazýrda çalýþan bir flash efekti varsa onu durdur
        if (activeFlashCoroutine != null) {
            StopCoroutine(activeFlashCoroutine);
        }

        // Yeni efekti baþlat ve referansýný kaydet
        activeFlashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine() {
        float elapsedTime = 0f;

        // flashTime süresi boyunca her kare (frame) çalýþacak döngü
        while (elapsedTime < flashTime) {
            elapsedTime += Time.deltaTime;

            // Zamaný 0 ile 1 arasýna normalize et (Curve'ün daha saðlýklý çalýþmasý için)
            float normalizedTime = elapsedTime / flashTime;

            // Curve'den o anki deðeri al (Genelde 1 tam hasar rengi, 0 ise orijinal renktir)
            float flashAmount = flashSpeedCurve.Evaluate(normalizedTime);

            // Orijinal renk (Beyaz) ile Hasar rengini Curve'den gelen deðere göre karýþtýr
            Color currentColor = Color.Lerp(Color.white, damageColor, flashAmount);

            // Hesaplanan rengi tüm mesh'lere uygula
            foreach (Renderer r in allRenderers) {
                r.GetPropertyBlock(propBlock);
                propBlock.SetColor(ColorPropertyID, currentColor);
                r.SetPropertyBlock(propBlock);
            }

            yield return null; // Bir sonraki kareyi bekle
        }

        // Döngü bittiðinde (süre dolduðunda) olasý ufak hesaplama sapmalarýna karþý 
        // tüm mesh'leri kesin olarak orijinal görünümlerine (Beyaz) döndür.
        foreach (Renderer r in allRenderers) {
            r.GetPropertyBlock(propBlock);
            propBlock.SetColor(ColorPropertyID, Color.white);
            r.SetPropertyBlock(propBlock);
        }
    }
}