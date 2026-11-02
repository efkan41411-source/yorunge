# Paketçi — Son Teslimat

Unity 6 ile hazırlanmış, dikey ekrana yönelik, mahalle temalı bir paket eşleştirme oyunu.

## Çalıştırma

Projeyi Unity Hub ile açıp `Assets/Scenes/SampleScene.unity` sahnesini Play modunda çalıştırın. Oyun başlangıçta kendi arayüzünü ve seviyesini oluşturur; sahneye ayrıca nesne eklemek gerekmez.

## Oyun modları ve kontroller

- **Hikâye:** Süre sınırı olmadan bölümleri tamamlayın. Yeni renkler ve joker paket 3, 4, 8 ve 13. bölümlerde açılır.
- **Ekspres:** 75 saniyede olabildiğince çok sipariş teslim edin. Her üçlü sipariş dalgasından sonra yeni bir rota açılır. Doğru teslimat süre ekler, yanlış eşleşme süre götürür ve rekor kaydedilir.
- **Günlük:** Her gün açılan üç sabit rota: 90, 75 ve 60 saniye; renk sayısı ve tuzak paketleri artar. Her rotanın ödülü günde bir kez alınır. Rotaları istediğiniz zaman oynayabilirsiniz.
- Sipariş kartındaki renkleri eşleştiren üç paketi seçin. Joker paket herhangi bir rengi tamamlar.
- **GERİ AL** son seçimi çıkarır. İlk ipucu ücretsiz, sonraki ipuçları 5 jetondur.
- Ana ekrandan bölüm haritasını ve **AYARLAR**'ı açın. Oyun sırasında **Ⅱ** ile duraklatın; buradan ayarlara, yeniden başlatmaya veya ana menüye geçin.
- Ayarlardan ses efektlerini, ses düzeyini ve titreşimi değiştirebilirsiniz. Klavyede Escape ile duraklatıp devam edebilirsiniz.
- Süreli rota biterse isterseniz, reklam tamamen izlenince bir kez 30 saniye ek süre alabilirsiniz. Reklam uygun değilse bölümü ücretsiz yeniden deneyebilirsiniz.

Bölüm ilerlemesi, günlük rotalar, rekorlar, jetonlar, yıldızlar ve tercihler cihazda yerel olarak saklanır. Google Mobile Ads SDK test reklam birimleriyle yapılandırılmıştır; gerçek reklam geliri sağlamaz. Yayın öncesinde kendi AdMob uygulama/reklam birimi kimliklerinizi girin ve AdMob'da gizlilik/izin mesajlarını yapılandırıp test edin.
