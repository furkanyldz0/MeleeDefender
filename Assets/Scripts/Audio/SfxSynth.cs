using System;
using UnityEngine;

public enum Sfx {
    Parry,
    Swing,
    EnemyHit,
    EnemyDeath,
    BaseHit,
    Coin,
    LevelUp,
    UIClick,
    UIHover,
    Dash,
    SkillActivate,
    WaveStart,
    WaveCleared,
    BossRoar,
    BossCharge,
    Explosion,
    Purchase,
    Error,
    Crit,
    ShieldBlock
}

// Ses dosyası olmadan, çalışma anında küçük retro efekt sesleri üretir.
// Gerçek ses dosyaları eklendiğinde AudioManager'a AudioClip atamak yeterli; bu sınıf sadece yedek.
public static class SfxSynth {

    private const int SampleRate = 44100;
    private static readonly System.Random Rng = new System.Random(1234);

    public static AudioClip Create(Sfx sfx) {
        switch (sfx) {
            case Sfx.Parry: return Build("Parry", 0.35f, Parry);
            case Sfx.Swing: return Build("Swing", 0.14f, Swing);
            case Sfx.EnemyHit: return Build("EnemyHit", 0.12f, EnemyHit);
            case Sfx.EnemyDeath: return Build("EnemyDeath", 0.45f, t => Explosion(t, 0.45f, 0.7f));
            case Sfx.BaseHit: return Build("BaseHit", 0.4f, BaseHit);
            case Sfx.Coin: return Build("Coin", 0.16f, Coin);
            case Sfx.LevelUp: return Build("LevelUp", 0.7f, LevelUp);
            case Sfx.UIClick: return Build("UIClick", 0.06f, t => Tone(t, 1150f, Square) * Env(t, 0.002f, 0.05f) * 0.25f);
            case Sfx.UIHover: return Build("UIHover", 0.03f, t => Tone(t, 2100f, Sine) * Env(t, 0.001f, 0.025f) * 0.12f);
            case Sfx.Dash: return Build("Dash", 0.2f, Dash);
            case Sfx.SkillActivate: return Build("SkillActivate", 0.6f, SkillActivate);
            case Sfx.WaveStart: return Build("WaveStart", 0.7f, WaveStart);
            case Sfx.WaveCleared: return Build("WaveCleared", 0.6f, WaveCleared);
            case Sfx.BossRoar: return Build("BossRoar", 1.3f, BossRoar);
            case Sfx.BossCharge: return Build("BossCharge", 0.35f, BossCharge);
            case Sfx.Explosion: return Build("Explosion", 0.9f, t => Explosion(t, 0.9f, 1f));
            case Sfx.Purchase: return Build("Purchase", 0.35f, Purchase);
            case Sfx.Error: return Build("Error", 0.18f, t => Tone(t, 140f, Square) * Env(t, 0.005f, 0.17f) * 0.25f);
            case Sfx.Crit: return Build("Crit", 0.3f, Crit);
            case Sfx.ShieldBlock: return Build("ShieldBlock", 0.25f, ShieldBlock);
            default: return Build("Silence", 0.05f, t => 0f);
        }
    }

    private static AudioClip Build(string name, float duration, Func<float, float> generator) {
        int sampleCount = Mathf.CeilToInt(duration * SampleRate);
        float[] data = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++) {
            float t = i / (float)SampleRate;
            data[i] = Mathf.Clamp(generator(t), -1f, 1f);
        }

        // Tıklama olmasın diye son 5 ms'yi sustur
        int fade = Mathf.Min(sampleCount, SampleRate / 200);
        for (int i = 0; i < fade; i++) {
            data[sampleCount - 1 - i] *= i / (float)fade;
        }

        AudioClip clip = AudioClip.Create("Sfx_" + name, sampleCount, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    #region Temel dalga biçimleri

    private static float Sine(float phase) => Mathf.Sin(phase * 2f * Mathf.PI);
    private static float Square(float phase) => (phase % 1f) < 0.5f ? 1f : -1f;
    private static float Saw(float phase) => 2f * (phase % 1f) - 1f;
    private static float Triangle(float phase) => 1f - 4f * Mathf.Abs((phase % 1f) - 0.5f);
    private static float Noise() => (float)(Rng.NextDouble() * 2.0 - 1.0);

    private static float Tone(float t, float frequency, Func<float, float> wave) => wave(t * frequency);

    // Frekansı zamanla değişen ton (faz integrali ile, kaymasız)
    private static float Sweep(float t, float from, float to, float duration, Func<float, float> wave) {
        float k = (to - from) / duration;
        float phase = from * t + 0.5f * k * t * t;
        return wave(phase);
    }

    // Hızlı yükselip üstel sönen zarf
    private static float Env(float t, float attack, float decay) {
        if (t < attack) return t / attack;
        return Mathf.Exp(-(t - attack) / Mathf.Max(0.0001f, decay));
    }

    #endregion

    #region Sesler

    private static float Parry(float t) {
        float ring = Tone(t, 1760f, Sine) * Env(t, 0.001f, 0.07f) * 0.45f
                   + Tone(t, 2640f, Sine) * Env(t, 0.001f, 0.04f) * 0.25f
                   + Tone(t, 3960f, Sine) * Env(t, 0.001f, 0.02f) * 0.15f
                   + Tone(t, 880f, Triangle) * Env(t, 0.001f, 0.12f) * 0.2f;
        float click = Noise() * Env(t, 0.0005f, 0.004f) * 0.5f;
        return ring + click;
    }

    private static float swingFilter;
    private static float Swing(float t) {
        if (t == 0f) swingFilter = 0f;
        float cutoff = Mathf.Lerp(0.5f, 0.08f, t / 0.14f);
        swingFilter += (Noise() - swingFilter) * cutoff;
        float env = Mathf.Sin(Mathf.Clamp01(t / 0.14f) * Mathf.PI);
        return swingFilter * env * 0.35f;
    }

    private static float EnemyHit(float t) {
        return Sweep(t, 320f, 120f, 0.12f, Square) * Env(t, 0.001f, 0.04f) * 0.2f
             + Noise() * Env(t, 0.001f, 0.02f) * 0.3f;
    }

    private static float explosionFilter;
    private static float Explosion(float t, float duration, float volume) {
        if (t == 0f) explosionFilter = 0f;
        float cutoff = Mathf.Lerp(0.6f, 0.03f, Mathf.Clamp01(t / duration));
        explosionFilter += (Noise() - explosionFilter) * cutoff;
        float thump = Sweep(t, 110f, 40f, duration, Sine) * Env(t, 0.002f, duration * 0.25f);
        return (explosionFilter * 0.8f * Env(t, 0.002f, duration * 0.3f) + thump * 0.6f) * volume;
    }

    private static float baseHitFilter;
    private static float BaseHit(float t) {
        if (t == 0f) baseHitFilter = 0f;
        baseHitFilter += (Noise() - baseHitFilter) * 0.08f;
        float thud = Sweep(t, 90f, 38f, 0.4f, Sine) * Env(t, 0.002f, 0.14f);
        return thud * 0.75f + baseHitFilter * Env(t, 0.001f, 0.08f) * 0.6f;
    }

    private static float Coin(float t) {
        float frequency = t < 0.05f ? 1318.5f : 1975.5f;
        float local = t < 0.05f ? t : t - 0.05f;
        return (Tone(t, frequency, Sine) * 0.7f + Tone(t, frequency * 2f, Sine) * 0.15f) * Env(local, 0.001f, 0.06f) * 0.3f;
    }

    private static float LevelUp(float t) {
        float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f };
        float step = 0.09f;
        int index = Mathf.Min(notes.Length - 1, Mathf.FloorToInt(t / step));
        float local = t - index * step;
        float decay = index == notes.Length - 1 ? 0.3f : 0.08f;
        float tone = Tone(t, notes[index], Triangle) * 0.6f + Tone(t, notes[index] * 2f, Sine) * 0.2f;
        return tone * Env(local, 0.003f, decay) * 0.35f;
    }

    private static float dashFilter;
    private static float Dash(float t) {
        if (t == 0f) dashFilter = 0f;
        float cutoff = Mathf.Lerp(0.05f, 0.45f, t / 0.2f);
        dashFilter += (Noise() - dashFilter) * cutoff;
        return dashFilter * Mathf.Sin(Mathf.Clamp01(t / 0.2f) * Mathf.PI) * 0.4f;
    }

    private static float SkillActivate(float t) {
        float sweep = Sweep(t, 180f, 900f, 0.6f, Saw) * 0.25f + Sweep(t, 360f, 1800f, 0.6f, Sine) * 0.2f;
        float shimmer = Tone(t, 2400f + 300f * Mathf.Sin(t * 40f), Sine) * 0.08f;
        return (sweep + shimmer) * Env(t, 0.05f, 0.35f);
    }

    private static float WaveStart(float t) {
        float frequency = t < 0.22f ? 392f : 523.25f;
        float local = t < 0.22f ? t : t - 0.22f;
        float horn = Tone(t, frequency, Saw) * 0.3f + Tone(t, frequency * 0.5f, Square) * 0.12f;
        return horn * Env(local, 0.02f, t < 0.22f ? 0.12f : 0.25f) * 0.5f;
    }

    private static float WaveCleared(float t) {
        float[] notes = { 659.25f, 783.99f, 987.77f };
        float step = 0.12f;
        int index = Mathf.Min(notes.Length - 1, Mathf.FloorToInt(t / step));
        float local = t - index * step;
        return Tone(t, notes[index], Triangle) * Env(local, 0.004f, index == notes.Length - 1 ? 0.3f : 0.1f) * 0.35f;
    }

    private static float roarFilter;
    private static float BossRoar(float t) {
        if (t == 0f) roarFilter = 0f;
        float vibrato = 1f + 0.06f * Mathf.Sin(t * 2f * Mathf.PI * 7f);
        float growl = Tone(t, 62f * vibrato, Saw) * 0.5f + Tone(t, 93f * vibrato, Saw) * 0.3f;
        roarFilter += (growl + Noise() * 0.3f - roarFilter) * 0.12f;
        float env = Mathf.Clamp01(t / 0.15f) * Mathf.Clamp01((1.3f - t) / 0.5f);
        return roarFilter * env * 0.9f;
    }

    private static float BossCharge(float t) {
        return Sweep(t, 300f, 900f, 0.35f, Triangle) * Mathf.Clamp01(t / 0.3f) * Env(Mathf.Max(0f, t - 0.3f), 0.001f, 0.03f) * 0.18f;
    }

    private static float Purchase(float t) {
        float first = Tone(t, 1567.98f, Sine) * Env(t, 0.001f, 0.05f);
        float second = t > 0.08f ? Tone(t, 2093f, Sine) * Env(t - 0.08f, 0.001f, 0.15f) : 0f;
        return (first + second) * 0.3f + Noise() * Env(t, 0.0005f, 0.01f) * 0.2f;
    }

    // Enerji kalkanına çarpma: tok bir metal "tung" + elektrik cızırtısı
    private static float ShieldBlock(float t) {
        float thunk = Sweep(t, 520f, 260f, 0.25f, Triangle) * Env(t, 0.001f, 0.06f) * 0.35f;
        float hum = Tone(t, 1240f, Sine) * Env(t, 0.001f, 0.09f) * 0.15f;
        float buzz = Noise() * Env(t, 0.001f, 0.02f) * 0.25f;
        return thunk + hum + buzz;
    }

    private static float Crit(float t) {
        return (Tone(t, 2349f, Sine) * Env(t, 0.001f, 0.06f) * 0.3f
              + Tone(t, 3136f, Sine) * Env(t, 0.001f, 0.1f) * 0.2f
              + Sweep(t, 600f, 200f, 0.3f, Square) * Env(t, 0.001f, 0.05f) * 0.1f);
    }

    #endregion
}
