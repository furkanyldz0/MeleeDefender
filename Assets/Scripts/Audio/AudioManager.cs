using System;
using System.Collections.Generic;
using UnityEngine;

// Efekt seslerini çalar ve oyun olaylarına kendisi abone olur.
// Her sesin kendi AudioClip'i Inspector'dan atanabilir; atanmamışsa SfxSynth ile üretilir.
public class AudioManager : MonoBehaviour {

    public static AudioManager Instance { get; private set; }

    [Serializable]
    public class SfxOverride {
        public Sfx sfx;
        public AudioClip clip;
    }

    [SerializeField] private List<SfxOverride> overrides = new List<SfxOverride>();

    private const int VoiceCount = 12;
    private const float MinRepeatInterval = 0.035f; // aynı sesin üst üste binip patlamasını önler

    private readonly Dictionary<Sfx, AudioClip> clips = new Dictionary<Sfx, AudioClip>();
    private readonly Dictionary<Sfx, float> lastPlayTime = new Dictionary<Sfx, float>();
    private AudioSource[] voices;
    private int nextVoice;

    private void Awake() {
        if (Instance != null) {
            Debug.LogError("Sahnede birden fazla AudioManager var!");
        }
        Instance = this;

        voices = new AudioSource[VoiceCount];
        for (int i = 0; i < VoiceCount; i++) {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.ignoreListenerPause = true;
            voices[i] = source;
        }

        foreach (SfxOverride o in overrides) {
            if (o.clip != null) clips[o.sfx] = o.clip;
        }
        foreach (Sfx sfx in Enum.GetValues(typeof(Sfx))) {
            if (!clips.ContainsKey(sfx)) clips[sfx] = SfxSynth.Create(sfx);
        }
    }

    private void Start() {
        Player.Instance.GetMelee().OnSwing += Melee_OnSwing;
        Player.Instance.OnDash += Player_OnDash;
        Player.Instance.OnSkillUsed += Player_OnSkillUsed;
        GameEvents.OnParry += GameEvents_OnParry;
        GameEvents.OnDamageDealt += GameEvents_OnDamageDealt;
        GameEvents.OnBaseDamaged += GameEvents_OnBaseDamaged;
        GameEvents.OnGoldEarned += GameEvents_OnGoldEarned;
        GameEvents.OnShieldBlocked += GameEvents_OnShieldBlocked;
        Enemy.OnAnyEnemyDied += Enemy_OnAnyEnemyDied;
        Boss.OnAnyBossDefeated += Boss_OnAnyBossDefeated;
        RunProgression.Instance.OnPerkChoicesOffered += RunProgression_OnPerkChoicesOffered;
        WaveManager.Instance.OnWaveAnnounced += WaveManager_OnWaveAnnounced;
        WaveManager.Instance.OnWaveCleared += WaveManager_OnWaveCleared;
        WaveManager.Instance.OnBossSpawned += WaveManager_OnBossSpawned;
    }

    public void Play(Sfx sfx, float volume = 1f, float pitchVariation = 0.05f) {
        if (!clips.TryGetValue(sfx, out AudioClip clip) || clip == null) return;

        float now = Time.unscaledTime;
        if (lastPlayTime.TryGetValue(sfx, out float last) && now - last < MinRepeatInterval) return;
        lastPlayTime[sfx] = now;

        SaveData settings = SaveSystem.Data;
        AudioSource source = voices[nextVoice];
        nextVoice = (nextVoice + 1) % voices.Length;

        source.pitch = 1f + UnityEngine.Random.Range(-pitchVariation, pitchVariation);
        source.PlayOneShot(clip, volume * settings.masterVolume * settings.sfxVolume);
    }

    #region Olaylar

    private void Melee_OnSwing() => Play(Sfx.Swing, 0.6f, 0.12f);
    private void Player_OnDash() => Play(Sfx.Dash, 0.7f);
    private void Player_OnSkillUsed(object sender, EventArgs e) => Play(Sfx.SkillActivate);
    private void GameEvents_OnParry(Vector3 position) => Play(Sfx.Parry, 0.8f, 0.08f);
    private void GameEvents_OnBaseDamaged(Vector3 position, float amount) => Play(Sfx.BaseHit);
    private void GameEvents_OnGoldEarned(Vector3 position, int amount) => Play(Sfx.Coin, 0.5f, 0.1f);
    private void GameEvents_OnShieldBlocked(Vector3 position) => Play(Sfx.ShieldBlock, 0.8f, 0.08f);
    private void Enemy_OnAnyEnemyDied(object sender, Enemy.OnAnyEnemyDiedEventArgs e) => Play(Sfx.EnemyDeath, 0.8f, 0.1f);
    private void Boss_OnAnyBossDefeated(Boss boss) => Play(Sfx.Explosion, 1f, 0f);
    private void RunProgression_OnPerkChoicesOffered(int level, List<PerkDefinition> choices) => Play(Sfx.LevelUp, 0.8f, 0f);
    private void WaveManager_OnWaveCleared(int wave) => Play(Sfx.WaveCleared, 0.8f, 0f);

    private void GameEvents_OnDamageDealt(Vector3 position, float amount, bool isCrit) {
        Play(isCrit ? Sfx.Crit : Sfx.EnemyHit, isCrit ? 0.8f : 0.6f, 0.1f);
    }

    private void WaveManager_OnWaveAnnounced(int wave) {
        Play(WaveManager.Instance.IsBossWaveNumber(wave) ? Sfx.BossRoar : Sfx.WaveStart, 0.8f, 0f);
    }

    private void WaveManager_OnBossSpawned(Boss boss) {
        boss.OnTelegraph += () => Play(Sfx.BossCharge, 0.6f, 0.05f);
        boss.OnEnraged += () => Play(Sfx.BossRoar, 1f, 0f);
        boss.OnDeathStarted += () => Play(Sfx.Explosion, 0.9f, 0.1f);
    }

    #endregion

    private void OnDestroy() {
        GameEvents.OnParry -= GameEvents_OnParry;
        GameEvents.OnDamageDealt -= GameEvents_OnDamageDealt;
        GameEvents.OnBaseDamaged -= GameEvents_OnBaseDamaged;
        GameEvents.OnGoldEarned -= GameEvents_OnGoldEarned;
        GameEvents.OnShieldBlocked -= GameEvents_OnShieldBlocked;
        Enemy.OnAnyEnemyDied -= Enemy_OnAnyEnemyDied;
        Boss.OnAnyBossDefeated -= Boss_OnAnyBossDefeated;

        if (Player.Instance != null) {
            Player.Instance.GetMelee().OnSwing -= Melee_OnSwing;
            Player.Instance.OnDash -= Player_OnDash;
            Player.Instance.OnSkillUsed -= Player_OnSkillUsed;
        }
        if (RunProgression.Instance != null) {
            RunProgression.Instance.OnPerkChoicesOffered -= RunProgression_OnPerkChoicesOffered;
        }
        if (WaveManager.Instance != null) {
            WaveManager.Instance.OnWaveAnnounced -= WaveManager_OnWaveAnnounced;
            WaveManager.Instance.OnWaveCleared -= WaveManager_OnWaveCleared;
            WaveManager.Instance.OnBossSpawned -= WaveManager_OnBossSpawned;
        }
    }
}
