using Confluent.Kafka;
using Confluent.SchemaRegistry.Serdes;
using Confluent.SchemaRegistry;
using System.Diagnostics;
using KafkaPlayground.Avros;
using Confluent.Kafka.SyncOverAsync;

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
                //consumer.Commit(result);
            }
            if (count >= 10_000_000)
            {
                time.Stop();
                Console.WriteLine($"Concluído em {time.Elapsed}"); // 00:00:22.4117550
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"[Task {index}] ### Execução cancelada ###");
            consumer.Close();
        }
    }
});