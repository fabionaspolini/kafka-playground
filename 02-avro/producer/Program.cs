using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using playground.kafka;

Console.WriteLine(".:: Kafka Playground - Avro Producer ::.");
const string TopicName = "avro-playground";
const int Messages = 50_000_000;

// Setup - Criar tópico se não existir
var adminConfig = new AdminClientConfig
{
    BootstrapServers = "localhost:9092"
};
using var admin = new AdminClientBuilder(adminConfig).Build();
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
var schemaRegistryConfig = new SchemaRegistryConfig { Url = "http://localhost:8081" };

var avroSerializerConfig = new AvroSerializerConfig
{
    BufferBytes = 1024,
    SubjectNameStrategy = SubjectNameStrategy.Record
};

var producerConfig = new ProducerConfig
{
    BootstrapServers = "localhost:9092",
    CompressionType = CompressionType.Snappy,
    QueueBufferingMaxMessages = 1_000_000, // Tamanho da fila in memory
};

using var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);
var avroSerializer = new AvroSerializer<Pessoa>(schemaRegistry, avroSerializerConfig);
using var producer = new ProducerBuilder<int, Pessoa>(producerConfig)
    .SetValueSerializer(avroSerializer.AsSyncOverAsync())
    .Build();

for (int i = 1; i <= Messages; i++)
{
    var model = new Pessoa
    {
        id = i.ToString(),
        nome = $"Fulano {i}"
    };
    producer.Produce(TopicName, new Message<int, Pessoa> { Key = i, Value = model });
    if (i % producerConfig.QueueBufferingMaxMessages == 0)
    {
        Console.WriteLine($"Flushing {i:N0}...");
        producer.Flush();
    }
}

producer.Flush();
Console.WriteLine("Fim");