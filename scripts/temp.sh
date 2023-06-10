kafka-topics.sh --bootstrap-server localhost:9092 --create \
    --topic teste-store \
    --partitions 1 \
    --config cleanup.policy=compact \
    --config retention.ms=5000 \
    --config min.cleanable.dirty.ratio=0.01

# kafka-topics.sh --bootstrap-server localhost:9092 --create \
#     --topic temp-store \
#     --partitions 3 \
#     --config cleanup.policy=compact \
#     --config retention.ms=5000 \
#     --config min.cleanable.dirty.ratio=0.01

    