# Şehrin Paketçisi — Modernizasyon doğrulaması

## Teslim edilen sürüm
- SampleScene içinde kalıcı uGUI Canvas, TextMeshPro metinleri, New Input System EventSystem ve tek oyun giriş noktası.
- Yuvarlatılmış ve basış tepkili düğmeler; sade mod seçimi, koşullu günlük kartlar, harita, ayarlar, duraklatma ve sonuç panelleri.
- Özgün mahalle, paket maskotu, kurye ve renk + simge ile ayrılmış paket sprite varlıkları.
- İki oyun türünün kuralları ve kayıt deposu sunumdan ayrıldı. Eski OnGUI yolu kaldırıldı.
- Kurye: tutarlı şerit geçişi, mükemmel zamanlama penceresi, sabit eski hedefe paket uçuşu, son hedefin tekrarlanmaması.
- Bulmaca: kaydırılabilir 12 paketlik alan, seçim uçuşu, ipucu, geri alma, kombo ve sonuç geri bildirimi.
- Dokunmalı kısa öğretici; tutorial/duraklatma süresince tur durur.
- Eski PlayerPrefs anahtarları, paylaşılan günlük maske ve yıldız ilerlemesi korunur. Google reklam servisi ve paket kaynakları değiştirilmedi.

## Başarılı kontroller
- Tüm oluşturulan/değiştirilen C# dosyaları derlendi.
- `Tools > Paketci > Run Rule Checks`: 442 assertion geçti. Testler bellek içi kayıt deposu kullanır; oyuncunun ilerlemesini değiştirmez.
- Sırasız/repeated renk eşleştirme, joker, yanlış/geçersiz renk, deterministik tedarik, hedef tekrarı, şerit sınırları, erken/geç/mükemmel teslimat kuralları.
- İki oynanış türü × üç mod için model başlat/duraklat/devam/yeniden başlat/menü akışı.
- Süresiz hikâye bulmacası; ücretsiz/ücretli ipucu; bölüm tamamlanması; aynı bitişte tekrar ödül vermeme; günlük ödülü tekrarlamama; UTC gün yenilenmesi; eski revive callback koruması; tutorial süresinin durması.
- Runtime'da session oluşturulması, başlangıç menüsü, tek controller ve EventSystem, 12 kart, ScrollRect taşma alanı, paket seçiminin tek işlem olması, geri al, duraklatma raycast'i, ayarlardan molaya dönme, devam ve kurye görsel şerit konumu doğrulandı.
- Menü, bulmaca, kurye ve ayar görüntüleri incelendi. Gökyüzüne çıkan yol çizgileri, yuvarlatılmış tam ekran arka plan köşeleri ve düşük kontrastlı seçili mod açıklaması düzeltildi.
- İlk runtime testinde bulunan Unity `Start()` / oynama eylemi isim çakışması düzeltildi. Sonraki testlerde başlangıç menüsü doğrulandı.
- Sahne taramasında missing script yok; oyuna ait zorunlu referanslar bağlandı. UI'nin isteğe bağlı varsayılan materyal/navigation/font-fallback alanlarının null olması hata değildir.
- Android Development build: **Succeeded**, **0 errors**, IL2CPP + ARM64.
- APK: `Builds/PaketciDevelopment.apk`, 67,376,262 bytes.
- Inter font lisansı `Assets/UI/Fonts/Inter-LICENSE.txt` içinde.

## Sınırlamalar / henüz doğrulanmayanlar
- Çoklu ekran görüntüsü alan uzun Play Mode otomasyonu iki denemede timeout ile tamamlanamadı. Geçmiş assertion'lar yukarıda ayrı listelenmiştir; bütün seri başarılı sayılmamıştır. Aynı davranış için toplam üç runtime denemesinden sonra tekrar deneme durduruldu.
- Tablet, tüm ekran oranları ve çentik simülasyonu tam kabul matrisinden geçirilmedi. 390×780 ve 1080×2160 portre görüntüleri incelendi; eski farklı-oran görüntüleri nihai kabul kanıtı değildir.
- Fiziksel Android cihazda FPS, bellek/ısınma, reklam ödülü/iptali, titreşim ve uygulama arka plana geçiş testi yapılmadı. APK derleme başarısı cihaz doğrulaması değildir.
- Bu, ilk cilalı sürümdür: düğme basışları, paket uçuşu, kombo ve toast hareketi vardır; kapsamlı ekran geçiş animasyonları ve daha zengin bölüm sanatı sonraki cilalama aşamasına kalır.
- Build sırasında mevcut paketlerde shader/Google iOS plugin uyarıları görüldü; bunlar APK derlemesini engellemedi ve paket kaynakları değiştirilmedi.

## Temizlik
- Kullanıcının onayıyla testte geçici açılan Domain Reload, eski `DisableDomainReload` ayarına geri getirildi.
- Geçici otomatik Play Mode çalıştırıcısı silindi; editör Play Mode'da bırakılmadı.
- Testte seçilen oyun stili önceki paket eşleştirme tercihine geri getirildi; mevcut jeton/yıldız/bölüm ilerlemesi sıfırlanmadı.
- Önceki kaynakların yedeği: `PaketciBackups/20261102_161851/`.
