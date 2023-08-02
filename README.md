# Kafka Playground

- [Visão geral](#visão-geral)
- [Docker compose](#docker-compose)
- [Benchmark consumidores](#benchmark-consumidores)
- [Avro](#avro)
- [Schema registry](#schema-registry)
- [Kafka Configs](#kafka-configs)
	- [Broker](#broker)
- [Scripts](#scripts)
	- [Producers](#producers)
	- [Consumers](#consumers)
	- [Excluir consumer groups](#excluir-consumer-groups)
	- [Excluir tópicos](#excluir-tópicos)
	- [Outros](#outros)
	- [Clean up policy: Compact](#clean-up-policy-compact)
- [Outros scripts para os exemplos de código](#outros-scripts-para-os-exemplos-de-código)

## Visão geral

**Kafka**

- [topic-configs](https://docs.confluent.io/platform/current/installation/configuration/topic-configs.html)
- [consumer-configs](https://docs.confluent.io/platform/current/installation/configuration/consumer-configs.html)
- [producer-configs](https://docs.confluent.io/platform/current/installation/configuration/producer-configs.html)

**SDK .net**

- Commit informando offset é sincrono
- É um wrapper da library [librdkafka](https://github.com/confluentinc/librdkafka), assim como todas outras linguagens, exceto Java.
- Não permite consumir tópico sem gerar o grupo consumidor (group.id).
  - O projeto **librdkafka** não suporta a feature. Issue: https://github.com/confluentinc/librdkafka/issues/3261
  - Issue do wrapper .net: https://github.com/confluentinc/confluent-kafka-dotnet/issues/1697


## Docker compose

Template para subir serviço local.

Fonte: <https://github.com/confluentinc/cp-all-in-one>

```bash
# subir serviço
docker compose up -d

# remover serviço
docker compose down
```

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

## Kafka Configs

### Broker

- `log.retention.check.interval.ms`: Frequência para verificar se algum log é elegível para exclusão. Default: 5 min (Exclusão ou compactação de acordo com cleanup.policy do tópico)

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

```bash
kafka-topics.sh --bootstrap-server localhost:9092 --delete --topic basic-playground
kafka-topics.sh --bootstrap-server localhost:9092 --delete --topic avro-playground
kafka-topics.sh --bootstrap-server localhost:9092 --delete --topic kafka-flow-playground
```

### Outros

```bash
kafka-topics.sh --bootstrap-server localhost:9092 --topic topic1 --describe
```

### Clean up policy: Compact

```bash
# criar tópico - com politica de compactação (parâmetros agressivos para o teste).
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

Tópico com configuração tradicional de exclusão para efeito de comparação.

```bash
kafka-topics.sh --bootstrap-server localhost:9092 --create --topic cadastros \
	--config cleanup.policy=delete \
	--config retention.ms=10000
```

## Outros scripts para os exemplos de código

```bash
# consumer para exemplo "05 without-consumer-group"
kafka-console-consumer.sh --bootstrap-server localhost:9092 --topic without-consumer-group-playground \
	--from-beginning \
	--property print.key=true \
	--timeout-ms 1000
```
