using Confluent.Kafka;
using Confluent.SchemaRegistry.Serdes;
using Confluent.SchemaRegistry;
using System.Diagnostics;
using Confluent.Kafka.SyncOverAsync;
using playground.kafka;

Console.WriteLine(".:: Kafka Playground - Avro Consumer ::.");

const string TopicName = "avro-playground";

var cancellationToken = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    Console.WriteLine("Desligando consumer...");
    e.Cancel = true;
    cancellationToken.Cancel();
};

// Configurações singletons
var schemaRegistryConfig = new SchemaRegistryConfig { Url = "http://localhost:8081" };

using var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);
var avroDeserializer = new AvroDeserializer<Pessoa>(schemaRegistry);

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
        PartitionAssignmentStrategy = PartitionAssignmentStrategy.CooperativeSticky,

        EnableAutoCommit = true,
        AutoCommitIntervalMs = 5_000,

        HeartbeatIntervalMs = 3_000,
        SessionTimeoutMs = 10_000,
        MaxPollIntervalMs = 300_000,

        // --- Testes para incrementar performance ao máximo - 1 milhão de msg/seg
        //MaxPartitionFetchBytes = 10485760, // 10 mb
        //FetchMinBytes = 10485760, // 10 mb
        //FetchMaxBytes = 52428800,
        //FetchWaitMaxMs = 3_000,

        //QueuedMinMessages = 3_000_000,
        //QueuedMaxMessagesKbytes = 2097151,
    };
    using var consumer = new ConsumerBuilder<int, Pessoa>(consumerConfig)
        .SetValueDeserializer(avroDeserializer.AsSyncOverAsync())
        .Build();
    consumer.Subscribe(TopicName);

    var count = 0;
    var time = Stopwatch.StartNew();
    while (!cancellationToken.IsCancellationRequested)
    {
        try
        {
            var result = consumer.Consume(cancellationToken);
            //Console.WriteLine($"[Task {index}] {result.Message.Key}: {result.Message.Value.nome}");
            count++;
            if (count % 100_000 == 0)
            {
                Console.WriteLine($"[Task {index}] Count: {count:N0} - {result.Message.Key}: {result.Message.Value.nome}");
                //consumer.Commit(result); // Auto commit habilitado. Analise os possíveis casos de exceptions no seu caso de uso para decidir sobre isso!
            }
            if (count >= 10_000_000)
            {
                time.Stop();

                // Sem incremento de performance
                // 10 milhões: Concluído em 00:00:21.8974886
                // 50 milhões: Concluído em 00:01:43.1019030

                // Com incremento de performance
                // 10 milhões: Concluído em 00:00:21.0839030
                // 50 milhões: Concluído em 00:01:34.9580854
                Console.WriteLine($"Concluído em {time.Elapsed}");
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"[Task {index}] ### Execução cancelada ###");
            consumer.Close();
        }
    }
});