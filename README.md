# 🏋️‍♂️ Gym Management Web Application

## 📌 Proje Tanıtımı

Bu uygulama, spor salonları için üyelik, randevu ve eğitmen yönetimini dijital ortama taşıyan bir web sistemidir. ASP.NET Core MVC teknolojisiyle geliştirilen bu sistem; salon yöneticilerinin, üyelerin ve eğitmenlerin tüm işlemlerini kolaylıkla takip edebilmesini sağlar.

Proje kapsamında kullanıcılar:
- Üye olarak sisteme kayıt olabilir,
- Eğitmenleri ve hizmetleri görüntüleyebilir,
- Müsait eğitmenlere uygun saatlerde randevu alabilir,
- Yapay zekâ destekli egzersiz ve diyet önerileri alabilirler.

---

## 🔧 Kullanılan Teknolojiler

- **Backend:** ASP.NET Core MVC (C#), Entity Framework Core, LINQ
- **Frontend:** HTML5, CSS3, Bootstrap 5, jQuery
- **Veritabanı:** PostgreSQL (Code-First Migrations)
- **Kimlik Doğrulama:** ASP.NET Identity (Admin ve Member rolleri)
- **Yapay Zekâ Entegrasyonu:** Google GenAI (Gemini API) – Öneri Sistemi
- **Versiyon Kontrol:** Git & GitHub

---

## 👥 Rol ve Yetkilendirme

| Rol     | Yetkiler |
|---------|----------|
| **Admin** | Eğitmen/Spor salonu/hizmet ekleme, randevu onaylama, tüm sistem kontrolü |
| **Üye**   | Kayıt olma, eğitmen/hizmet inceleme, randevu alma, AI öneri görüntüleme |

---

## 🤖 Yapay Zekâ Modülü

- Kullanıcı; boy, kilo, vücut tipi gibi verilerini girerek Google Gemini API’sine bağlanır.

- AI öneri sayfasında kişiye özel fitness + beslenme planları ve dönüşüm görselleri görüntülenir.

- Kullanılan model: gemini-2.5-flash-image 


