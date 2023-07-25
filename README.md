# Kafka Playground

- [Benchmark consumidores](#benchmark-consumidores)
- [Avro](#avro)
- [Schema registry](#schema-registry)
- [Scripts](#scripts)
	- [Producers](#producers)
	- [Consumers](#consumers)
	- [Excluir consumer groups](#excluir-consumer-groups)
	- [Excluir tópicos](#excluir-tópicos)
	- [Clean up policy: Compact](#clean-up-policy-compact)

## Benchmark consumidores

```txt
.net basic                                      java basic
10 milhões: Concluído em 00:00:10.9476809       10 milhões: Concluído em 00:00:03. Millis: 3.100
50 milhões: Concluído em 00:00:42.3009102       50 milhões: Concluído em 00:00:14. Millis: 14.852

.net avro                                       java avro
10 milhões: Concluído em 00:00:21.0839030       10 milhões: Concluído em 00:00:09. Millis: 9.465
50 milhões: Concluído em 00:01:34.9580854       50 milhões: Concluído em 00:00:38. Millis: 38.270
```

## Avro

- Especificação: https://avro.apache.org/docs/1.11.1/specification/
- Exemplo código: https://github.com/confluentinc/confluent-kafka-dotnet/blob/master/examples/AvroSpecific/Program.cs
- **Source generator:** [Confluent.Apache.Avro.AvroGen](https://www.confluent.io/blog/decoupling-systems-with-apache-kafka-schema-registry-and-avro/) ✔️

Plugin mais antigo, mas sem margem de personalziação que possa causar errors de namespace.

```bash
dotnet tool install -g Confluent.Apache.Avro.AvroGen
avrogen -s ./Pessoa.avsc .
```

- **Source generator:**  [Apache.Avro.Tools](https://github.com/confluentinc/confluent-kafka-dotnet/tree/master/examples/AvroSpecific) ❌

Plugin mais atual, porém possui a opção de mudar o namespace do código fonte gerado e isso causa quebra de interoperabilidade entre linguagens.

```bash
dotnet tool install --global Apache.Avro.Tools
avrogen -s ./Pessoa.avsc . --namespace "playground.kafka:playground.kafka"
```

## Schema registry

local server: http://localhost:8081/

Rotas:

```txt
GET /subjects
GET /subjects/{subject}/versions
GET /subjects/{subject}/versions/{version}
GET /subjects/{subject}/versions/{version}/referencedby
GET /schemas
GET /schemas/ids/{id}
GET /schemas/ids/{id}/versions
```

http://localhost:8081/subjects/playground.kafka.Pessoa/versions/1

## Scripts

### Producers

```bash
kafka-console-producer.sh --bootstrap-server localhost:9092 --topic kafka-flow-playground \
	--property parse.key=true \
	--property key.separator=:
```

### Consumers

```bash
kafka-console-consumer.sh --bootstrap-server localhost:9092 --topic kafka-flow-playground \
	--from-beginning \
	--property print.key=true
```

### Excluir consumer groups

```bash
kafka-consumer-groups.sh --bootstrap-server localhost:9092 --delete --group dotnet-playground & \
kafka-consumer-groups.sh --bootstrap-server localhost:9092 --delete --group java-playground
```

### Excluir tópicos

```
kafka-topics.sh --bootstrap-server localhost:9092 --delete --topic basic-playground
kafka-topics.sh --bootstrap-server localhost:9092 --delete --topic avro-playground
kafka-topics.sh --bootstrap-server localhost:9092 --delete --topic kafka-flow-playground
```

### Clean up policy: Compact

```bash
# criar - exclusão rápida com configurações agressivas.
kafka-topics.sh --bootstrap-server localhost:9092 --create --topic cadastros \
	--config cleanup.policy=compact \
	--config segment.ms=100 \
	--config min.cleanable.dirty.ratio=0.01

# produzir dados no formato "key:value"
kafka-console-producer.sh --bootstrap-server localhost:9092 --topic cadastros \
	--property parse.key=true \
	--property key.separator=:

# consumer
kafka-console-consumer.sh --bootstrap-server localhost:9092 --topic cadastros \
	--from-beginning \
	--property print.key=true \
	--timeout-ms 1000

# excluir
kafka-topics.sh --bootstrap-server localhost:9092 --delete --topic cadastros
```

Consumer para exemplo "05 without-consumer-group"

```
kafka-console-consumer.sh --bootstrap-server localhost:9092 --topic without-consumer-group-playground \
	--from-beginning \
	--property print.key=true \
	--timeout-ms 1000
```