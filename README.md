#  AIKOCLUK - Yapay Zeka Destekli YKS Koçluk ve Oyunlaştırma Platformu

AIKOCLUK, üniversite sınavına (YKS) hazırlanan öğrenciler için geliştirilmiş, geleneksel net takip uygulamalarından çok daha fazlasını sunan yeni nesil bir eğitim ekosistemidir. 

Öğrencilerin sadece akademik başarılarını değil, psikolojik durumlarını da analiz eden bu platform; **Gemini AI destekli dinamik koçluk**, **tükenmişlik (burnout) sendromu tespiti**, **sanal çalışma arkadaşı (virtual buddy)** ve **gölge rakip (shadow rival)** gibi benzersiz özellikler barındırır.

---

## ✨ Öne Çıkan Özellikler

*   🤖 **Yapay Zeka Destekli Koçluk (Gemini AI):** Öğrencinin netlerine, çalışma alışkanlıklarına ve eksik konularına göre kişiselleştirilmiş haftalık çalışma planları ve tavsiyeler üretir.
*   🔥 **Tükenmişlik (Burnout) Analizi:** Öğrencinin çalışma temposunu ve stres verilerini analiz ederek tükenmişlik riskini hesaplar, gerektiğinde mola ve motivasyon yönlendirmeleri yapar.
*   👾 **Sanal Arkadaş (Virtual Buddy):** Öğrenci çalıştıkça ve hedeflerine ulaştıkça seviye atlayan, canı (HP) artan oyunlaştırılmış (gamified) bir sanal yoldaş.
*   👤 **Gölge Rakip (Shadow Rival):** Öğrenciyi kendi hedefindeki ve seviyesindeki sanal bir rakiple radar grafikleri üzerinden kıyaslayarak rekabet duygusunu artırır.
*   📊 **Gelişmiş Sınav Analitiği:** TYT ve AYT (Sayısal, Sözel, Eşit Ağırlık) için dinamik branş girişleri, konu eksiği tespiti ve trend analiz grafikleri.
*   👨‍🏫 **Öğretmen ve Veli Portalları:** Sınıftaki öğrencilerin risk durumlarını (burnout vb.) ve gelişimlerini tek ekranda takip edebilme imkanı.

---

## 🛠️ Teknoloji Yığını (Tech Stack)

Bu proje, ölçeklenebilirlik, performans ve modern UI/UX standartları göz önünde bulundurularak geliştirilmiştir.

### Frontend (İstemci)
*   **Framework:** Next.js 14 (App Router)
*   **Dil:** TypeScript
*   **Stil & UI:** Tailwind CSS, shadcn/ui, Lucide Icons
*   **State Management & API:** TanStack Query (React Query) v5, Axios
*   **Form Yönetimi:** React Hook Form + Zod (Güçlü client-side validasyonlar)
*   **Veri Görselleştirme & Animasyon:** Recharts, Framer Motion

### Backend (Sunucu)
*   **Framework:** .NET 8 (C#) - ASP.NET Core Web API
*   **Mimarisi:** N-Tier Architecture (Controllers, Services, Repositories, DTOs, Models)
*   **Veritabanı & ORM:** PostgreSQL, Entity Framework Core
*   **Kimlik Doğrulama:** API Key Auth Middleware (Genişletilebilir yapı)
*   **Yapay Zeka Entegrasyonu:** Google Gemini API

---

## 📂 Proje Mimarisi

### Backend Yapısı
Sistem RESTful API standartlarına göre tasarlanmış olup iş mantığı `Services` katmanında soyutlanmıştır:
*   `AiCoachService` / `GeminiService`: AI entegrasyonu ve prompt yönetimi.
*   `BurnoutDetectorService`: Öğrenci verilerinden algoritmik stres hesabı.
*   `ShadowRivalService`: Gelişim kıyaslama algoritmaları.

### Frontend Yapısı
App Router ile `(student)`, `(teacher)`, `(parent)` şeklinde rol bazlı route gruplaması yapılmıştır. DTO'lar frontend tarafında TypeScript interfaceleri ile birebir eşlenerek tip güvenliği (type-safety) sağlanmıştır.

---

##  Kurulum ve Çalıştırma

Projeyi yerel ortamınızda çalıştırmak için aşağıdaki adımları izleyin.

### Gereksinimler
*   Node.js (v18+)
*   .NET 8 SDK
*   PostgreSQL
*   Google Gemini API Anahtarı

### Backend Kurulumu
1. Repoyu klonlayın:
   ```bash
   git clone [https://github.com/kullaniciadiniz/AIKOCLUK.git](https://github.com/kullaniciadiniz/AIKOCLUK.git)
