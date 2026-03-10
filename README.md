# RabbitMQ Egitim Projeleri

Bu repo, RabbitMQ egitimi boyunca olusturulan .NET console uygulamalarini icerir.

## Gereksinimler

- .NET 8 SDK
- RabbitMQ Server
- Varsayilan baglanti bilgileri:
  - Host: `localhost`
  - Port: `5672`
  - User: `guest`
  - Password: `guest`

## Projeler

### Hello World

- `HelloWord.Send`
  - Temel mesaj gonderme ornegi
- `HelloWord.Receive`
  - Temel mesaj dinleme ornegi

### Work Queues

- `WorkQueues.NewTask`
  - `task_queue` kuyruguna kalici mesaj gonderir
- `WorkQueues.Worker`
  - `task_queue` kuyrugundan mesaj tuketir
  - `manual ack` ve `prefetch` kullanir

### Publish / Subscribe

- `PublishSubscribe/PublishSubscribe.EmitLog`
  - `logs` isimli `fanout` exchange'e mesaj yollar
- `PublishSubscribe/PublishSubscibe.ReceiveLogs`
  - `logs` exchange'inden gelen mesajlari dinler
- `PublishSubscribe/PublishSubscribe.ReceiveSms`
  - `logs` exchange'inden gelen mesajlari SMS senaryosu gibi dinler

Not: `PublishSubscribe/PublishSubscibe.ReceiveLogs` klasor adinda yazim farki vardir. Proje bu isimle calisir.

### Routing

- `Routing/Routing.EmitLog`
  - `direct-logs` exchange'ine severity bilgisi ile mesaj yollar
- `Routing/Routing.Receive`
  - Belirli severity degerlerine gore mesaj dinler

### Topic

- `Topic/Topic.EmitLog`
  - `topic-logs` exchange'ine routing key ile mesaj yollar
- `Topic/Topic.ReceiveLogs`
  - Binding key desenlerine gore mesaj dinler

### Properties

- `Properties/Properties.Publisher`
  - Queue arguments ve message properties kullanarak mesaj yollar
- `Properties/Properties.Consumer`
  - Mesaj icerigini ve property bilgilerini ekrana yazar

## Calistirma Ornekleri

### Hello World

```bash
cd HelloWord.Receive
dotnet run
```

```bash
cd HelloWord.Send
dotnet run
```

### Work Queues

```bash
cd WorkQueues.Worker
dotnet run
```

```bash
cd WorkQueues.NewTask
dotnet run "Task mesaji..."
```

### Publish / Subscribe

```bash
cd PublishSubscribe/PublishSubscibe.ReceiveLogs
dotnet run
```

```bash
cd PublishSubscribe/PublishSubscribe.ReceiveSms
dotnet run
```

```bash
cd PublishSubscribe/PublishSubscribe.EmitLog
dotnet run
```

### Routing

```bash
cd Routing/Routing.Receive
dotnet run info warning error
```

```bash
cd Routing/Routing.EmitLog
dotnet run error "Sistem hatasi olustu"
```

### Topic

```bash
cd Topic/Topic.ReceiveLogs
dotnet run "#.info" "*.error"
```

```bash
cd Topic/Topic.EmitLog
dotnet run kern.error "Disk dolmak uzere"
```

### Properties

```bash
cd Properties/Properties.Consumer
dotnet run
```

```bash
cd Properties/Properties.Publisher
dotnet run
```

## Notlar

- Projelerdeki `Program.cs` dosyalarina Turkce aciklama satirlari eklenmistir.
- Projeler RabbitMQ.Client `7.2.1` ile uyumlu hale getirilmistir.
- Bazi orneklerde kuyruk, exchange, routing key ve message property kullanimlari egitim amacli olarak gosterilmistir.