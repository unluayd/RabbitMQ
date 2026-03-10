// RabbitMQ ile calismak icin gerekli siniflari projeye dahil eder.
using RabbitMQ.Client;
// Metni byte dizisine cevirmek icin gerekli sinifi projeye dahil eder.
using System.Text;

// Bu dosyanin ait oldugu isim alanini tanimlar.
namespace WorkQueues.NewTask;

// Uygulamanin ana sinifini tanimlar.
internal class Program
{
    // Uygulama calistiginda ilk olarak burasi calisir.
    static async Task Main(string[] args)
    {

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

        // "task_queue" adinda bir kuyruk olusturur; varsa mevcut kuyrugu kullanir.
        await channel.QueueDeclareAsync(
            // Olusturulacak veya kullanilacak kuyrugun adini belirtir.
            queue: "task_queue",
            // Kuyrugun kalici olup olmayacagini belirler.
            durable: true,
            // Kuyrugun sadece bu baglantiya ozel olup olmayacagini belirler.
            exclusive: false,
            // Kullanim bitince kuyrugun otomatik silinip silinmeyecegini belirler.
            autoDelete: false,
            // Kuyruk icin ek ayarlar verilmedigini belirtir.
            arguments: null
        );
        // Kuyruga gonderilecek metni tanimlar.
        string message = GetMessage(args);
        // Metni RabbitMQ'nun kullanacagi byte dizisine cevirir.
        var body = Encoding.UTF8.GetBytes(message);

        // Mesajla birlikte gonderilecek temel AMQP ozelliklerini olusturur.
        var properties = new BasicProperties();
        // Mesajin kalici olarak isaretlenmesini saglar.
        properties.Persistent = true;

        // Mesaji varsayilan exchange uzerinden belirtilen kuyruga yollar.
        await channel.BasicPublishAsync(
            // Varsayilan exchange kullanildigini belirtir.
            exchange: string.Empty,
            // Mesajin hangi kuyruga gidecegini belirtir.
            routingKey: "task_queue",
            // Mesajin hedefe ulasmamasi durumunda geri donus davranisini varsayilan birakır.
            mandatory: false,
            // Gonderilecek mesaj verisini byte olarak verir.
            basicProperties: properties,
            body: body
        );

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
        // Parametre geldiyse birlestirir, gelmediyse varsayilan metni kullanir.
        return args.Length > 0 ? string.Join(" ", args) : "Hello World!";
    }
}

