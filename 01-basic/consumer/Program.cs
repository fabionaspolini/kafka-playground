using Confluent.Kafka;
using System.Diagnostics;

Console.WriteLine(".:: Kafka Playground - Basic Consumer ::.");

const string TopicName = "basic-playground";

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
        PartitionAssignmentStrategy = PartitionAssignmentStrategy.CooperativeSticky,

        EnableAutoCommit = true,
        AutoCommitIntervalMs = 5_000,

        HeartbeatIntervalMs = 3_000, // Enviar sinal de vida a cada 3 segundos para o broker.
        SessionTimeoutMs = 10_000, // Se o broker ficar 10 segundos sem receber sinal de vida, irá remover consumidor do grupo e rebalancer.
        MaxPollIntervalMs = 300_000, // Tempo máximo entre chamadas do método "consume()", em outras palavras é o tempo para processar a mensagem. Caso exceder, o broker irá remover o consumidor do grupo e rebalancer a partição.

        // --- Testes para incrementar performance ao máximo - 1 milhão de msg/seg
        MaxPartitionFetchBytes = 10485760, // 10 mb
        FetchMinBytes = 10485760, // 10 mb
        FetchMaxBytes = 52428800,
        FetchWaitMaxMs = 3_000,

        QueuedMinMessages = 3_000_000,
        QueuedMaxMessagesKbytes = 2097151,

        // -- Outras
        //EnablePartitionEof = true, // Consumidor pode verificar "result.IsPartitionEOF" para saber que partição chegou ao fim. Não é o tópico que chegou no fim, apenas a partição
        //ReceiveMessageMaxBytes = 100000000,
        //MessageMaxBytes = 10485760, // 10 mb
    };

    using var consumer = new ConsumerBuilder<int, string>(consumerConfig).Build();
    consumer.Subscribe(TopicName);
    //consumer.Assign()

    var count = 0;
    var time = Stopwatch.StartNew();
    while (!cancellationToken.IsCancellationRequested)
    {
        try
        {
            var result = consumer.Consume(cancellationToken);
            //Console.WriteLine($"[Task {index}] {result.Message.Key}: {result.Message.Value}");
            count++;
            if (count % 100_000 == 0)
            {
                Console.WriteLine($"[Task {index}] Count: {count:N0} - {result.Message.Key}: {result.Message.Value}");
                //consumer.Commit(result); // Auto commit habilitado. Analise os possíveis casos de exceptions no seu caso de uso para decidir sobre isso!
            }
            if (count >= 10_000_000)
            //if (result.IsPartitionEOF)
            {
                time.Stop();

                // 10 milhões: Concluído em 00:00:10.9476809
                // 50 milhões: Concluído em 00:00:42.3009102
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