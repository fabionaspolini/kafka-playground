using Confluent.Kafka;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.Stream;
using Streamiz.Kafka.Net.Table;

Console.WriteLine(".:: Kafka Streams Playground - Worker ::.");

// STERAM
var streamConfig = new StreamConfig<StringSerDes, StringSerDes>()
{
    BootstrapServers = "localhost:9092",
    ApplicationId = "playground-stream-sample-6"
};

var streamBuilder = new StreamBuilder();
var kstream = streamBuilder.Stream<string, string>("playground.transacional.venda");
var ktable = streamBuilder.Table("playground.cadastros.cliente", InMemory.As<string, string>("teste-store"));
// kstream.To("playground.analitico.venda");
kstream
    .Join(ktable, (v, v1) => $"{v}-{v1}")
    .To("playground.analitico.venda");

// kstream.Repartition(Repartitioned<string, string>.NumberOfPartitions(3));
var topology = streamBuilder.Build();

var stream = new KafkaStream(topology, streamConfig);
Console.CancelKeyPress += (_, _) => stream.Dispose();


await stream.StartAsync();


Console.WriteLine("Fim");