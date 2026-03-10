// RabbitMQ ile calismak icin gerekli siniflari projeye dahil eder.
using RabbitMQ.Client;
// Metni byte dizisine cevirmek icin gerekli sinifi projeye dahil eder.
using System.Text;

// Bu dosyanin ait oldugu isim alanini tanimlar.
namespace PublishSubscribe.EmitLog;

// Uygulamanin ana sinifini tanimlar.
internal class Program
{
    // Uygulama calistiginda ilk olarak bu metot calisir.
    static async Task Main(string[] args)
    {
        // Mesaji gondermeden once kullanicidan Enter tusuna basmasini ister.
        Console.WriteLine(" Press [enter] to send message.");
        // Kullanici Enter tusuna basana kadar bekler.
        Console.ReadLine();

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

        // Fanout tipinde "logs" adinda bir exchange olusturur; varsa mevcut exchange'i kullanir.
        await channel.ExchangeDeclareAsync(
            exchange: "logs",
            type: ExchangeType.Fanout);

        // Gonderilecek log mesajini komut satirindan veya varsayilan metinden alir.
        var message = GetMessage(args);
        // Metni RabbitMQ'nun kullanacagi byte dizisine cevirir.
        var body = Encoding.UTF8.GetBytes(message);

        // Mesaji "logs" exchange'ine yollar.
        await channel.BasicPublishAsync(
            exchange: "logs",
            routingKey: string.Empty,
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
        // Parametre geldiyse birlestirir, gelmediyse varsayilan log mesajini kullanir.
        return args.Length > 0 ? string.Join(" ", args) : "info: Hello Logs!";
    }
}
