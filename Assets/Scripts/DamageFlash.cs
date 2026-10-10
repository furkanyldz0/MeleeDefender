using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageFlash : MonoBehaviour
{
    private static readonly int FlashColorId = Shader.PropertyToID("_FlashColor");
    private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");

    [ColorUsage(true,true)]
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashTime = 0.25f;
    [SerializeField] private AnimationCurve flashSpeedCurve;

    private readonly List<MeshRenderer> meshRenderers = new List<MeshRenderer>();

    // Her vuruşta yeni materyal kopyası oluşturmamak için MaterialPropertyBlock kullanılıyor
    private MaterialPropertyBlock propertyBlock;

    private Coroutine damageFlashCoroutine;

    private void Awake() {
        propertyBlock = new MaterialPropertyBlock();
        RefreshRenderers();
    }

    // Görseller çalışma anında değiştirildiğinde (EnemyVisual vb.) tekrar çağrılır
    public void RefreshRenderers() {
        meshRenderers.Clear();

        foreach (MeshRenderer meshRenderer in GetComponentsInChildren<MeshRenderer>(true)) {
            if (!meshRenderer.enabled) continue;
            if (meshRenderer.GetComponent<NoDamageFlash>() != null) continue;

            meshRenderers.Add(meshRenderer);
        }
    }

    public void CallDamageFlash() {
        if (!isActiveAndEnabled) return;

        if (damageFlashCoroutine != null) {
            StopCoroutine(damageFlashCoroutine);
        }
        damageFlashCoroutine = StartCoroutine(DamageFlasher());
    }

    private IEnumerator DamageFlasher() {
        float currentFlashAmount = 0f;
        float elapsedTime = 0f;
        while(elapsedTime < flashTime) {
            elapsedTime += Time.deltaTime;

            float curveValue = flashSpeedCurve != null && flashSpeedCurve.length > 0 ? flashSpeedCurve.Evaluate(elapsedTime) : 0f;
            currentFlashAmount = Mathf.Lerp(1f, curveValue, elapsedTime/flashTime);
            SetFlashAmount(currentFlashAmount);

            yield return null; //while'ın bir kare duraksamasını sağlar
        }

        SetFlashAmount(0f);
        damageFlashCoroutine = null;
    }

    private void SetFlashAmount(float amount) {
        foreach (MeshRenderer meshRenderer in meshRenderers) {
            if (meshRenderer == null) continue;

            meshRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(FlashColorId, flashColor);
            propertyBlock.SetFloat(FlashAmountId, amount);
            meshRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
