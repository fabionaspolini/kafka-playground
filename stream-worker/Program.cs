using Confluent.Kafka;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.Table;

Console.WriteLine(".:: Kafka Streams Playground - Worker ::.");

// STERAM
var config = new StreamConfig<StringSerDes, StringSerDes>()
{
    BootstrapServers = "localhost:9092",
    ApplicationId = "playground-stream-sample-22",
    AutoOffsetReset = AutoOffsetReset.Earliest,
    // CommitIntervalMs = 500
};

var builder = new StreamBuilder();

var clienteTable = builder.Table("playground.cadastros.cliente", InMemory.As<string, string>("cliente-store"));
// var produtoTable = builder.Table("playground.cadastros.produto", InMemory.As<string, string>("produto-store"));

builder
    .Stream<string, string>("playground.transacional.venda")
    // .Join(clienteTable, (venda, cliente) => $"{venda} => {cliente}")
    // .MapValues(v => v.venda + " => " + v.cliente)
    .Peek((k, v) => Console.WriteLine($"key={k} value={v} #################"))
    .MapValues(v => v.ToUpper() + " - stream sample 22")
    .To("playground.analitico.venda");
var stream = new KafkaStream(builder.Build(), config);
Console.CancelKeyPress += (_, _) => stream.Dispose();
await stream.StartAsync();

// var builder = new StreamBuilder();
// builder
//     .Stream<string, string>("playground.transacional.venda")
//     // .Peek((k, v) => Console.WriteLine($"key={k} value={v} #################"))
//     .MapValues(v => v.ToUpper() + " - stream sample 3")
//     .To("playground.analitico.venda");
// var stream = new KafkaStream(builder.Build(), config);
// Console.CancelKeyPress += (_, _) => stream.Dispose();
// await stream.StartAsync();

Console.WriteLine("Fim");