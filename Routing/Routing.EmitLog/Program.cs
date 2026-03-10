// RabbitMQ ile calismak icin gerekli siniflari projeye dahil eder.
using RabbitMQ.Client;
// Metni byte dizisine cevirmek icin gerekli sinifi projeye dahil eder.
using System.Text;

// Bu dosyanin ait oldugu isim alanini tanimlar.
namespace Routing.EmitLog;

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

        // Mesaji gondermeden once kullanicidan Enter tusuna basmasini ister.
        Console.WriteLine(" Press [enter] to send message.");
        // Kullanici Enter tusuna basana kadar bekler.
        Console.ReadLine();

        // RabbitMQ sunucusuna asenkron baglanti acip isi bitince otomatik kapatir.
        await using var connection = await factory.CreateConnectionAsync();
        // Mesaj gonderme islemleri icin baglanti uzerinden bir kanal olusturur.
        await using var channel = await connection.CreateChannelAsync();

        // Direct tipinde "direct-logs" adinda bir exchange olusturur; varsa mevcut exchange'i kullanir.
        await channel.ExchangeDeclareAsync(
            exchange: "direct-logs",
            type: ExchangeType.Direct);

        // Komut satirindan log seviyesini alir, gelmezse varsayilan olarak "info" kullanir.
        var severity = GetSeverity(args);
        // Gonderilecek log mesajini komut satirindan veya varsayilan metinden alir.
        var message = GetMessage(args);
        // Metni RabbitMQ'nun kullanacagi byte dizisine cevirir.
        var body = Encoding.UTF8.GetBytes(message);

        // Mesaji belirtilen log seviyesine gore direct exchange'e yollar.
        await channel.BasicPublishAsync(
            exchange: "direct-logs",
            routingKey: severity,
            body: body);

        // Gonderilen log seviyesini ve mesaji ekrana yazar.
        Console.WriteLine($" [x] Sent '{severity}':'{message}'");
        // Uygulamayi kapatmak icin tekrar Enter bekledigini ekrana yazar.
        Console.WriteLine(" Press [enter] to exit.");
        // Kullanici Enter tusuna basana kadar uygulamayi acik tutar.
        Console.ReadLine();
    }

    // Komut satirindan log seviyesini okur.
    static string GetSeverity(string[] args)
    {
        // Ilk parametre varsa onu kullanir, yoksa varsayilan "info" degerini dondurur.
        return args.Length > 0 ? args[0] : "info";
    }

    // Komut satirindan gelen metni tek bir mesaj haline getirir.
    static string GetMessage(string[] args)
    {
        // Ilk parametreden sonraki metni birlestirir, yoksa varsayilan mesaji kullanir.
        return args.Length > 1 ? string.Join(" ", args.Skip(1)) : "Hello Routing!";
    }
}
