// RabbitMQ ile baglanti kurmak ve kuyruk islemleri yapmak icin gerekli siniflari ekler.
using RabbitMQ.Client;
// RabbitMQ'dan gelen mesajlari olay mantigiyla dinlemek icin gerekli siniflari ekler.
using RabbitMQ.Client.Events;
// Gelen byte verisini metne cevirmek icin gerekli sinifi ekler.
using System.Text;

// Bu dosyanin ait oldugu isim alanini tanimlar.
namespace HelloWord.Receive;

// Uygulamanin ana sinifini tanimlar.
internal class Program
{
    // Uygulama basladiginda ilk olarak bu metot calisir.
    static void Main(string[] args)
    {
        // RabbitMQ baglanti ayarlarini olusturur ve host bilgisini verir.
        var factory = new ConnectionFactory() { HostName = "localhost" };
        // RabbitMQ sunucusunun port numarasini belirler.
        factory.Port = 5672;
        // RabbitMQ kullanici adini belirler.
        factory.UserName = "guest";
        // RabbitMQ sifresini belirler.
        factory.Password = "guest";

        // RabbitMQ sunucusuna baglanti olusturur.
        var connection = factory.CreateConnection();
        // Baglanti uzerinden mesaj alma ve dinleme islemleri icin kanal olusturur.
        var channel = connection.CreateModel();

        // "hello" adinda bir kuyruk olusturur; varsa mevcut kuyrugu kullanir.
        channel.QueueDeclare(
            // Kullanilacak kuyrugun adini belirtir.
            queue: "hello",
            // Kuyrugun kalici olup olmayacagini belirler.
            durable: false,
            // Kuyrugun sadece bu baglantiya ozel olup olmayacagini belirler.
            exclusive: false,
            // Son baglanti kapaninca kuyrugun otomatik silinip silinmeyecegini belirler.
            autoDelete: false,
            // Kuyruk icin ek ayar verilmedigini belirtir.
            arguments: null
        );

        // Kullaniciya artik mesaj beklenmeye baslandigini bildirir.
        Console.WriteLine(" [*] Waiting for messages.");

        // Kanala bagli bir consumer nesnesi olusturur.
        var consumer = new EventingBasicConsumer(channel);
        // Kuyruktan mesaj geldiginde calisacak olayi tanimlar.
        consumer.Received += (model, ea) =>
        {
            // Gelen mesajin govdesini byte dizisi olarak alir.
            var body = ea.Body.ToArray();
            // Byte dizisini UTF-8 metnine cevirir.
            var message = Encoding.UTF8.GetString(body);
            // Alinan mesaji ekrana yazar.
            Console.WriteLine($" [x] Received {message}");
        };

        // Belirtilen kuyrugu dinlemeye baslar.
        channel.BasicConsume(
            // Dinlenecek kuyrugun adini belirtir.
            queue: "hello",
            // Mesajlarin otomatik onaylanacagini belirtir.
            autoAck: true,
            // Mesajlari isleyecek consumer nesnesini verir.
            consumer: consumer
        );

        // Uygulamayi kapatmak icin Enter tusuna basilmasini ister.
        Console.WriteLine(" Press [enter] to exit.");
        // Kullanici Enter tusuna basana kadar uygulamayi acik tutar.
        Console.ReadLine();
    }
}
