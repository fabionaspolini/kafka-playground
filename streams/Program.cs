using Confluent.Kafka;

Console.WriteLine(".:: Kafka Streams Playground ::.");

// Produzir mensagens
var producerConfig = new ProducerConfig
{
    BootstrapServers = "localhost:9092",
};
var producer = new ProducerBuilder<string, string>(producerConfig).Build();

producer.Produce("playground.cadastros.cliente", new() { Key = "fábio", Value = "nome=Fábio,idade=30" });
producer.Produce("playground.cadastros.cliente", new() { Key = "joão", Value = "nome=João,idade=20" });
producer.Produce("playground.cadastros.cliente", new() { Key = "pedro", Value = "nome=Pedro,idade=15" });

producer.Produce("playground.cadastros.cliente", new() { Key = "fábio", Value = "nome=Fábio,idade=30" });
producer.Produce("playground.cadastros.cliente", new() { Key = "joão", Value = "nome=João,idade=20" });
producer.Produce("playground.cadastros.cliente", new() { Key = "pedro", Value = "nome=Pedro,idade=15" });

// producer.Produce

producer.Flush();

Console.WriteLine("Fim");