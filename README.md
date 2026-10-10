# Melee Defender

Oyunun mevcut sürümü: https://furkanyldz.itch.io/melee-defender

## Kontroller

| Tuş | İşlev |
| --- | --- |
| A / D | Sola / sağa hareket |
| Sol tık | Savuştur (mermiyi düşmana geri yansıt) |
| Sağ tık | Yetenek (çubuk dolunca) |
| Shift | Atılma (dash) |
| 1 / 2 / 3 | Seviye atlayınca güçlendirme seç |
| ESC | Duraklat |

## Bu branch'te eklenenler

- **Dalgalar:** Her dalgada belirli sayıda düşman var. Her 5. dalgada boss geliyor.
- **3 boss:** Demir Muhafız, Girdap Kulesi, Kor Lordu. Canları yarıya inince öfkeleniyorlar.
- **Seviye ve güçlendirmeler:** Savuşturma ve öldürmeler XP veriyor. Seviye atlayınca 3 güçlendirmeden biri seçiliyor.
- **Altın ve Cephanelik:** Altın oyunlar arasında saklanıyor. Cephanelik'ten yeni silah ve kalıcı geliştirme alınıyor.
- **5 silah:** Uzun Kılıç, Katana, Savaş Baltası, Mızrak, Çifte Hançer.
- **Kalkanlı düşmanlar:** 3. dalgadan itibaren çıkıyorlar. Kalkan 2 vuruşta kırılıyor. Duvardan sektirilen veya delici mermiler kalkanı atlatıyor.
- **Skor tablosu:** Ana menüde. Oyun sonunda skor otomatik gönderiliyor.
- **Savuşturma kılıca göre:** Kılıcın bıçağının üzerinden geçtiği mermiler savuşturuluyor.
- **Yeni arayüz:** Ana menü, ayarlar (ses, ekran sarsıntısı, hasar sayıları, Türkçe / İngilizce), duraklatma ve oyun sonu ekranları.
- **Yeni görünüm:** Şövalye, düşman robotlar, kale, bosslar ve ses efektleri.

## Kurulum notları

- Unity sürümü: **6000.6.0f1**.
- Yeni sistemler sahneye oyun başında otomatik ekleniyor. Sahnede elle bir şey yapmak gerekmiyor.
- **Çevrimiçi skor tablosu (LootLocker):** Anahtarlar girilmezse oyun sadece o cihazdaki skorları gösterir. Açmak için:
  1. lootlocker.com'da ücretsiz hesap ve oyun oluştur.
  2. *Leaderboards* bölümünde **Player** tipinde bir leaderboard oluştur.
  3. *Create > Melee Defender > Game Database* ile `Assets/Resources/GameDatabase.asset` dosyasını oluştur.
  4. `Online` alanına Game API Key'i ve leaderboard key'i yaz.
- Test kısayolları (sadece editörde ve development build'de): F1 seviye atla, F2 sonraki boss, F3 +500 altın, F4 kaleyi onar, F5 düşmanları öldür.
