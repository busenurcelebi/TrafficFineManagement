# FineFlow — Trafik Cezası Yönetim Sistemi

FineFlow, şirket araçlarına ait trafik cezalarının kaydedilmesi ve yönetici ile finans birimleri arasındaki onay sürecinin takip edilmesi için geliştirdiğim bir ASP.NET Core MVC uygulamasıdır.

Projede yalnızca kayıt oluşturma işlemlerine değil; cezaların hangi sırayla değerlendirileceğine, kullanıcıların yetkilerine ve verilen kararların geçmişinin korunmasına da odaklandım.

## Kullanılan Teknolojiler

- ASP.NET Core MVC (.NET 8) ve C#
- Entity Framework Core
- SQL Server LocalDB
- ASP.NET Core Identity
- Razor Views, HTML, CSS, JavaScript
- Git

## Proje Özellikleri

### Araç Yönetimi

Araçlar plaka, tür, marka ve model bilgileriyle sisteme eklenebilir ve listelenebilir.

Binek, Çekici, Dorse ve Kiralık Araç türleri desteklenir.

Plaka bilgisi kayıt sırasında düzenlenir; boşluklar kaldırılır ve harfler büyük harfe çevrilir. Aynı plakanın tekrar kaydedilmesini önlemek için hem uygulamada kontrol yapılır hem de veritabanında benzersiz indeks kullanılır.

Bir araca birden fazla trafik cezası kaydedilebilir.

### Trafik Cezası Yönetimi

Trafik cezaları kayıtlı araçlarla ilişkilendirilir. Her ceza için tutanak numarası, tutar, tarih ve açıklama bilgileri tutulur.

Uygulamada ceza oluşturma, listeleme, detay görüntüleme ve onay geçmişini inceleme işlemleri yapılabilir.

Kayıt sırasında zorunlu alanlar, tutar aralığı, ceza tarihi, seçilen aracın varlığı ve tutanak numarasının benzersizliği kontrol edilir.

### İki Aşamalı Onay Süreci

Yeni oluşturulan trafik cezaları önce yönetici, ardından finans birimi tarafından değerlendirilir.

```text
Yeni Trafik Cezası
        |
        v
   Yönetici Onayı
     /       \
   Onay      Ret
    |         |
    v         v
 Finans     Reddedildi
 Onayı
  /   \
Onay   Ret
 |      |
 v      v
Tamamlandı Reddedildi
```

Yönetici onayı olmadan finans aşamasına geçilemez. Yönetici ve finans kullanıcıları yalnızca kendi aşamalarındaki cezalar için karar verebilir.

Her iki aşamada da onay veya ret işlemi yapılabilir. Ret işlemlerinde gerekçe yazılması zorunludur.

## Tasarım Kararları

Case kapsamında bazı iş kuralları geliştiricinin kararına bırakılmıştı. Bu konularda aşağıdaki yaklaşımı tercih ettim.

### Reddedilen cezalar

Bir ceza reddedildiğinde kaydı silmek yerine ret gerekçesiyle birlikte saklıyorum. Böylece cezanın neden reddedildiği ve kararı kimin verdiği sonradan görülebiliyor.

Reddedilen cezaları yeniden onaya açmadım. Çünkü mevcut iş akışında bir düzeltme ve yeniden değerlendirme aşaması tanımlı değil. Böyle bir ihtiyaç oluşursa bunu ayrı bir süreç olarak ele almak daha doğru olur.

Buradaki ret işlemi, trafik cezasının hukuken iptal edilmesi değil, şirket içindeki onay talebinin reddedilmesidir.

### Tamamlanan cezalar ve kayıt değişiklikleri

Yönetici ve finans onayını alan ceza tamamlanmış duruma geçer. Tamamlanan veya reddedilen bir cezaya tekrar onay ya da ret verilemez.

Ayrıca mevcut uygulamada trafik cezaları için sonradan düzenleme ve silme işlemi sunmadım. Çünkü onay verilen bir cezanın tutarı veya bağlı olduğu araç değiştirilirse önceki kararların hangi bilgilere göre verildiği belirsizleşebilir.

Bu nedenle kayıtların ve karar geçmişinin korunmasını tercih ettim.

### Onay geçmişi

Her onay ve ret işlemini `ApprovalHistory` tablosunda ayrı bir kayıt olarak tutuyorum.

Bu kayıtlarda işlemi yapan kullanıcı, tarih, işlem türü, açıklama, önceki durum ve yeni durum bilgileri bulunuyor.

Böylece bir cezanın yalnızca son durumunu değil, hangi aşamalardan geçerek o duruma ulaştığını da görebiliyoruz.

## Teknik Yaklaşım

**İş kurallarının yönetimi**

Onay işlemlerini `ApprovalService` içerisinde topladım. Böylece controller, gelen isteği karşılamakla ilgilenirken rol kontrolü ve durum geçişleri servis üzerinden yönetiliyor.

**Yetkilendirme**

Kullanıcı girişleri ve roller için ASP.NET Core Identity kullandım. `Manager` ve `Finance` rollerinin yetkileri yalnızca arayüzde değil, sunucu tarafında da kontrol ediliyor.

**Veri bütünlüğü**

Ceza durumunun güncellenmesi ile onay geçmişinin kaydedilmesi aynı veritabanı transaction'ı içinde yapılıyor.

Ayrıca güncelleme sırasında cezanın önceki durumu kontrol edilerek aynı aşamada birden fazla işlem yapılmasından kaynaklanabilecek çakışma riski azaltılıyor.

Araç, trafik cezası ve onay geçmişi arasında ilişkiler kurdum. Plaka ve tutanak numaraları için benzersiz indeksler, bağlı kayıtların otomatik silinmesini sınırlamak için de `DeleteBehavior.Restrict` kullandım.

## Kurulum

### Gereksinimler

- .NET 8 SDK
- Visual Studio 2022 veya uyumlu bir geliştirme ortamı
- SQL Server LocalDB

### Çalıştırma adımları

1. GitHub deposunu indirin veya klonlayın.
2. `TrafficFineManagement.sln` çözümünü Visual Studio ile açın.
3. NuGet paketlerinin geri yüklenmesini bekleyin.
4. `appsettings.json` dosyasındaki `DefaultConnection` bağlantı ayarını kontrol edin.
5. Visual Studio'da **Paket Yöneticisi Konsolu** üzerinden varsayılan proje olarak `TrafficFineManagement` projesini seçip şu komutu çalıştırın:

   ```powershell
   Update-Database
   ```

6. `TrafficFineManagement.csproj` dosyasının bulunduğu klasörde terminal açarak demo hesapları için şu komutu çalıştırın:

   ```powershell
   dotnet user-secrets set "DemoUsers:Password" "FineFlowDemo2026!"
   ```

7. Uygulamayı **Development ortamında** HTTPS profiliyle başlatın.

Uygulama ilk açıldığında `Admin`, `Manager` ve `Finance` rolleri oluşturulur. Demo hesapları eksikse otomatik hazırlanır ve ilgili roller atanır.

### Demo Hesapları ve Giriş Bilgileri

Değerlendirme sırasında onay sürecinin kolayca denenebilmesi için üç demo hesabı hazırlanır.

| Rol | E-posta | Şifre |
|---|---|---|
| Yönetici | `manager@test.local` | `FineFlowDemo2026!` |
| Finans | `finance@test.local` | `FineFlowDemo2026!` |
| Standart Kullanıcı | `user@test.local` | `FineFlowDemo2026!` |

Bu hesaplarla aşağıdaki işlemler denenebilir:

- **Yönetici:** Yeni oluşturulan trafik cezalarını onaylama veya gerekçe belirterek reddetme
- **Finans:** Yönetici onayından geçen cezaları değerlendirme
- **Standart kullanıcı:** Araç ve ceza kayıtlarını görüntüleme, onay yetkisi olmadan uygulamayı kullanma

**Önemli notlar:**

- Demo hesapları yalnızca `Development` ortamında ve `DemoUsers:Password` ayarı tanımlandığında oluşturulur.
- Parola proje kaynak kodunda tutulmaz; .NET User Secrets üzerinden okunur.
- Önceden mevcut olan kullanıcı hesaplarının şifreleri değiştirilmez.
- Tablodaki şifreler, **yeni bir veritabanında oluşturulan demo hesapları** için geçerlidir.
- Bu hesaplar yalnızca yerel değerlendirme amacıyla kullanılmalıdır. Gerçek ortamda demo hesapları kullanılmamalıdır.

## Test Edilen Senaryolar

Aşağıdaki işlemleri uygulama üzerinden manuel olarak kontrol ettim:

- Araç oluşturma ve listeleme
- Trafik cezası oluşturma ve detaylarını görüntüleme
- Yönetici onayıyla finans aşamasına geçiş
- Finans onayıyla sürecin tamamlanması
- Yönetici tarafından gerekçeli ret
- Finans tarafından gerekçeli ret
- Onay geçmişinin görüntülenmesi
- Standart kullanıcı hesabında onay işlemlerinin gösterilmemesi
- Demo standart kullanıcı hesabıyla giriş yapılması

Bu çalışma kapsamında otomatik test eklemedim.

---

Bu projeyi Junior .NET Developer teknik değerlendirmesi için geliştirdim. Özellikle onay sırasının doğru ilerlemesine, kullanıcı yetkilerine ve yapılan işlemlerin takip edilebilir olmasına dikkat ettim.