using System.Collections;
using UnityEngine;

public class HitStop : MonoBehaviour
{
    public static HitStop Instance { get; private set; }

    private bool isWaiting;

    private void Awake() {
        if(Instance != null) {
            Debug.LogError("Sahnede birden fazla HitStop var!");
        }

        Instance = this;
    }

    public void StopTime(float duration) {
        if (isWaiting)
            return;

        Time.timeScale = 0.0f;
        StartCoroutine(Wait(duration));
    }

    private IEnumerator Wait(float duration) {
        isWaiting = true;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1.0f;
        isWaiting = false;
    }

}
