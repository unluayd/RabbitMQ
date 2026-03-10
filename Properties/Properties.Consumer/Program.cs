// RabbitMQ ile calismak icin gerekli siniflari projeye dahil eder.
using RabbitMQ.Client;
// RabbitMQ'dan gelen mesajlari asenkron olarak dinlemek icin gerekli sinifi ekler.
using RabbitMQ.Client.Events;
// Gelen byte verisini metne cevirmek icin gerekli sinifi projeye dahil eder.
using System.Text;

// Bu dosyanin ait oldugu isim alanini tanimlar.
namespace Properties.Consumer;

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

        // Publisher tarafindakiyle ayni kuyruk ozelliklerini tanimlar.
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

        // Kullaniciya artik mesaj beklenmeye baslandigini bildirir.
        Console.WriteLine(" [*] Waiting for property messages.");

        // Kanala bagli asenkron bir consumer nesnesi olusturur.
        var consumer = new AsyncEventingBasicConsumer(channel);
        // Kuyruktan mesaj geldiginde calisacak olayi tanimlar.
        consumer.ReceivedAsync += async (_, ea) =>
        {
            // Gelen mesajin govdesini byte dizisi olarak alir.
            var body = ea.Body.ToArray();
            // Byte dizisini UTF-8 metnine cevirir.
            var message = Encoding.UTF8.GetString(body);
            // Mesajin icerigini ekrana yazar.
            Console.WriteLine($" [x] Message: {message}");

            // Mesajla birlikte gelen temel property alanlarini ekrana yazar.
            Console.WriteLine($" Persistent: {ea.BasicProperties.Persistent}");
            Console.WriteLine($" ContentType: {ea.BasicProperties.ContentType}");
            Console.WriteLine($" ContentEncoding: {ea.BasicProperties.ContentEncoding}");
            Console.WriteLine($" Priority: {ea.BasicProperties.Priority}");
            Console.WriteLine($" CorrelationId: {ea.BasicProperties.CorrelationId}");
            Console.WriteLine($" ReplyTo: {ea.BasicProperties.ReplyTo}");
            Console.WriteLine($" Expiration: {ea.BasicProperties.Expiration}");
            Console.WriteLine($" MessageId: {ea.BasicProperties.MessageId}");
            Console.WriteLine($" Type: {ea.BasicProperties.Type}");
            Console.WriteLine($" UserId: {ea.BasicProperties.UserId}");
            Console.WriteLine($" AppId: {ea.BasicProperties.AppId}");
            Console.WriteLine();

            await Task.CompletedTask;
        };

        // Kuyrugu dinlemeye baslar.
        await channel.BasicConsumeAsync(
            // Dinlenecek kuyrugun adini belirtir.
            queue: "hello",
            // Mesajlarin otomatik onaylanacagini belirtir.
            autoAck: true,
            // Mesajlari isleyecek consumer nesnesini verir.
            consumer: consumer);

        // Uygulamayi kapatmak icin Enter tusuna basilmasini ister.
        Console.WriteLine(" Press [enter] to exit.");
        // Kullanici Enter tusuna basana kadar uygulamayi acik tutar.
        Console.ReadLine();
    }
}
