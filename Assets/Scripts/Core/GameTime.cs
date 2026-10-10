using System.Collections.Generic;
using UnityEngine;

// Time.timeScale'e dokunan herkes buradan geçer.
// Duraklatma, seviye atlama paneli, hitstop ve ağır çekim birbirini ezmesin diye
// istekler ayrı ayrı tutulur ve en son birlikte uygulanır.
public static class GameTime {

    private static readonly HashSet<object> pauseRequests = new HashSet<object>();
    private static bool isFrozen;
    private static float slowMotionScale = 1f;

    public static bool IsPaused => pauseRequests.Count > 0;

    public static void RequestPause(object owner) {
        pauseRequests.Add(owner);
        Apply();
    }

    public static void ReleasePause(object owner) {
        pauseRequests.Remove(owner);
        Apply();
    }

    // HitStop için çok kısa donma
    public static void SetFrozen(bool frozen) {
        isFrozen = frozen;
        Apply();
    }

    public static void SetSlowMotion(float scale) {
        slowMotionScale = Mathf.Clamp(scale, 0.05f, 1f);
        Apply();
    }

    public static void Reset() {
        pauseRequests.Clear();
        isFrozen = false;
        slowMotionScale = 1f;
        Apply();
    }

    private static void Apply() {
        if (IsPaused || isFrozen) {
            Time.timeScale = 0f;
        }
        else {
            Time.timeScale = slowMotionScale;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() {
        pauseRequests.Clear();
        isFrozen = false;
        slowMotionScale = 1f;
    }
}
