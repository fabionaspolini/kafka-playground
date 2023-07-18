﻿using Confluent.Kafka;
using System.Diagnostics;

Console.WriteLine(".:: Kafka Playground - Basic Consumer ::.");
const string TopicName = "simple-playground";

var cancellationToken = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    Console.WriteLine("Desligando consumer...");
    e.Cancel = true;
    cancellationToken.Cancel();
};

Task.WaitAll(
    StartConsumerTask(1, cancellationToken.Token)
    //StartConsumerTask(2, cancellationToken.Token)
);

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

        // -- Outras
        //ReceiveMessageMaxBytes = 100000000,
        //MessageMaxBytes = 10485760, // 10 mb
    };
    //consumerConfig.Set("socket.blocking.max.ms", "1");
    //consumerConfig.Set("max.poll.records", "10000"); // somente java
    var consumer = new ConsumerBuilder<int, string>(consumerConfig).Build();
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
                //consumer.Commit(result);
            }
            if (count >= 10_000_000)
            {
                time.Stop();
                Console.WriteLine($"Concluído em {time.Elapsed}"); // 00:00:09.9888885
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"[Task {index}] ### Execução cancelada ###");
            consumer.Close();
        }
    }
});