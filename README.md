# Yörünge — Şehrin Paketçisi

Unity `6000.6.7f1` ile hazırlanmış, dikey ekranlı mahalle temalı oyun prototipi. Ana menüden iki ayrı oynanış tarzı seçilebilir: üç şeritli kurye aksiyonu veya ilk prototipteki paket eşleştirme bulmacası. Her iki tarzda da Mahalle, Ekspres ve Günlük modları bulunur.

## Çalıştırma

Projeyi Unity Hub ile açıp `Assets/Scenes/SampleScene.unity` sahnesini Play modunda çalıştırın. Oyun başlangıçta kendi arayüzünü ve seviyesini oluşturur; sahneye ayrıca nesne eklemek gerekmez.

## Oynanış tarzları ve modlar

- **Kurye Aksiyonu:** Kuryeyi üç şerit arasında yönlendirip hedefe yaklaşırken doğru zamanda paket fırlatın.
- **Paket Eşleştirme:** İlk sürümdeki sipariş kartı, renkli paketler, ayırma tepsisi, jokerler ve ipuçlarıyla siparişleri tamamlayın.
- **Mahalle:** Kurye tarzında süre içinde teslimat hedefine ulaşın; eşleştirme tarzında bölüm siparişlerini tamamlayın.
- **Ekspres:** Kurye tarzında isabetli atışlarla süre kazanıp teslimat rekoru kırın; eşleştirme tarzında 75 saniyelik sipariş serisini oynayın.
- **Günlük:** Her iki tarzda da üç günlük rota/bulmaca oynanabilir. Kurye akşam rotasında zamanlama aralığı daha dardır; günlük ödül cihazda bir kez alınır.
- Kurye kontrolleri: **SOL / ORTA / SAĞ** düğmeleriyle şerit değiştirin; hedef yeşil zamanlama aralığına girdiğinde **PAKETİ FIRLAT!** düğmesine basın. Yanlış şerit veya zamanlama 4 saniye kaybettirir.
- Klavyede kurye için **A / Sol Ok**, **D / Sağ Ok** ile şerit değiştirin; **Space** ile atış yapın. Paket eşleştirme tarzında renkleri eşleştiren üç paketi tepsiye seçin; **GERİ AL** ve **İPUCU** düğmelerini kullanın.
- Ana ekrandan bölüm haritasını ve **AYARLAR**'ı açın. Oyun sırasında **Ⅱ** ile duraklatın; buradan ayarlara, yeniden başlatmaya veya ana menüye geçin.
- Ayarlardan ses efektlerini, ses düzeyini ve titreşimi değiştirebilirsiniz. Klavyede Escape ile duraklatıp devam edebilirsiniz.
- Süre biterse isterseniz, reklam tamamen izlenince bir kez 30 saniye ek süre alabilirsiniz. Reklam uygun değilse bölümü ücretsiz yeniden deneyebilirsiniz.

Bölüm ilerlemesi, günlük ödüller, her oynanış tarzının Ekspres rekoru, jetonlar, yıldızlar ve seçili oynanış tarzı cihazda yerel olarak saklanır. Görsel ve oynanış şu anda kodla çizilmiş ilk prototiptir; gerçek 3D karakter/şehir assetleri ve fizik tabanlı paket fırlatma sonraki geliştirme aşamalarıdır. Google Mobile Ads SDK test reklam birimleriyle yapılandırılmıştır; gerçek reklam geliri sağlamaz. Yayın öncesinde kendi AdMob uygulama/reklam birimi kimliklerinizi girin ve AdMob'da gizlilik/izin mesajlarını yapılandırıp test edin.
