# Project Overview
- **Game Title:** Şehrin Paketçisi (Unity ürün adı şu anda `yorunge`; mağaza/ürün adı bu çalışmada değiştirilmez).
- **High-Level Concept:** Mahallede doğru zamanda teslimat yapan bir kurye ve renkli paketleri siparişlerle eşleştiren mobil bulmaca deneyimi. Mevcut iki oynanış türünü ve yerel ilerlemeyi koruyarak sıcak, çizgi film estetiğinde daha okunaklı, hareketli ve tutarlı bir oyun oluşturmak.
- **Players:** Tek oyunculu, yerel kayıtlı; ağ sistemi eklenmeyecek.
- **Inspiration / Reference Games:** Kullanıcı belirli bir referans belirtmedi. Görsel yön mevcut mahalle/paket temasından geliştirilecek; başka bir oyunun varlıkları kopyalanmayacak.
- **Tone / Art Direction:** Kullanıcının seçimi: sıcak çizgi film. Lacivert, turuncu, krem ve mint vurgular; yuvarlatılmış kartlar, hafif derinlik, özgün paket simgeleri ve katmanlı mahalle arka planı. Pixel-art ya da 3D dönüşüm yapılmayacak.
- **Target Platform:** Android, ARM64; Unity 6000.6.7f1.
- **Screen Orientation / Resolution:** Dikey. 390×780 mantıksal tasarım ölçüsü korunabilir; 1080×2160, 1080×1920, 1080×2400 ve tablet oranlarında duyarlı düzen ve ekran güvenli alanı kullanılacak. Telefonlar geniş ekranın ortasında dar bir kolon olarak tasarlanmamalı; normalleştirilmiş ankrajlarla kullanılabilir alan doldurulacak.
- **Render Pipeline:** Mevcut URP 17.6.0 / 2D Renderer korunacak.
- **Seçilen yaklaşım:** Kalıcı modernizasyon; Canvas tabanlı uGUI + TextMeshPro sunum, ayrılmış oyun kuralları ve iki modda ölçülü oynanış geliştirmesi.
- **Alternatif:** OnGUI üzerinde hızlı makyaj daha kısa sürer, ancak mobil düzen ve bakımı zor tek dosya sorunlarını çözmez. Kullanıcı kalıcı yaklaşımı seçti.
- **Kapsam sınırı:** Bu ilk cilalı sürüm; yeni mağaza, satın alma, çevrimiçi sıralama, AI üretimli karakter, kapsamlı yeni bölüm içeriği veya yeni reklam SDK'sı içermez. Kusursuz ticari kalite garantisi yerine ölçülebilir kullanılabilirlik ve oynanış hedefleri uygulanacak.

# Game Mechanics
## Core Gameplay Loop
### Paket eşleştirme
1. Üç açık siparişi incele; renk + özgün simge ile paketleri ayırt et.
2. Bir pakete dokunarak tepsiye al; üç paket mevcut kurallara göre otomatik eşleşir. Sırasız eşleştirme ve joker davranışı korunur.
3. Paket tepsiye hareket eder; doğru sipariş teslim animasyonu ve kombo geri bildirimi verir. Yanlış seçim açıklanır; geri al ve ipucu işlevleri erişilebilir kalır.
4. Hikâyede bölüm/yıldız ilerlemesi, Ekspres'te süre ve rekor, Günlük'te üç rota ve tek seferlik günlük ödül korunur.
5. Hikâye bulmacası süresiz kalır; günlük ve Ekspres süreleri farklı amaçlara hizmet eder. Süresiz mod sakin keşfi, süreli mod SDT'nin yeterlik/ustalık hissini destekler.

### Kurye
1. Hedef şeridi ve yaklaşan adresi oku; üç şerit arasında hareket et.
2. Hedef zamanlama alanına geldiğinde paketi fırlat.
3. Mevcut başarı aralığının merkezinde daha dar bir 'Mükemmel' bölgesi göster: başlangıçta toplam başarı aralığının ortadaki %35'i. Normal başarılı teslimat ve mevcut ödül formülleri korunur; mükemmel teslimat ilk sürümde ek para vermeden görsel/sesli ustalık geri bildirimi sağlar.
4. Hata, kombo sıfırlanması ve süre cezası açıkça bildirilir; yükselen zorluk mevcut hız/hedef formülleriyle sürer.
5. Karakterin görsel şerit hareketi ile teslimat doğrulamasını tutarlı hale getir; havadaki paket eski hedefe giderken bir sonraki hedefin kimliği/konumu karışmamalı.

### İlk kullanım ve geri bildirim
- Her oynanış türü için bir kez gösterilen kısa, atlanabilir, etkileşimli yönlendirme. Yönlendirme sırasında süre/rota ilerlemesi durur.
- Başarı/yanlış seçim/son saniyeler farklı ve okunabilir geri bildirim kullanır. Efektler kontrolü kapatmaz.
- MDA hedefi: daha açık kurallar → daha az yanlış dokunma ve anlaşılır zamanlama → sıcak, tatmin edici kısa oturumlar.
- Sadece kozmetik efektlerle yeni mekanik varmış gibi gösterilmez; bu sürümün mekanik katkısı kurye zamanlama ustalığı, güvenilir durum geçişleri ve iki modda öğretici akıştır.

## Controls and Input Methods
- **Doğrulanan backend:** PlayerSettings `activeInputHandler=1`, New Input System; paket 1.20.1.
- uGUI EventSystem üzerinde `InputSystemUIInputModule`; eski `StandaloneInputModule` eklenmez.
- Kontroller InputAction olaylarıyla yönetilecek; `Keyboard.current` Update polling kaldırılacak.
- Kurye: ekranda üç şerit düğmesi + büyük fırlatma düğmesi. Klavyede A/D veya sol/sağ, Space; Escape/geri duraklatma veya üst ekranı kapatma.
- Bulmaca: dokun/seç, geri al, ipucu. Sürükle-bırak bu ilk sürümde eklenmez; mevcut kolay kullanım korunur.
- Eylemler tek merkezden oyun durumu kontrolü yapar: menü, duraklatma, öğretici, bitiş veya reklam bekleme halinde oynanış girdileri engellenir.
- En az 48 mantıksal birim dokunma yüksekliği hedeflenir; küçük simge düğmelerinin görünmez dokunma alanı da yeterli büyüklükte olur.

# UI
## Görsel sistem
- Ana yüzey: lacivert; ana eylem: turuncu; içerik: krem; başarı: mint. Renkler tutarlı bir tema varlığından alınır.
- 12–20 birim köşe yarıçapı, 8 birim boşluk ritmi, sınırlı yumuşak gölge, iç kenar vurgusu. Önce okunabilirlik; ağır blur/post-processing gerekmez.
- Basılı/seçili/devre dışı durumlar; basışta kısa ölçek tepkisi, ekran/panel açılışında 150–250 ms yumuşak geçiş.
- Türkçe karakterleri içeren statik TMP font atlası; noktalama ve gerekli karakterler test edilir. İkonlar emoji yerine sprite olur. Font ölçeği 1, sayaçlarda AutoSize kapalı; zaman ve sayılar yalnızca değer değişince güncellenir.
- Paket renkleri ayrıca altı ayrı simgeyle kodlanır; jokerin ayrı görünümü olur. Renk tek bilgi kanalı değildir.

## Ana menü taslağı
```text
┌─────────────────────────────────┐
│ Şehrin Paketçisi      Jeton  ⚙  │
│                                 │
│  Mahalle illüstrasyonu + maskot  │
│  Kısa başlık / mevcut mahalle    │
│                                 │
│ [Kurye]   [Paket Eşleştirme]     │
│                                 │
│ [Mahalle] [Ekspres] [Günlük]     │
│ Seçilen modun kısa açıklaması   │
│ [Bölüm 08 • yıldızlar • Harita] │
│                                 │
│ Günlük seçiliyse: 3 rota kartı  │
│                                 │
│     [  OYNAMAYA BAŞLA  →  ]     │
└─────────────────────────────────┘
```
- Günlük kartlar yalnızca ilgili modda açılır; tüm sayılar ve seçenekler aynı anda ekrana yığılmaz.
- Ayarlar, harita ve sonuçlar ayrı panellerdir. Alt CTA güvenli alan içinde sabitlenir; küçük ekranlarda orta içerik kayabilir.

## Bulmaca taslağı
```text
[Mahalle / Bölüm] [Süre*] [Duraklat]
[Hedef ilerlemesi / kombo]
[ Sipariş 1: simgeli 3 paket ]
[ Sipariş 2: simgeli 3 paket ]
[ Sipariş 3: simgeli 3 paket ]
[     Tepsi:  □  □  □      ]
[İpucu: ücretsiz/5]       [Geri al]
[3 kolonlu paket ızgarası; gerekirse kayar]
*Süre yalnızca süreli modlarda
```

## Kurye taslağı
```text
[Mod / Bölüm] [Süre] [Duraklat]
[Teslimat hedefi] [Kombo]
[Katmanlı sokak, belirgin hedef işareti]
[Kurye + paket uçuşu + kısa başarı efekti]
[Normal / Mükemmel zamanlama göstergesi]
[   SOL   ] [  ORTA  ] [   SAĞ   ]
[          PAKETİ FIRLAT          ]
```
- Sonuç paneli: teslimat/sipariş sayısı, hata, kazanılan ödül, yıldız; birincil tekrar/sonraki bölüm ve ikincil menü. Ekspres süresinin bitmesi 'Tur tamamlandı' olarak sunulur, hikâye/günlük süre sonu başarısızlıkla karıştırılmaz.
- Modallar alttaki butonlara dokunmayı engeller; yanlışlıkla aynı tıklama iki eylem üretmez.

# Key Asset & Context
## İncelenen gerçek durum
- `Assets/Scripts/PaketciGame.cs`: 2088 satır. Oyun kuralları, `OnGUI`, çizim, ses üretimi ve PlayerPrefs tek sınıfta.
- `OnGUI` 390×780 sabit Rect ve GUI.matrix ile çiziyor. Düğmeler tek piksellik doku arka planları, border=0 ile oluşturuluyor; mevcut 'basic' görünümün somut nedeni bu.
- `DrawMenu`, `DrawCourierGame`, `DrawLegacyGame`, sonuç/duraklatma/ayar/harita fonksiyonları tamamen IMGUI.
- `BuildOrderWave`, `CanMatchOrder`, `AddPackage`, `ShowHint`, `ThrowPackage`, `SpawnNextTarget` korunacak temel davranış kaynakları.
- `SpawnNextTarget`: önceki şerit takibi son hedef yerine bir önceki hedefle karşılaştırılabiliyor; düzeltilecek.
- `RegisterMiss` süreyi bitirip `FinishTimedRun` çağırabildiği için Update aynı karede tekrar bitirme çağrısı yapabilir; bitiş idempotent olacak.
- `ThrowPackage` mantıksal şeridi doğrularken görsel şerit henüz geçişte olabilir; yeni model/görsel geçiş politikası tek kaynaktan yönetilecek.
- `AddPackage` üçüncü seçimde eşleştirmeyi hemen yapıyor; animasyonların ayrıca durum/işlem kilidi olması gerekir.
- Günlük ödül maskesi ve hikâye yıldız/açık bölüm kayıtları iki oynanış türü arasında ortaktır. Mevcut veriler korunacak; bu sürümde sessizce ayrılmayacak.
- `Assets/Scripts/RewardedAdService.cs`: Google Mobile Ads 11.6.0, UMP, test reklam kimlikleri. `Show`, `IsReady`, `PrivacyOptionsRequired`, `ShowPrivacyOptions` sözleşmeleri korunacak; SDK veya üretim kimlikleri değiştirilmeyecek.
- `Assets/Scenes/SampleScene.unity`: kayıtlı kökler Main Camera ve Global Light 2D; Paketci Game çalışma anında Bootstrap ile ekleniyor. Mevcut sahnede kalıcı Canvas/sprite dünyası yok.
- `Assets/Settings/InputSystem_Actions.inputactions`: mevcut şablon asset; oyun ona bağlı değil. Yeni oyuna özel actions kullanılacak, şablonun ilgisiz eylemleri değiştirilmez.
- Mevcut varlıklar arasında oyun sanat paketi yok; şekil tabanlı çizimler kullanılıyor. İlk sürümde özgün, yeniden kullanılabilir yerel 2D çizimler/simgeler hazırlanacak. Harici AI üretimi veya ücretli varlık alımı planın gereği değildir.
- Mevcut konsol sorgusunda hata/uyarı yok; bu tam test edilmiş olduğu anlamına gelmez.
- AssetDatabase.GetDependencies ile sahne ve iki scriptin bağımlılık/dependent taraması yapıldı. Scene scriptlere serileştirilmiş referans taşımıyor; Bootstrap ve C# referansları ayrıca incelendi. Uygulamadan hemen önce değiştirilecek varlıklar için etki analizi tekrar edilecek.

## Oluşturulacak / değiştirilecek dosyalar
- **Değiştir:** `Assets/Scripts/PaketciGame.cs` — Bootstrap/yaşam döngüsü cephesi; eski çizim ve kuralları aşamalı olarak taşır, aynı anda iki arayüzü çizmez.
- **Yeni:** `Assets/Scripts/Game/PaketciSession.cs` — durum makinesi ve ortak tur yaşam döngüsü; `GameState`, `GameMode`, `GameplayStyle`, `StartRun`, `Pause`, `Resume`, `FinishRun`.
- **Yeni:** `Assets/Scripts/Game/SortingRules.cs`, `CourierRules.cs` — test edilebilir kurallar, seeded hedef/tedarik üretimi; kalıcı UI referansı içermez.
- **Yeni:** `Assets/Scripts/Game/PaketciSaveRepository.cs` — mevcut `Paketci.*` anahtarları, günlük tarih kontrolü, kayıt işlemlerinin birleştirilmesi.
- **Yeni:** `Assets/Scripts/Game/PaketciGameConfig.cs`, `Assets/GameData/PaketciGameConfig.asset` — mevcut süre/hız/hedef/ödül varsayılanları ve mükemmel pencere parametresi.
- **Yeni:** `Assets/Scripts/Input/PaketciInputController.cs`, `Assets/Input/PaketciControls.inputactions` — olay tabanlı UI/oynanış girdisi.
- **Yeni:** `Assets/Scripts/UI/PaketciUIController.cs`, `PaketciTheme.cs`, `SafeAreaLayout.cs`, `ButtonFeedback.cs`, `SortingBoardView.cs`, `CourierStreetView.cs`, `TutorialController.cs`, `RunFeedbackView.cs`.
- **Yeni:** `Assets/UI/Prefabs/PaketciUI.prefab`, `PackageCard.prefab`, `OrderCard.prefab`, `ModeCard.prefab`, `Dialog.prefab` — düzenlenebilir Canvas hiyerarşisi; statik UI, değişen HUD ve efekt katmanları ayrılır.
- **Yeni:** `Assets/UI/PaketciTheme.asset`, `Assets/UI/Fonts/` (lisansı uygun font/TMP atlası), `Assets/Art/UI/` (9-slice panel/button ve simgeler), `Assets/Art/Environment/` (mahalle katmanları), `Assets/Art/Characters/` (stilize kurye/paket maskotu çizimleri).
- **Değiştir:** `Assets/Scenes/SampleScene.unity` — tek oyun giriş noktası, UI prefabı, kamera/Canvas bağlantıları. Runtime Bootstrap sahnede controller varken ikinci örnek oluşturmaz.
- **Yeni, gerekirse:** `Assets/Editor/PaketciPresentationBuilder.cs` — tekrar çalıştırıldığında kopya üretmeyen editör kurulum/çizim aracı; oyuncu build'ine dahil edilmez.
- **Yeni:** `Assets/Tests/EditMode/` ve `Assets/Tests/PlayMode/` altında kurallar, kayıt, durum ve UI testleri; uygun asmdef/Unity Test Framework kurulumu yalnızca mevcut paket uyumluluğu doğrulanarak.
- **Dokunma:** `RewardedAdService.cs`, Google paket kaynakları, URP varlıkları, Android gradle dosyaları; mevcut sözleşmede engel çıkarsa önce raporlanır ve ayrı onay alınır.

# Implementation Steps
## Kesinti sonrası doğrulanan ilerleme ve devam noktası
- Geri dönüş yedeği mevcut: `PaketciBackups/20261102_161851/`. Mevcut ana oyun scripti, reklam scripti, SampleScene ve şablon input dosyası bu yedekle birebir aynı. Git deposu önceki kontrolde tespit edildi; kalan uygulama öncesinde güncel diff tekrar kontrol edilmeli.
- `Assets/Scripts/Game/PaketciGameConfig.cs` ve `PaketciSaveRepository.cs` oluşturulmuş ve yeniden okunarak doğrulanmıştır. Bunlar henüz `PaketciGame` tarafından kullanılmıyor; yapılandırma asseti de henüz yok.
- Yeni Canvas, UI prefabları, oyun sanat varlıkları, oyun input asseti, Session/SortingRules/CourierRules ve testler henüz oluşturulmamış. Görsel modernizasyon tamamlanmış değildir.
- Güncel konsol sorgusunda hata/uyarı yok. Bu sonuç yeni sınıfların davranış testlerinden geçtiği anlamına gelmez; mevcut ilerlemeye yazan repository üretim PlayerPrefs ile inceleme sırasında örneklenmedi.
- Step 1'in yedekleme kısmı tamamlanmış; altı oyun kombinasyonunun davranış referansı ve güncel Git diff kontrolü hâlâ yapılmalı. Step 2 yalnızca config ve kayıt deposunun ilk taslağı düzeyinde kısmen tamamlanmıştır; Steps 3–9 bekliyor.
- Devam ederken bu iki sınıf yeniden oluşturulmayacak; önce test edilecek, gerektiğinde hedefli düzeltilecek ve sonra Session/kurallar üzerinden bağlanacak. Eski oyun çizimi yeni UI doğrulanana kadar korunacak; son aşamada kaldırılacak.
- Kayıt deposunun mevcut `Complete` metodu tek başına tekrar çağrı koruması sağlamıyor; tek ödül güvencesi Session tur kimliği/bitiş kontrolüyle uygulanmalı ve test edilmelidir. Geçersiz indeks/parametre sınırları ve depo null sözleşmesi de testlerde ele alınmalı.

1. **Geri dönüş noktası ve davranış referansı oluştur.**
   - Dosyalar: mevcut iki script, SampleScene, input ve kayıt anahtarları.
   - Değişiklik öncesi bağımlılık taraması, repo/diff durumu ve sahne bağlantılarını tekrar kontrol et. Varsa Git snapshot, yoksa değiştirilecek dosyaların kontrollü yedeği; kullanıcının değişikliklerini silme.
   - Üç mod × iki oynanış türünde başlangıç, başarılı tur, hata, yeniden deneme akışını referans olarak kaydet. Gerçek ilerlemeyi sıfırlama; testler ayrı anahtar deposu kullanır.
   - **Dependencies:** Plan onayı.
2. **Model, kayıt ve durum akışını ayır.**
   - Dosyalar: Game altındaki yeni sınıflar ve PaketciGame.cs.
   - Mevcut eşleştirme, joker, süre/hedef/ödül ve kayıt davranışlarını ilk önce değiştirmeden taşı. Sunum olayları/snapshot ile UI güncellensin.
   - Tur bitişini tek seferlik yap; reklam callback'i tur kimliğini doğrulasın, eski callback yeni tura ödül vermesin. Tarih yenileme ve ayar/duraklatma süre davranışını merkezileştir.
   - Şerit hedefi geçmişini düzelt; teslimat hedefinin uçuş snapshot'ını sakla. Animasyonlar oyun mantığının içinde konum hesabına bağlı olmasın.
   - **Dependencies:** Step 1. **Kontrol:** eski örnek seed'ler, ödüller ve kayıt anahtarları için regresyon testleri.
3. **Görsel kit ve font hazırlığı.**
   - Dosyalar: Art klasörleri, UI/Fonts, PaketciTheme, tema asseti.
   - Tutarlı yuvarlatılmış 9-slice düğme/paneller, altı paket simgesi + joker, ayar/duraklatma/jeton/yıldız simgeleri; mahalle katmanları ve stilize kurye hazırlansın.
   - Türkçe metinler için uygun font lisansı ve glyph kapsamı kontrol edilsin; gerekli TMP temel kaynakları yoksa yalnızca gerekli kaynaklar eklensin.
   - Dokular bir defa hazırlanır; kare başına Texture2D/GUIStyle üretilmez. İthalat ayarları Android'e uygun, okunabilir CPU kopyaları yalnızca gerektiğinde tutulur.
   - **Dependencies:** Step 1. Step 2 ile bağımsız görsel hazırlık yapılabilir.
4. **Canvas ve modern menüleri kur.**
   - Dosyalar: UI prefabları, UIController, SafeAreaLayout, ButtonFeedback, SampleScene.
   - CanvasScaler, ankraj, düzen grupları, güvenli alan, küçük ekran kaydırma; menü, mod seçici, koşullu günlük kartlar, harita, ayar ve sonuç/duraklatma panelleri.
   - 3 farklı buton durumu ve geçiş animasyonları; üstte para, seçili mod açıklaması ve baskın oynama düğmesi. Kullanılamayan eylemleri devre dışı göster.
   - UI controller yalnızca modelin sunumunu ve eylem çağrılarını yönetir. Bootstrap ve prefab bağlantıları bir tek giriş noktası oluşturur.
   - **Dependencies:** Steps 2–3.
5. **Olay tabanlı mobil/klavye kontrolleri bağla.**
   - Dosyalar: PaketciControls.inputactions, InputController, EventSystem, UIController.
   - UI ve gameplay action map'leri; callback abonelikleri OnEnable/OnDisable ile yönetilir. Aynı eylem UI ve klavyeden gelince çift işlem engellenir.
   - Escape/Android geri, modal kapatma, duraklatma ve reklam bekleme doğru durumlarda çalışır. İlgisiz define veya paket uyumluluk bypass'ı kullanılmaz.
   - **Dependencies:** Steps 2, 4.
6. **Bulmaca ekranını yeniden oluştur ve cilala.**
   - Dosyalar: SortingBoardView, PackageCard/OrderCard prefabları, RunFeedbackView, SortingRules.
   - Üç sipariş, simgeli paket ızgarası, tepsi, ipucu fiyatı ve geri al. Maksimum 12 paket halinde taşma önlensin; küçük cihazda ızgara kayabilsin.
   - Paket tepsiye geçişi, eşleşme kutlaması, tamamlanan sipariş görünümü; kısa geçişlerde işlem kilidi ile çift ödül ve eski kart indeksleri engellensin.
   - İpucu mevcut seçimle uyumlu paketi işaretlesin; başarısız ipucu denemesi para kesmesin. Ses/titreşim tercihlerine uy.
   - **Dependencies:** Steps 2–5.
7. **Kurye ekranını geliştir.**
   - Dosyalar: CourierStreetView, CourierRules, config, RunFeedbackView.
   - Katmanlı sokak, yumuşak şerit geçişleri, belirgin adres/hedef simgesi, normal/mükemmel zamanlama alanı ve paket uçuşu.
   - Görsel şerit geçişi sırasında teslimat politikası: fırlatma ancak karakter hedef şeride yeterince ulaştığında kabul edilir; UI bunu kısa 'şerit değiştiriliyor' durumu ile anlatır. Zaman penceresi mevcut yaklaşma davranışıyla test edilir.
   - Yeni hedef uçuş tamamlanmadan üretilebilir, fakat eski paket sabitlenmiş eski hedef görseline ulaşır; efektler sonraki eylemi bloke etmez.
   - Mükemmel başarı ayrı olaydır; mevcut para dengesini ve skor ölçeğini değiştirmez.
   - **Dependencies:** Steps 2–5.
8. **Öğretici ve tam ekran akış regresyonunu tamamla.**
   - Dosyalar: TutorialController, UIController, Session, SaveRepository.
   - İki kısa ve atlanabilir ilk kullanım yönlendirmesi; yeni sürüm öğretici anahtarları eski ilerlemeye dokunmadan eklenir.
   - Sonuç/duraklatma/ayar/günlük/harita ekranları, test reklamının hazır/değil/iptal/ödül durumları mevcut servisle bağlanır.
   - Eski OnGUI yolu tamamen kaldırılır; gereksiz çalışma anı çizim ve geçici GUI doku üretimleri temizlenir. Mevcut ses geri bildirimi yeniden kullanılır; mixer/SDK yeniden yapılandırılmaz.
   - **Dependencies:** Steps 6–7.
9. **Cihaz, performans ve görsel kabul kontrolü.**
   - Dosyalar: testler, sahne/prefablar, yalnızca hatalı sunum dosyaları.
   - Aşağıdaki test matrisini çalıştır; önce/sonra aynı çözünürlük ekran görüntülerini karşılaştır. Android development build ve gerçek cihaz varsa test et.
   - Cihaz/SDK yoksa sonucu açıkça 'Editör doğrulandı, Android cihaz doğrulanmadı' olarak raporla; build başarı iddiasında bulunma.
   - **Dependencies:** Step 8.

# Verification & Testing
## EditMode kurallar ve kayıt
- Eşleştirme: farklı sıra, tekrarlı renk, joker, yanlış üçlü, zaten kullanılmış paket, üçten fazla seçim ve boş geri al.
- Üretilen 9–12 paketli dalgalarda siparişleri tamamlayacak tedarik mevcut; aynı seed aynı sonucu verir.
- İpucu: ilk ücretsiz; sonraki 5 jeton; yetersiz para veya öneri yokken kesinti yok.
- Kurye: sol/orta/sağ sınırları; erken/geç/yanlış şerit/normal/mükemmel isabet; zaman penceresi sınırları ve hareket geçişi.
- Hedef tekrar kontrolü son hedefi esas alır; günlük seed deterministiktir.
- FinishRun aynı karede iki kez çağrılsa da kayıt, ses/ödül ve sonuç olayı yalnızca bir kez.
- Hikâye yıldızları sadece eski en iyiye göre artar; açık bölüm, jeton ve eski anahtarlar korunur. Günlük maskesi mevcut şekilde iki stil için ortak kalır.
- Günlük UTC tarih sınırı, aynı ödülü iki kez almama; eski reklam callback'inin yeni tura etkisiz olması.

## PlayMode / manuel akışlar
- 2 oynanış türü × 3 mod: başlat, duraklat, ayarlar, devam, başarısızlık/tur sonucu, yeniden dene, menü ve sonraki bölüm.
- Süresiz hikâye bulmacası yanlışlıkla süre nedeniyle bitmez; öğretici ve arka plana geçiş süreyi tüketmez.
- Hızlı/çoklu dokunma, tekrar enable/disable, modal açıkken alttaki buton, bitiş anında girdiler: tek işlem, tek ödül, tek Canvas/EventSystem/controller.
- RewardedAdService değiştirilmeden hazır değil/iptal/başarılı akışları; ek süre tur başına bir kez. Test reklamları üretim reklamı olarak sunulmaz.
- Ses/titreşim/seviye ayarları uygulama yeniden açılınca korunur.

## Görsel kabul
- 1080×1920, 1080×2160, 1080×2400, 720×1280 ve tablet oranları; çentik/sistem çubuğu simülasyonu.
- Kritik CTA ve kontroller güvenli alanın içinde; metin üst üste binmez, Türkçe `ÇĞİÖŞÜ çğıöşü` doğru görünür.
- Ana düğme yuvarlatılmış, basılı/seçili/devre dışı durumları belirgin; eski kare/tek piksellik GUI kutuları kalmaz.
- Küçük ekranlarda tüm paketlere ve günlük kartlara erişim; renk + simge ayrımı, yeterli metin kontrastı.
- Menüler, her iki oyun ekranı ve sonuç panellerinin önce/sonra görüntüleri kullanıcıya gösterilir.

## Performans ve build
- Hedef: uygun bir Android referans cihazında 60 FPS; gerçek ölçüm olmadan garanti verilmez.
- Sabit oynanış karelerinde yeni GUIStyle/Texture2D oluşmaz, sprite/kart efektleri tekrar kullanılır; tekrar tur açıp kapamada bellek sürekli yükselmez.
- Değişen sayaçlar sadece değer değiştiğinde güncellenir; bütün ana Canvas her kare yeniden kurulmaz. Aşırı gölge/şeffaf katman ve dekoratif raycast hedefleri azaltılır.
- Derleme/konsol: yeni hata, missing script veya null UI referansı yok. Android build kurulu SDK ve mevcut gradle yapısıyla denenir; paket kaynağı/uyumluluk guard'ları değiştirilmez.
- Tamamlanma raporu: değişen dosyalar, doğrulanmış testler, cihaz/build kısıtları ve varsa kalan görsel/oynanış iyileştirmeleri açıkça listelenir.
