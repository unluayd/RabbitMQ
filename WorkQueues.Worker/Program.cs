// RabbitMQ ile calismak icin gerekli siniflari projeye dahil eder.
using RabbitMQ.Client;
// RabbitMQ'dan gelen mesajlari asenkron olarak dinlemek icin gerekli sinifi ekler.
using RabbitMQ.Client.Events;
// Gelen byte verisini metne cevirmek icin gerekli sinifi projeye dahil eder.
using System.Text;

// Bu dosyanin ait oldugu isim alanini tanimlar.
namespace WorkQueues.Worker;

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
        // Mesajlari tuketmek icin baglanti uzerinden bir kanal olusturur.
        await using var channel = await connection.CreateChannelAsync();

        // "task_queue" adinda bir kuyruk olusturur; varsa mevcut kuyrugu kullanir.
        await channel.QueueDeclareAsync(
            // Kullanilacak kuyrugun adini belirtir.
            queue: "task_queue",
            // Kuyrugun kalici olup olmayacagini belirler.
            durable: true,
            // Kuyrugun sadece bu baglantiya ozel olup olmayacagini belirler.
            exclusive: false,
            // Kullanim bitince kuyrugun otomatik silinip silinmeyecegini belirler.
            autoDelete: false,
            // Kuyruk icin ek ayar verilmedigini belirtir.
            arguments: null);

        // Aynı anda bir mesajin islenmesini isteyerek adil dagitim saglar.
        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

        // Kullaniciya artik mesaj beklenmeye baslandigini bildirir.
        Console.WriteLine(" [*] Waiting for messages.");

        // Kanala bagli asenkron bir consumer nesnesi olusturur.
        var consumer = new AsyncEventingBasicConsumer(channel);
        // Kuyruktan mesaj geldiginde calisacak olayi tanimlar.
        consumer.ReceivedAsync += async (_, ea) =>
        {
            // Gelen mesajin govdesini byte dizisi olarak alir.
            var body = ea.Body.ToArray();
            // Byte dizisini UTF-8 metnine cevirir.
            var message = Encoding.UTF8.GetString(body);
            // Alinan mesaji ekrana yazar.
            Console.WriteLine($" [x] Received {message}");

            // Mesajdaki nokta sayisina gore yapay islem suresi belirler.
            var dots = message.Split('.').Length - 1;
            // Her nokta icin 1 saniye bekleyerek is yukunu taklit eder.
            await Task.Delay(dots * 1000);

            // Mesajin islendiginin tamamlandigini ekrana yazar.
            Console.WriteLine(" [x] Done");
            // Mesajin basariyla islendigi bilgisini RabbitMQ'ya gonderir.
            await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
        };

        // Belirtilen kuyrugu dinlemeye baslar.
        await channel.BasicConsumeAsync(
            // Dinlenecek kuyrugun adini belirtir.
            queue: "task_queue",
            // Mesajlarin otomatik degil, manuel onaylanacagini belirtir.
            autoAck: false,
            // Mesajlari isleyecek consumer nesnesini verir.
            consumer: consumer);

        // Uygulamayi kapatmak icin Enter tusuna basilmasini ister.
        Console.WriteLine(" Press [enter] to exit.");
        // Kullanici Enter tusuna basana kadar uygulamayi acik tutar.
        Console.ReadLine();
    }
}
