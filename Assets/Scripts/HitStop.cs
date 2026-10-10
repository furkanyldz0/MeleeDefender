using System.Collections;
using UnityEngine;

public class HitStop : MonoBehaviour
{
    public static HitStop Instance { get; private set; }

    private bool isWaiting;
    private Coroutine slowMotionCoroutine;

    private void Awake() {
        if(Instance != null) {
            Debug.LogError("Sahnede birden fazla HitStop var!");
        }

        Instance = this;
    }

    public void StopTime(float duration) {
        // Duraklatma/seviye atlama ekranı açıkken zamanı geri açmamak için GameTime üzerinden yönetiliyor
        if (isWaiting || GameTime.IsPaused)
            return;

        GameTime.SetFrozen(true);
        StartCoroutine(Wait(duration));
    }

    // Boss ölümü gibi büyük anlar için kısa ağır çekim
    public void SlowMotion(float timeScale, float realDuration) {
        if (slowMotionCoroutine != null) StopCoroutine(slowMotionCoroutine);
        slowMotionCoroutine = StartCoroutine(SlowMotionRoutine(timeScale, realDuration));
    }

    private IEnumerator Wait(float duration) {
        isWaiting = true;
        yield return new WaitForSecondsRealtime(duration);
        GameTime.SetFrozen(false);
        isWaiting = false;
    }

    private IEnumerator SlowMotionRoutine(float timeScale, float realDuration) {
        GameTime.SetSlowMotion(timeScale);
        yield return new WaitForSecondsRealtime(realDuration);
        GameTime.SetSlowMotion(1f);
        slowMotionCoroutine = null;
    }

    private void OnDestroy() {
        GameTime.SetFrozen(false);
        GameTime.SetSlowMotion(1f);
    }

}
