kafka-console-consumer.sh \
    --bootstrap-server localhost:9092 \
    --topic playground.cadastros.cliente \
    --from-beginning \
    --property print.key=true