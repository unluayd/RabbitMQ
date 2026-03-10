// RabbitMQ ile calismak icin gerekli siniflari projeye dahil eder.
using RabbitMQ.Client;
// RabbitMQ'dan gelen mesajlari asenkron olarak dinlemek icin gerekli sinifi ekler.
using RabbitMQ.Client.Events;
// Gelen byte verisini metne cevirmek icin gerekli sinifi projeye dahil eder.
using System.Text;

// Bu dosyanin ait oldugu isim alanini tanimlar.
namespace Topic.ReceiveLogs;

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
        // Mesajlari dinlemek icin baglanti uzerinden bir kanal olusturur.
        await using var channel = await connection.CreateChannelAsync();

        // Topic tipinde "topic-logs" adinda bir exchange olusturur; varsa mevcut exchange'i kullanir.
        await channel.ExchangeDeclareAsync(
            exchange: "topic-logs",
            type: ExchangeType.Topic);

        // Sunucu tarafinda gecici ve benzersiz isimli bir kuyruk olusturur.
        var queue = await channel.QueueDeclareAsync();
        // Olusan kuyrugun adini bir degiskende saklar.
        var queueName = queue.QueueName;

        // Komut satirindan binding key degerlerini alir; gelmezse varsayilan filtreleri kullanir.
        var bindingKeys = GetBindingKeys(args);

        // Her bir binding key icin kuyrugu exchange'e baglar.
        foreach (var bindingKey in bindingKeys)
        {
            await channel.QueueBindAsync(
                queue: queueName,
                exchange: "topic-logs",
                routingKey: bindingKey);
        }

        // Hangi desenlerin dinlendigini ekrana yazar.
        Console.WriteLine($" [*] Waiting for topic logs: {string.Join(", ", bindingKeys)}");

        // Kanala bagli asenkron bir consumer nesnesi olusturur.
        var consumer = new AsyncEventingBasicConsumer(channel);
        // Exchange'den gelen mesaj geldiginde calisacak olayi tanimlar.
        consumer.ReceivedAsync += async (_, ea) =>
        {
            // Gelen mesajin govdesini byte dizisi olarak alir.
            var body = ea.Body.ToArray();
            // Byte dizisini UTF-8 metnine cevirir.
            var message = Encoding.UTF8.GetString(body);
            // Alinan routing key bilgisini ve mesaji ekrana yazar.
            Console.WriteLine($" [x] Received '{ea.RoutingKey}':'{message}'");
            await Task.CompletedTask;
        };

        // Kuyrugu dinlemeye baslar.
        await channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: true,
            consumer: consumer);

        // Uygulamayi kapatmak icin Enter tusuna basilmasini ister.
        Console.WriteLine(" Press [enter] to exit.");
        // Kullanici Enter tusuna basana kadar uygulamayi acik tutar.
        Console.ReadLine();
    }

    // Komut satirindan binding key degerlerini okur.
    static string[] GetBindingKeys(string[] args)
    {
        // Parametre geldiyse hepsini kullanir, yoksa varsayilan topic desenlerini dondurur.
        return args.Length > 0 ? args : ["#.info", "*.error"];
    }
}
