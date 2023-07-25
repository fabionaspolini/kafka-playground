using Confluent.Kafka;
using Confluent.Kafka.Admin;

Console.WriteLine(".:: Kafka Playground - Without Consumer Group (Producer) ::.");

const string TopicName = "without-consumer-group-playground";
const int Messages = 30;

// Setup - Criar tópico se não existir
var adminConfig = new AdminClientConfig
{
    BootstrapServers = "localhost:9092"
};
using var admin = new AdminClientBuilder(adminConfig).Build();
await admin.DeleteTopicsAsync(new[] { TopicName });
Thread.Sleep(2000);
var topicMetadata = admin.GetMetadata(TimeSpan.FromSeconds(10));
if (!topicMetadata.Topics.Any(x => x.Error.Code == ErrorCode.NoError && x.Topic == TopicName))
    await admin.CreateTopicsAsync(new[]
    {
        new TopicSpecification
        {
            Name = TopicName,
            NumPartitions = 5,
            Configs = new()
            {
                { "cleanup.policy", "compact" },
                { "segment.ms", "100" },
                { "min.cleanable.dirty.ratio", "0.01" },
            }
        }
    });

// Produzir mensagens
var producerConfig = new ProducerConfig
{
    BootstrapServers = "localhost:9092",
    CompressionType = CompressionType.Snappy,
    QueueBufferingMaxMessages = 1_000_000, // Tamanho da fila in memory
};
using var producer = new ProducerBuilder<int, string>(producerConfig).Build();

for (int i = 1; i <= Messages; i++)
{
    producer.Produce(TopicName, new Message<int, string> { Key = i, Value = $"Msg {i} - {DateTime.Now:O}" });
    //if (i % producerConfig.QueueBufferingMaxMessages == 0)
    {
        Console.WriteLine($"Flushing {i:N0}...");
        producer.Flush();
    }

    Thread.Sleep(150); // Para ser maior que o tempo de vida do segmento e forçar que mais mensagens fiquem elegíveis a compactação no teste
}

producer.Flush();
Console.WriteLine($"Fim - {Messages:N0} mensagens publicadas");