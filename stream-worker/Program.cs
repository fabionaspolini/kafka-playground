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
    ApplicationId = "playground-stream-sample-7",
};

var streamBuilder = new StreamBuilder();
var kstream = streamBuilder.Stream<string, string>("playground.transacional.venda");
var ktable = kstream
    .FlatMapValues(x => x + ",TESTE");
ktable.To("playground.analitico.venda");

// kstream.Repartition(Repartitioned<string, string>.NumberOfPartitions(3));
var topology = streamBuilder.Build();
var stream = new KafkaStream(topology, streamConfig);
Console.CancelKeyPress += (_, _) => stream.Dispose();


await stream.StartAsync();


Console.WriteLine("Fim");