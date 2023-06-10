kafka-topics.sh --bootstrap-server localhost:9092 --delete --topic playground.cadastros.cliente
kafka-topics.sh --bootstrap-server localhost:9092 --delete --topic playground.cadastros.produto
kafka-topics.sh --bootstrap-server localhost:9092 --delete --topic playground.transacional.venda
kafka-topics.sh --bootstrap-server localhost:9092 --delete --topic playground.analitico.venda

# kafka-topics.sh --bootstrap-server localhost:9092 --create \
#     --topic playground.cadastros.cliente \
#     --partitions 1 \
#     --config cleanup.policy=compact \
#     --config retention.ms=5000 \
#     --config delete.retention.ms=5000 \
#     --config max.compaction.lag.ms=5000 \
#     --config segment.ms=5000 \
#     --config min.cleanable.dirty.ratio=0.01

kafka-topics.sh --bootstrap-server localhost:9092 --create \
    --topic playground.cadastros.cliente \
    --partitions 1 \
    --config cleanup.policy=compact \
    --config retention.ms=5000 \
    --config min.cleanable.dirty.ratio=0.01

kafka-topics.sh --bootstrap-server localhost:9092 --create \
    --topic playground.cadastros.produto \
    --partitions 1 \
    --config cleanup.policy=compact \
    --config retention.ms=5000 \
    --config min.cleanable.dirty.ratio=0.01

kafka-topics.sh --bootstrap-server localhost:9092 --create \
    --topic playground.transacional.venda \
    --partitions 1 \
    --config cleanup.policy=compact \
    --config retention.ms=600000

kafka-topics.sh --bootstrap-server localhost:9092 --create \
    --topic playground.analitico.venda \
    --partitions 1 \
    --config cleanup.policy=compact \
    --config retention.ms=5000 \
    --config min.cleanable.dirty.ratio=0.01