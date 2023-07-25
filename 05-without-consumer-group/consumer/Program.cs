using Confluent.Kafka;

Console.WriteLine(".:: Kafka Playground - Without Consumer Group (Consumer) ::.");

// Configurações:
// 1. Consumer não deve ter group id e não será utilizado o método de "subscribe"
// 2. Não realizar subscribe do consumer no tópico
// 3. Realizar assignment manual no tópico/partição
// 4. Auto commit deve estar desabilitado
// Pontos de atenção:
// - Sem consumer group não há lag para observabilidade pelo broker e você precisará ter mecanismos a parte para ter visão de atrasos

// .NET NÃO SUPORTA RECURSO!!!
// Library .net é um wrapper da library "librdkafka" e ela não suporta consumo sem group.id

const string TopicName = "without-consumer-group-playground";

var cancellationToken = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    Console.WriteLine("Desligando consumer...");
    e.Cancel = true;
    cancellationToken.Cancel();
};

// Iniciar consumidor
Task.WaitAll(StartConsumerTask(1, cancellationToken.Token));
//Task.WaitAll(StartConsumerTask(1, cancellationToken.Token), StartConsumerTask(2, cancellationToken.Token));

Console.WriteLine("Fim");


Task StartConsumerTask(int index, CancellationToken cancellationToken) => Task.Run(() =>
{

    var consumerConfig = new ConsumerConfig()
    {
        BootstrapServers = "localhost:9092",
        GroupId = "dotnet-playground",
        GroupInstanceId = index.ToString(), // Para evitar rebalancing. Se consumidor reconectar com mesmo Id, será atribuida a mesma partição sem esperar o tempo de expiração da sessão
        ClientId = "dotnet-playground",
        AutoOffsetReset = AutoOffsetReset.Earliest,
        PartitionAssignmentStrategy = null,

        EnableAutoCommit = false, // Sem auto commit
        EnableAutoOffsetStore = false,
        AutoCommitIntervalMs = null,

        HeartbeatIntervalMs = 3_000,
        SessionTimeoutMs = 10_000,
        MaxPollIntervalMs = 300_000,

        // --- Testes para incrementar performance ao máximo - 1 milhão de msg/seg
        MaxPartitionFetchBytes = 10485760, // 10 mb
        FetchMinBytes = 10485760, // 10 mb
        FetchMaxBytes = 52428800,
        FetchWaitMaxMs = 3_000,

        QueuedMinMessages = 3_000_000,
        QueuedMaxMessagesKbytes = 2097151,

        // -- Outras
        //EnablePartitionEof = true,
        //ReceiveMessageMaxBytes = 100000000,
        //MessageMaxBytes = 10485760, // 10 mb
    };
    using var consumer = new ConsumerBuilder<int, string>(consumerConfig).Build();
    consumer.Subscribe(TopicName);
    //consumer.Assign()

    var count = 0;
    while (!cancellationToken.IsCancellationRequested)
    {
        try
        {
            var result = consumer.Consume(cancellationToken);
            Console.WriteLine($"[Task {index}] {result.Message.Key}: {result.Message.Value}");
            count++;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"[Task {index}] ### Execução cancelada ###");
            consumer.Close();
        }
    }

    Console.WriteLine($"{count:N0} mensagens processadas");
});