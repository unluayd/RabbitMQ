// RabbitMQ ile calismak icin gerekli siniflari projeye dahil eder.
using RabbitMQ.Client;
// Metni byte dizisine cevirmek icin gerekli sinifi projeye dahil eder.
using System.Text;

// Bu dosyanin ait oldugu isim alanini tanimlar.
namespace Topic.EmitLog;

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

        // Topic tipinde "topic-logs" adinda bir exchange olusturur; varsa mevcut exchange'i kullanir.
        await channel.ExchangeDeclareAsync(
            exchange: "topic-logs",
            type: ExchangeType.Topic);

        // Komut satirindan routing key bilgisini alir, gelmezse varsayilan anahtari kullanir.
        var routingKey = GetRoutingKey(args);
        // Gonderilecek log mesajini komut satirindan veya varsayilan metinden alir.
        var message = GetMessage(args);
        // Metni RabbitMQ'nun kullanacagi byte dizisine cevirir.
        var body = Encoding.UTF8.GetBytes(message);

        // Mesaji belirtilen routing key ile topic exchange'e yollar.
        await channel.BasicPublishAsync(
            exchange: "topic-logs",
            routingKey: routingKey,
            body: body);

        // Gonderilen routing key ve mesaji ekrana yazar.
        Console.WriteLine($" [x] Sent '{routingKey}':'{message}'");
        // Uygulamayi kapatmak icin tekrar Enter bekledigini ekrana yazar.
        Console.WriteLine(" Press [enter] to exit.");
        // Kullanici Enter tusuna basana kadar uygulamayi acik tutar.
        Console.ReadLine();
    }

    // Komut satirindan routing key bilgisini okur.
    static string GetRoutingKey(string[] args)
    {
        // Ilk parametre varsa onu kullanir, yoksa varsayilan anahtari dondurur.
        return args.Length > 0 ? args[0] : "anonymous.info";
    }

    // Komut satirindan gelen metni tek bir mesaj haline getirir.
    static string GetMessage(string[] args)
    {
        // Ilk parametreden sonraki metni birlestirir, yoksa varsayilan mesaji kullanir.
        return args.Length > 1 ? string.Join(" ", args.Skip(1)) : "Hello Topic!";
    }
}
