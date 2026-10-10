using System;
using System.Collections.Generic;
using UnityEngine;

public enum Language {
    Turkish = 0,
    English = 1
}

// Veri tanımlarında (silah, perk, boss adı vb.) iki dilli metin tutmak için
[Serializable]
public struct LocText {
    public string tr;
    public string en;

    public LocText(string tr, string en) {
        this.tr = tr;
        this.en = en;
    }

    public string Get() {
        return Loc.Current == Language.Turkish ? tr : en;
    }

    public override string ToString() => Get();
}

// Basit iki dilli metin tablosu. Yeni dil eklemek için tabloya sütun eklemek yeterli.
public static class Loc {

    public static event Action OnLanguageChanged;

    public static Language Current {
        get {
            int saved = SaveSystem.Data.language;
            if (saved >= 0) return (Language)saved;

            return Application.systemLanguage == SystemLanguage.Turkish ? Language.Turkish : Language.English;
        }
    }

    public static void SetLanguage(Language language) {
        SaveSystem.Data.language = (int)language;
        SaveSystem.Save();
        OnLanguageChanged?.Invoke();
    }

    public static string T(string key) {
        if (Table.TryGetValue(key, out string[] values)) {
            return values[(int)Current];
        }

        Debug.LogWarning("Çeviri bulunamadı: " + key);
        return key;
    }

    public static string Format(string key, params object[] args) {
        return string.Format(T(key), args);
    }

    private static readonly Dictionary<string, string[]> Table = new Dictionary<string, string[]> {
        // Genel
        { "game_title", new[] { "MELEE DEFENDER", "MELEE DEFENDER" } },
        { "tagline", new[] { "Savuştur. Yansıt. Kaleyi koru.", "Parry. Reflect. Hold the wall." } },
        { "back", new[] { "GERİ", "BACK" } },
        { "gold", new[] { "ALTIN", "GOLD" } },
        { "score", new[] { "SKOR", "SCORE" } },
        { "best", new[] { "EN İYİ", "BEST" } },
        { "level_short", new[] { "SV", "LV" } },
        { "level", new[] { "SEVİYE", "LEVEL" } },
        { "wave", new[] { "DALGA", "WAVE" } },
        { "base", new[] { "KALE", "CASTLE" } },
        { "weapon", new[] { "SİLAH", "WEAPON" } },

        // Ana menü
        { "play", new[] { "OYNA", "PLAY" } },
        { "armory", new[] { "CEPHANELİK", "ARMORY" } },
        { "settings", new[] { "AYARLAR", "SETTINGS" } },
        { "quit", new[] { "ÇIKIŞ", "QUIT" } },
        { "best_wave", new[] { "En iyi dalga", "Best wave" } },
        { "best_score", new[] { "En iyi skor", "Best score" } },
        { "total_kills", new[] { "Toplam öldürme", "Total kills" } },
        { "bosses_defeated", new[] { "Yenilen boss", "Bosses defeated" } },
        { "weapon_label", new[] { "Silah", "Weapon" } },
        { "controls_hint", new[] { "A/D: Hareket   Sol Tık: Savuştur   Shift: Atıl   Sağ Tık: Yetenek   ESC: Duraklat",
                                   "A/D: Move   Left Click: Parry   Shift: Dash   Right Click: Skill   ESC: Pause" } },

        // Skor tablosu
        { "leaderboard", new[] { "SKOR TABLOSU", "LEADERBOARD" } },
        { "your_name", new[] { "Adın", "Your name" } },
        { "save", new[] { "KAYDET", "SAVE" } },
        { "name_saved", new[] { "Ad kaydedildi", "Name saved" } },
        { "name_invalid", new[] { "Ad 2-16 karakter olmalı (harf, rakam, boşluk, - _)", "Name must be 2-16 characters (letters, digits, space, - _)" } },
        { "loading", new[] { "Yükleniyor...", "Loading..." } },
        { "lb_online", new[] { "Dünya sıralaması", "Global ranking" } },
        { "lb_not_configured", new[] { "Çevrimiçi skor tablosu henüz ayarlanmadı — bu cihazdaki skorlar gösteriliyor", "Online leaderboard is not set up yet — showing scores from this device" } },
        { "lb_offline", new[] { "Sunucuya bağlanılamadı — bu cihazdaki skorlar gösteriliyor", "Could not reach the server — showing scores from this device" } },
        { "lb_empty", new[] { "Henüz skor yok. İlk sen ol!", "No scores yet. Be the first!" } },
        { "col_name", new[] { "OYUNCU", "PLAYER" } },
        { "col_score", new[] { "SKOR", "SCORE" } },
        { "rank_world", new[] { "Dünya sıralaması: #{0}", "Global rank: #{0}" } },
        { "rank_local", new[] { "Bu cihazda: #{0}", "On this device: #{0}" } },
        { "rank_pending", new[] { "Skor gönderiliyor...", "Submitting score..." } },

        // Cephanelik
        { "tab_weapons", new[] { "SİLAHLAR", "WEAPONS" } },
        { "tab_upgrades", new[] { "GELİŞTİRMELER", "UPGRADES" } },
        { "buy", new[] { "SATIN AL", "BUY" } },
        { "equip", new[] { "KUŞAN", "EQUIP" } },
        { "equipped_btn", new[] { "KUŞANILDI", "EQUIPPED" } },
        { "maxed", new[] { "MAKS", "MAX" } },
        { "not_enough_gold", new[] { "Yeterli altın yok", "Not enough gold" } },
        { "stat_damage", new[] { "Hasar", "Damage" } },
        { "stat_speed", new[] { "Hız", "Speed" } },
        { "stat_reach", new[] { "Menzil", "Reach" } },
        { "armory_hint", new[] { "Altın, oyunda düşmanlardan ve bosslardan kazanılır.", "Gold is earned from enemies and bosses." } },
        { "special_split", new[] { "Ek mermi +{0}", "Extra shot +{0}" } },
        { "special_pierce", new[] { "Delme +{0}", "Pierce +{0}" } },
        { "special_crit", new[] { "Kritik şansı %{0}", "Crit chance {0}%" } },
        { "special_heavy", new[] { "Ağır vuruş", "Heavy hitter" } },
        { "special_balanced", new[] { "Dengeli", "Balanced" } },

        // Ayarlar
        { "master_volume", new[] { "Ana Ses", "Master Volume" } },
        { "sfx_volume", new[] { "Efekt Sesi", "SFX Volume" } },
        { "screen_shake", new[] { "Ekran Sarsıntısı", "Screen Shake" } },
        { "damage_numbers", new[] { "Hasar Sayıları", "Damage Numbers" } },
        { "language", new[] { "Dil", "Language" } },
        { "on", new[] { "AÇIK", "ON" } },
        { "off", new[] { "KAPALI", "OFF" } },
        { "reset_progress", new[] { "İLERLEMEYİ SIFIRLA", "RESET PROGRESS" } },
        { "reset_confirm", new[] { "EMİN MİSİN? (TEKRAR BAS)", "ARE YOU SURE? (PRESS AGAIN)" } },
        { "progress_reset", new[] { "İlerleme sıfırlandı", "Progress reset" } },

        // Oyun içi
        { "enemies_left", new[] { "Kalan düşman: {0}", "Enemies left: {0}" } },
        { "boss_wave", new[] { "BOSS DALGASI", "BOSS WAVE" } },
        { "wave_cleared", new[] { "DALGA TEMİZLENDİ", "WAVE CLEARED" } },
        { "wave_bonus", new[] { "+{0} altın", "+{0} gold" } },
        { "boss_incoming", new[] { "BOSS YAKLAŞIYOR", "BOSS INCOMING" } },
        { "boss_enraged", new[] { "ÖFKELENDİ!", "ENRAGED!" } },
        { "boss_defeated", new[] { "BOSS YENİLDİ!", "BOSS DEFEATED!" } },
        { "skill_ready", new[] { "YETENEK HAZIR  [SAĞ TIK]", "SKILL READY  [RIGHT CLICK]" } },
        { "crit", new[] { "KRİTİK", "CRIT" } },
        { "blocked", new[] { "BLOK", "BLOCKED" } },
        { "tutorial_shield", new[] { "Kalkanlı düşman! Kalkanı 2 vuruşla kır ya da mermiyi duvardan sektir.", "Shielded enemy! Break the shield in 2 hits or bank shots off the walls." } },
        { "tutorial_parry", new[] { "Mermiler gelirken SOL TIK ile savuştur — düşmana geri yansır!", "LEFT CLICK as bullets arrive to parry them back at the enemy!" } },

        // Duraklatma
        { "paused", new[] { "DURAKLATILDI", "PAUSED" } },
        { "resume", new[] { "DEVAM ET", "RESUME" } },
        { "main_menu", new[] { "ANA MENÜ", "MAIN MENU" } },
        { "restart", new[] { "YENİDEN BAŞLA", "RESTART" } },
        { "current_perks", new[] { "Güçlendirmeler", "Perks" } },
        { "no_perks", new[] { "Henüz güçlendirme yok", "No perks yet" } },

        // Seviye atlama
        { "level_up", new[] { "SEVİYE ATLADIN!", "LEVEL UP!" } },
        { "choose_perk", new[] { "Bir güçlendirme seç  (1 / 2 / 3)", "Choose a perk  (1 / 2 / 3)" } },
        { "rarity_common", new[] { "SIRADAN", "COMMON" } },
        { "rarity_rare", new[] { "NADİR", "RARE" } },
        { "rarity_epic", new[] { "EFSANEVİ", "EPIC" } },
        { "stack", new[] { "Seviye {0}/{1}", "Rank {0}/{1}" } },

        // Oyun sonu
        { "game_over", new[] { "KALE DÜŞTÜ", "THE CASTLE FELL" } },
        { "new_record", new[] { "YENİ REKOR!", "NEW RECORD!" } },
        { "play_again", new[] { "TEKRAR OYNA", "PLAY AGAIN" } },
        { "stat_wave", new[] { "Ulaşılan dalga", "Wave reached" } },
        { "stat_kills", new[] { "Öldürme", "Kills" } },
        { "stat_parries", new[] { "Savuşturma", "Parries" } },
        { "stat_gold", new[] { "Kazanılan altın", "Gold earned" } },
        { "stat_level", new[] { "Seviye", "Level" } },
        { "stat_bosses", new[] { "Yenilen boss", "Bosses defeated" } },
        { "stat_time", new[] { "Süre", "Time" } },
    };
}
