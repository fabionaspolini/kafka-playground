using Confluent.Kafka;

Console.WriteLine(".:: Kafka Streams Playground - Producer ::.");

var producerConfig = new ProducerConfig
{
    BootstrapServers = "localhost:9092",
};
var producer = new ProducerBuilder<string, string>(producerConfig).Build();

producer.Produce("playground.cadastros.cliente", new() { Key = "fábio", Value = "nome=Fábio,idade=30" });
producer.Produce("playground.cadastros.cliente", new() { Key = "joão", Value = "nome=João,idade=20" });
producer.Produce("playground.cadastros.cliente", new() { Key = "pedro", Value = "nome=Pedro,idade=15" });

producer.Produce("playground.cadastros.produto", new() { Key = "batata", Value = "nome=Batata,tipo=legume" });
producer.Produce("playground.cadastros.produto", new() { Key = "banana", Value = "nome=Banana,tipo=fruta" });
producer.Produce("playground.cadastros.produto", new() { Key = "laranja", Value = "nome=Laranja,tipo=fruta" });

producer.Produce("playground.transacional.venda", new() { Key="1", Value="fábio,batata" });
producer.Produce("playground.transacional.venda", new() { Key="1", Value="fábio,laranja" });
producer.Produce("playground.transacional.venda", new() { Key="2", Value="joão,batata" });
producer.Produce("playground.transacional.venda", new() { Key="3", Value="pedro,banana" });

/*producer.Produce("playground.cadastros.cliente", new() { Key = "fábio", Value = "nome=Fábio,idade=40" });
producer.Produce("playground.cadastros.cliente", new() { Key = "joão", Value = "nome=João,idade=45" });
producer.Produce("playground.cadastros.cliente", new() { Key = "pedro", Value = "nome=Pedro,idade=50" });

producer.Produce("playground.cadastros.cliente", new() { Key = "a", Value = "nome=a,idade=30" });
producer.Produce("playground.cadastros.cliente", new() { Key = "b", Value = "nome=b,idade=20" });
producer.Produce("playground.cadastros.cliente", new() { Key = "c", Value = "nome=c,idade=15" });
producer.Produce("playground.cadastros.cliente", new() { Key = "d", Value = "nome=d,idade=30" });
producer.Produce("playground.cadastros.cliente", new() { Key = "e", Value = "nome=e,idade=20" });
producer.Produce("playground.cadastros.cliente", new() { Key = "f", Value = "nome=f,idade=15" });

for (var i = 1; i <= 20; i++)
    producer.Produce("playground.cadastros.cliente", new() { Key = i.ToString(), Value = $"nome={i},idade={i}" });*/

producer.Flush();

Console.WriteLine("Fim");