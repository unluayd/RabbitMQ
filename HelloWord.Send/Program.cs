// RabbitMQ ile calismak icin gerekli siniflari projeye dahil eder.
using RabbitMQ.Client;
// Metni byte dizisine cevirmek icin gerekli sinifi projeye dahil eder.
using System.Text;

// Bu dosyanin ait oldugu isim alanini tanimlar.
namespace HelloWord.Send;

// Uygulamanin ana sinifini tanimlar.
internal class Program
{
    // Uygulama calistiginda ilk olarak burasi calisir.
    static async Task Main(string[] args)
    {
        // Kullaniciya mesaj gonderme islemi icin Enter tusuna basmasi gerektigini soyler.
        Console.WriteLine("Press [enter] to send message");
        // Kullanici Enter tusuna basana kadar uygulamayi bekletir.
        Console.ReadLine();

        // RabbitMQ baglanti ayarlarini tutacak nesneyi olusturur.
        var factory = new ConnectionFactory();
        // RabbitMQ sunucusunun calistigi adresi belirler.
        factory.HostName = "localhost";
        // RabbitMQ sunucusunun port numarasini belirler.
        factory.Port = 5672;
        // RabbitMQ kullanici adini belirler.
        factory.UserName = "guest";
        // RabbitMQ sifresini belirler.
        factory.Password = "guest";

        // Istenirse tek satirda uzak baglanti adresi tanimlamak icin kullanilabilir.
        // factory.Uri = new Uri("amqps://username:password@host/vhost");

        // RabbitMQ sunucusuna asenkron bir baglanti acip isi bitince otomatik kapatir.
        await using var connection = await factory.CreateConnectionAsync();
        // Mesaj gonderme islemleri icin baglanti uzerinden bir kanal olusturur.
        await using var channel = await connection.CreateChannelAsync();

        // "hello" adinda bir kuyruk olusturur; varsa mevcut kuyrugu kullanir.
        await channel.QueueDeclareAsync(
            // Olusturulacak veya kullanilacak kuyrugun adini belirtir.
            queue: "hello",
            // Kuyrugun kalici olup olmayacagini belirler.
            durable: false,
            // Kuyrugun sadece bu baglantiya ozel olup olmayacagini belirler.
            exclusive: false,
            // Kullanim bitince kuyrugun otomatik silinip silinmeyecegini belirler.
            autoDelete: false,
            // Kuyruk icin ek ayarlar verilmedigini belirtir.
            arguments: null
        );

        // Kuyruga gonderilecek metni tanimlar.
        string message = "Hello World!";
        // Metni RabbitMQ'nun kullanacagi byte dizisine cevirir.
        var body = Encoding.UTF8.GetBytes(message);

        // Mesaji varsayilan exchange uzerinden belirtilen kuyruga yollar.
        await channel.BasicPublishAsync(
            // Varsayilan exchange kullanildigini belirtir.
            exchange: string.Empty,
            // Mesajin hangi kuyruga gidecegini belirtir.
            routingKey: "hello",
            // Gonderilecek mesaj verisini byte olarak verir.
            body: body
        );

        // Mesajin gonderildigini ekrana yazar.
        Console.WriteLine($" [x] Sent {message}");
        // Uygulamayi kapatmak icin tekrar Enter bekledigini ekrana yazar.
        Console.WriteLine(" Press [enter] to exit.");
        // Kullanici Enter tusuna basana kadar uygulamayi acik tutar.
        Console.ReadLine();
    }
}
