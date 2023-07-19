# Kafka Playground

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

- Schema Registry local server: http://localhost:8081/
- Especificação: https://avro.apache.org/docs/1.11.1/specification/
- Exemplo código: https://github.com/confluentinc/confluent-kafka-dotnet/blob/master/examples/AvroSpecific/Program.cs
- Avro gen: https://github.com/confluentinc/confluent-kafka-dotnet/tree/master/examples/AvroSpecific
- Avro gen 2: https://www.confluent.io/blog/decoupling-systems-with-apache-kafka-schema-registry-and-avro/

```bash
dotnet tool install --global Apache.Avro.Tools

# Gerar classe de arquivo local
avrogen -s ./Pessoa.avsc . --namespace "playground.kafka:playground.kafka"
```

GET /subjects/{subject}/versions/{version}/referencedby
GET /schemas/ids/{id}/versions

## Scripts

#### Producers

```bash
kafka-console-producer.sh --bootstrap-server localhost:9092 --topic kafka-flow-playground \
	--property parse.key=true \
	--property key.separator=:
```

#### Consumers

```bash
kafka-console-consumer.sh --bootstrap-server localhost:9092 --topic kafka-flow-playground \
	--from-beginning \
	--property print.key=true
```

#### Excluir consumer groups

```bash
kafka-consumer-groups.sh --bootstrap-server localhost:9092 --delete --group dotnet-playground & \
kafka-consumer-groups.sh --bootstrap-server localhost:9092 --delete --group java-playground
```

#### Excluir tópicos

```
kafka-topics.sh --bootstrap-server localhost:9092 --delete --topic basic-playground
kafka-topics.sh --bootstrap-server localhost:9092 --delete --topic avro-playground
kafka-topics.sh --bootstrap-server localhost:9092 --delete --topic kafka-flow-playground
```
