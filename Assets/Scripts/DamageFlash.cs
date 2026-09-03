using System.Collections;
using UnityEngine;

public class DamageFlash : MonoBehaviour
{
    [ColorUsage(true,true)]
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashTime = 0.25f;
    [SerializeField] private AnimationCurve flashSpeedCurve;

    private MeshRenderer[] meshRenderers;
    private Material[] materials;

    private Coroutine damageFlashCoroutine;

    private void Awake() {
        meshRenderers = GetComponentsInChildren<MeshRenderer>();

        Init();
    }

    private void Init() {
        materials = new Material[meshRenderers.Length];

        for(int i = 0; i < meshRenderers.Length; i++) {
            materials[i] = meshRenderers[i].material;
        }
    }

    public void CallDamageFlash() {
        damageFlashCoroutine = StartCoroutine(DamageFlasher());
    }

    private IEnumerator DamageFlasher() {
        SetFlashColor();

        float currentFlashAmount = 0f;
        float elapsedTime = 0f;
        while(elapsedTime < flashTime) {
            elapsedTime += Time.deltaTime;

            currentFlashAmount = Mathf.Lerp(1f, flashSpeedCurve.Evaluate(elapsedTime), elapsedTime/flashTime);
            SetFlashAmount(currentFlashAmount);

            yield return null; //while'ýn bir kare duraksamasýný saðlar
        }
    }

    private void SetFlashColor() {
        for (int i = 0; i < materials.Length; i++) {
            materials[i].SetColor("_FlashColor", flashColor);
        }
    }

    private void SetFlashAmount(float amount) {
        for (int i = 0; i < materials.Length; i++) {
            materials[i].SetFloat("_FlashAmount", amount);
        }
    }
}
