using KafkaFlow.TypedHandler;
using KafkaFlow;
using Microsoft.Extensions.DependencyInjection;
using KafkaFlow.Serializer;

Console.WriteLine(".:: Kafka Playground - Kafka Flow Consumer ::.");

const string TopicName = "kafka-flow-playground";

//var cancellationToken = new CancellationTokenSource();
//Console.CancelKeyPress += (_, e) =>
//{
//    Console.WriteLine("Desligando consumer...");
//    e.Cancel = true;
//    cancellationToken.Cancel();
//};

// Não funcionou. O handler não é executado, mas da para perceber
var services = new ServiceCollection();
services.AddKafka(kafka => kafka
    .UseConsoleLog()
    .AddCluster(cluster => cluster
        .WithBrokers(new[] { "localhost:9092" })
        .AddConsumer(consumer => consumer
            .Topic(TopicName)
            .WithGroupId("dotnet-playground")
            .WithAutoOffsetReset(AutoOffsetReset.Earliest)
            .WithBufferSize(200)
            .WithWorkersCount(1)
            //.WithConsumerConfig(new Confluent.Kafka.ConsumerConfig {  })
            .AddMiddlewares(middleware => middleware
                .AddSerializer<JsonCoreSerializer>()
                .AddTypedHandlers(h => h.AddHandler<HelloMessageHandler>())))));

var serviceProvider = services.BuildServiceProvider();

var bus = serviceProvider.CreateKafkaBus();
await bus.StartAsync();

//cancellationToken.Token.WaitHandle.WaitOne();
Console.ReadKey();

await bus.StopAsync();

public class HelloMessage
{
    public string Text { get; set; } = default!;
};

public class HelloMessageHandler : IMessageHandler<HelloMessage>
{
    public Task Handle(IMessageContext context, HelloMessage message)
    {
        Console.WriteLine(
            "Partition: {0} | Offset: {1} | Message: {2}",
            context.ConsumerContext.Partition,
            context.ConsumerContext.Offset,
            message.Text);

        return Task.CompletedTask;
    }
}