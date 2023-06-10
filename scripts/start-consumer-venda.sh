kafka-console-consumer.sh \
    --bootstrap-server localhost:9092 \
    --topic playground.transacional.venda \
    --from-beginning \
    --property print.key=true