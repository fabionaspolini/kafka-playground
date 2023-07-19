# Kafka Playground

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
kafka-consumer-groups.sh --bootstrap-server localhost:9092 --delete --group dotnet-playground-flow & \
kafka-consumer-groups.sh --bootstrap-server localhost:9092 --delete --group java-playground
```

#### Excluir tópicos

```
kafka-topics.sh --bootstrap-server localhost:9092 --delete --topic basic-playground
kafka-topics.sh --bootstrap-server localhost:9092 --delete --topic kafka-flow-playground
```
