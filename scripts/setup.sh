kafka-topics.sh --bootstrap-server localhost:9092 --delete --topic playground.cadastros.cliente
kafka-topics.sh --bootstrap-server localhost:9092 --delete --topic playground.cadastros.produto
kafka-topics.sh --bootstrap-server localhost:9092 --delete --topic playground.transacional.venda
kafka-topics.sh --bootstrap-server localhost:9092 --delete --topic playground.analitico.venda

kafka-topics.sh --bootstrap-server localhost:9092 --create \
    --topic playground.cadastros.cliente \
    --partitions 3 \
    --config cleanup.policy=compact \
    --config retention.ms=10000

    # --config delete.retention.ms=10000

kafka-topics.sh --bootstrap-server localhost:9092 --create \
    --topic playground.cadastros.produto \
    --partitions 3 \
    --config cleanup.policy=compact \
    --config retention.ms=10000

kafka-topics.sh --bootstrap-server localhost:9092 --create \
    --topic playground.transacional.venda \
    --partitions 3 \
    --config cleanup.policy=delete \
    --config retention.ms=10000

kafka-topics.sh --bootstrap-server localhost:9092 --create \
    --topic playground.analitico.venda \
    --partitions 3 \
    --config cleanup.policy=compact \
    --config retention.ms=10000