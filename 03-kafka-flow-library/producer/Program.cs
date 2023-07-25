using KafkaFlow;
using KafkaFlow.Producers;
using KafkaFlow.Serializer;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

Console.WriteLine(".:: Kafka Playground - Kafka Flow Producer ::.");

const string TopicName = "kafka-flow-playground";
const int Messages = 1_000;

var serializerOptions = new JsonSerializerOptions
{
    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
};

var services = new ServiceCollection();
services.AddKafka(kafka => kafka
    .UseConsoleLog()
    .AddCluster(cluster => cluster
        .WithBrokers(new[] { "localhost:9092" })
        .CreateTopicIfNotExists(TopicName, 10, 1)
        .AddProducer(
            name: "my-producer",
            producer => producer
                .DefaultTopic(TopicName)
                .AddMiddlewares(m => m.AddSerializer(x => new JsonCoreSerializer(serializerOptions))))));
var serviceProvider = services.BuildServiceProvider();

var producer = serviceProvider.GetRequiredService<IProducerAccessor>().GetProducer("my-producer");

// Serializador não utiliza corretamente configurações personalizadas e não são gerados os caracteres especiais legíveis
// Issue: https://github.com/Farfetch/kafkaflow/issues/414
//var message = new HelloMessage("Olá!!!");

for (int i = 1; i <= Messages; i++)
    producer.Produce(i.ToString(), new HelloMessage($"Ola {i}!!!"));

Thread.Sleep(1000);
Console.WriteLine("Fim");

record class HelloMessage(string Text);