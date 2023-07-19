using Confluent.Kafka;
using Confluent.Kafka.Admin;

Console.WriteLine(".:: Kafka Playground - Basic Producer ::.");
const string TopicName = "basic-playground";
const int Messages = 10_000_000;

// Setup - Criar tópico se não existir
var adminConfig = new AdminClientConfig
{
    BootstrapServers = "localhost:9092"
};
var admin = new AdminClientBuilder(adminConfig).Build();
//await admin.DeleteTopicsAsync(new[] { TopicName });
//Thread.Sleep(2000);
var topicMetadata = admin.GetMetadata(TimeSpan.FromSeconds(10));
if (!topicMetadata.Topics.Any(x => x.Error.Code == ErrorCode.NoError && x.Topic == TopicName))
    await admin.CreateTopicsAsync(new[]
    {
        new TopicSpecification
        {
            Name = TopicName,
            NumPartitions = 10
        }
    });

// Produzir mensagens
var producerConfig = new ProducerConfig
{
    BootstrapServers = "localhost:9092",
    CompressionType = CompressionType.Snappy,
    QueueBufferingMaxMessages = 1_000_000, // Tamanho da fila in memory
};
var producer = new ProducerBuilder<int, string>(producerConfig).Build();

for (int i = 1; i <= Messages; i++)
{
    producer.Produce(TopicName, new Message<int, string> { Key = i, Value = $"Msg {i}" });
    if (i % producerConfig.QueueBufferingMaxMessages == 0)
    {
        Console.WriteLine($"Flushing {i:N0}...");
        producer.Flush();
    }
}

producer.Flush();
Console.WriteLine("Fim");