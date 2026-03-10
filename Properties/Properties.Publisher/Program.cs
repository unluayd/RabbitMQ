// RabbitMQ ile calismak icin gerekli siniflari projeye dahil eder.
using RabbitMQ.Client;
// Metni byte dizisine cevirmek icin gerekli sinifi projeye dahil eder.
using System.Text;

// Bu dosyanin ait oldugu isim alanini tanimlar.
namespace Properties.Publisher;

// Uygulamanin ana sinifini tanimlar.
internal class Program
{
    // Uygulama calistiginda ilk olarak bu metot calisir.
    static async Task Main(string[] args)
    {
        // RabbitMQ baglanti ayarlarini tutacak nesneyi olusturur.
        var factory = new ConnectionFactory
        {
            // RabbitMQ sunucusunun calistigi adresi belirler.
            HostName = "localhost",
            // RabbitMQ sunucusunun port numarasini belirler.
            Port = 5672,
            // RabbitMQ kullanici adini belirler.
            UserName = "guest",
            // RabbitMQ sifresini belirler.
            Password = "guest"
        };

        // RabbitMQ sunucusuna asenkron baglanti acip isi bitince otomatik kapatir.
        await using var connection = await factory.CreateConnectionAsync();
        // Mesaj gonderme islemleri icin baglanti uzerinden bir kanal olusturur.
        await using var channel = await connection.CreateChannelAsync();

        // Kuyruk icin ek ozellikleri tutacak sozlugu olusturur.
        var arguments = new Dictionary<string, object?>
        {
            // Mesajlarin kuyrukta en fazla 5 saniye tutulacagini belirtir.
            ["x-message-ttl"] = 5000,
            // Kuyruk kullanilmadiginda 60 saniye sonra silinmesini belirtir.
            ["x-expires"] = 60000,
            // Kuyrukta en fazla 500 mesaj tutulabilecegini belirtir.
            ["x-max-length"] = 500,
            // Kuyrugun alabilecegi maksimum agirligi byte cinsinden belirtir.
            ["x-max-length-bytes"] = 104576,
            // Islenemeyen mesajlarin yonlendirilecegi olu harf exchange'ini belirtir.
            ["x-dead-letter-exchange"] = "my.dead.letter.exchange",
            // Olu harf exchange'ine giderken kullanilacak routing key degerini belirtir.
            ["x-dead-letter-routing-key"] = "deadLetter",
            // Kuyrugun destekledigi maksimum oncelik sayisini belirtir.
            ["x-max-priority"] = 10,
            // Kuyrugun calisma modunu "lazy" olarak ayarlayip mesajlari diske yazmayi tercih eder.
            ["x-queue-mode"] = "lazy",
            // Ana kuyruk kopyasinin secilme stratejisini belirtir.
            ["x-queue-master-locator"] = ""
        };

        // "hello" adinda bir kuyruk olusturur ve ek ozellikleri uygular.
        await channel.QueueDeclareAsync(
            // Kullanilacak kuyrugun adini belirtir.
            queue: "hello",
            // Kuyrugun kalici olup olmayacagini belirler.
            durable: true,
            // Kuyrugun sadece bu baglantiya ozel olup olmayacagini belirler.
            exclusive: false,
            // Kullanim bitince kuyrugun otomatik silinip silinmeyecegini belirler.
            autoDelete: false,
            // Kuyruk icin tanimlanan ek ozellikleri uygular.
            arguments: arguments);

        // Gonderilecek mesaji komut satirindan veya varsayilan degerden alir.
        var message = GetMessage(args);
        // Metni RabbitMQ'nun kullanacagi byte dizisine cevirir.
        var body = Encoding.UTF8.GetBytes(message);

        // Mesajla birlikte gonderilecek temel AMQP ozelliklerini olusturur.
        var properties = new BasicProperties
        {
            // Mesajin kalici olarak isaretlenmesini saglar.
            Persistent = true,
            // Mesajin icerik turunu ayarlar.
            ContentType = "application/json",
            // Mesajin icerik kodlamasini ayarlar.
            ContentEncoding = "UTF-8",
            // Mesajin onceligini belirtir.
            Priority = 5,
            // Istek-yanit islemlerinde kullanilabilecek iliski kimligini belirtir.
            CorrelationId = Guid.NewGuid().ToString(),
            // Yanitin gonderilecegi kuyruk adini belirtir.
            ReplyTo = "reply_queue",
            // Mesajin son kullanma suresini milisaniye cinsinden belirtir.
            Expiration = "60000",
            // Mesajin benzersiz kimligini belirtir.
            MessageId = Guid.NewGuid().ToString(),
            // Mesajin olusturulma zamanini belirtir.
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            // Mesajin turunu belirtir.
            Type = "command",
            // Mesaji gonderen kullanici kimligini belirtir.
            UserId = "user123",
            // Mesaji gonderen uygulama kimligini belirtir.
            AppId = "app123"
        };

        // Mesaji varsayilan exchange uzerinden belirtilen kuyruga yollar.
        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: "hello",
            mandatory: false,
            basicProperties: properties,
            body: body);

        // Mesajin gonderildigini ekrana yazar.
        Console.WriteLine($" [x] Sent {message}");
        // Uygulamayi kapatmak icin tekrar Enter bekledigini ekrana yazar.
        Console.WriteLine(" Press [enter] to exit.");
        // Kullanici Enter tusuna basana kadar uygulamayi acik tutar.
        Console.ReadLine();
    }

    // Komut satirindan gelen metni tek bir mesaj haline getirir.
    static string GetMessage(string[] args)
    {
        // Parametre geldiyse birlestirir, gelmediyse varsayilan mesaji kullanir.
        return args.Length > 0 ? string.Join(" ", args) : "Hello Properties!";
    }
}
