# ??? GymForce Premium Dark Theme

## ?? Renk Paleti

### Ana Renkler
```css
Primary (Profesyonellik):     #2196F3
Primary Dark:   #1976D2
Primary Light:          #42A5F5

Accent (Enerji & Motivasyon): #00E676  /* Neon Yeþil */
Accent Dark:                   #00C853
Accent Light:     #69F0AE

Secondary Accent (CTA):        #FF6B35  /* Turuncu */
```

### Arka Plan & Surface
```css
Background (Near-Black):    #0A0E27
Surface (Kartlar):            #1A1F3A
Surface Elevated (Hover):     #252B4A
Surface Hover:     #2D3454
```

### Metin Renkleri
```css
Text Primary (Yüksek Kontrast): #FFFFFF
Text Secondary:     #A0AEC0
Text Muted:     #64748B
```

### Durum Renkleri
```css
Success:  #10B981
Warning:  #F59E0B
Danger:   #EF4444
Info:     #3B82F6
```

## ?? Tasarým Kurallarý

### Border Radius
- **Normal**: 12px
- **Kartlar (Large)**: 16px
- **Butonlar (Small)**: 8px

### Gölgeler
```css
Shadow Small:   0 2px 8px rgba(0, 0, 0, 0.3)
Shadow Medium:  0 4px 16px rgba(0, 0, 0, 0.4)
Shadow Large: 0 8px 32px rgba(0, 0, 0, 0.5)
Shadow Accent:  0 4px 20px rgba(0, 230, 118, 0.2)
Shadow Primary: 0 4px 20px rgba(33, 150, 243, 0.2)
```

### Tipografi
- **Font Family**: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif
- **Font Weight**: 400 (normal), 500 (medium), 600 (semibold), 700 (bold), 800 (extrabold)
- **Baþlýklar**: Letter-spacing: -0.02em (tighter)
- **Line Height**: 1.6

## ?? Kullaným Kurallarý

### Accent (Neon Yeþil) #00E676
**Ne zaman kullanýlýr:**
- ? Ana CTA butonlarý (Kayýt Ol, Randevu Al)
- ? Baþarý mesajlarý ve onay bildirimleri
- ? Aktif durumlar ve seçili öðeler
- ? Ýkonlar ve vurgular (önemli özellikler)
- ? Çok fazla kullanmayýn - aþýrý kullaným enerjisini azaltýr

### Primary (Mavi) #2196F3
**Ne zaman kullanýlýr:**
- ? Ýkincil butonlar ve linkler
- ? Navigasyon aktif öðeleri
- ? Bilgilendirme badge'leri
- ? Form focus durumlarý

### Secondary Accent (Turuncu) #FF6B35
**Ne zaman kullanýlýr:**
- ? Uyarý mesajlarý (önemli deðil ama dikkat gerektiren)
- ? Özel CTA'ler (sýnýrlý kullaným)
- ? Vurgu gerektiren özel durumlar

### Background/Surface Oraný
```
Background (#0A0E27)
??? Surface (#1A1F3A) - Kartlar, paneller
    ??? Surface Elevated (#252B4A) - Hover, aktif durumlar
     ??? Surface Hover (#2D3454) - Ekstra vurgu
```

## ?? Animasyonlar

### Transition
```css
all 0.3s cubic-bezier(0.4, 0, 0.2, 1)
```

### Hover Efektleri
- **Kartlar**: `translateY(-8px)` + gölge artýþý
- **Butonlar**: `translateY(-2px)` + gölge artýþý + renk deðiþimi
- **Ýkonlar**: `scale(1.1)` veya renk deðiþimi

### Özel Animasyonlar
- **fadeIn**: Sayfa yüklenirken içerik için
- **pulse**: Önemli badge'ler ve bildirimler için
- **counter**: Ýstatistik sayýlarý için

## ?? Özel Bileþenler

### Icon Box
```html
<div class="icon-box">
    <i class="bi bi-trophy"></i>
</div>
```
**Kullaným**: Hero section'lar, özellik kartlarý, istatistikler

### Hover Card
```html
<div class="card hover-card">
    <!-- Ýçerik -->
</div>
```
**Efekt**: Hover'da yukarý kayma ve gölge artýþý

### Stats Card
```html
<div class="stats-card">
    <!-- Ýstatistik içeriði -->
</div>
```
**Özellik**: Sol border accent rengi, hover'da saða kayma

## ?? Responsive Davranýþ

### Breakpoints
- **Mobile**: < 768px
  - Font boyutlarý küçülür
  - Border radius hafifçe azalýr
  - Padding'ler optimize edilir
- **Tablet**: 768px - 992px
  - Orta düzey optimizasyon
- **Desktop**: > 992px
- Tam efektler ve animasyonlar

## ? JavaScript Özellikleri

### Otomatik Özellikler
1. **Auto-dismiss Alerts**: 5 saniye sonra kaybolur
2. **Animated Counters**: Ýstatistikler scroll'da animasyon yapar
3. **Back to Top Button**: 300px scroll sonrasý görünür
4. **Active Nav Highlighting**: Aktif sayfa otomatik vurgulanýr

### Utility Functions
- `showToast(message, type)`: Toast notification gösterir
- `formatCurrency(amount)`: TL formatýnda para
- `formatDate(date)`: Türkçe tarih formatý

## ?? CSS Variable'lar

Tüm renkler CSS variable olarak tanýmlý:
```css
var(--gym-primary)
var(--gym-accent)
var(--gym-bg)
var(--gym-surface)
var(--gym-text-primary)
/* ... ve daha fazlasý */
```

**Avantajý**: Kolay tema deðiþimi ve özelleþtirme

## ?? Performans

- **Google Fonts**: Preconnect ile optimize
- **Animasyonlar**: GPU-accelerated (transform, opacity)
- **Gölgeler**: Optimize edilmiþ blur deðerleri
- **Lazy Loading**: Intersection Observer kullanýmý

## ?? Eriþilebilirlik

- ? Yüksek kontrast oranlarý (WCAG AA uyumlu)
- ? Focus durumlarý belirgin
- ? Aria labels mevcut
- ? Klavye navigasyonu destekli
- ? Tooltip ve popover'lar eriþilebilir

## ?? Ýpuçlarý

1. **Accent rengi dikkatli kullanýn**: Sadece önemli CTA'ler ve vurgular için
2. **Gölgeleri katmanlandýrýn**: Derinlik hissi yaratýr
3. **Hover efektlerini abartmayýn**: Profesyonel görünümü koruyun
4. **Animasyonlarý kullanýcý etkileþimine baðlayýn**: Otomatik animasyonlardan kaçýnýn
5. **Responsive test edin**: Mobilde de güçlü görünmeli

---

**Brand Identity**: GymForce - Premium, güçlü, modern, motivasyonel
**Target Audience**: Fitness tutkunlarý, spor salonu yöneticileri
**Visual Feel**: Gece modu, yüksek teknoloji, enerji dolu
