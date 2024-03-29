package org.example;

import io.confluent.kafka.serializers.KafkaAvroDeserializer;
import io.confluent.kafka.serializers.KafkaAvroDeserializerConfig;
import org.apache.kafka.clients.consumer.ConsumerConfig;
import org.apache.kafka.clients.consumer.KafkaConsumer;
import org.apache.kafka.common.errors.WakeupException;
import org.apache.kafka.common.serialization.IntegerDeserializer;
import playground.kafka.Pessoa;

import java.text.NumberFormat;
import java.util.Collections;
import java.util.Properties;
import java.util.concurrent.TimeUnit;


// Press Shift twice to open the Search Everaywhere dialog and type `show whitespaces`,
// then press Enter. You can now see whitespace characters in your code.
public class Main {

    public static void main(String[] args) {
        System.out.println(".:: Kafka Playground - Avro Java Consumer ::.");

        Properties props = new Properties();
        props.put(ConsumerConfig.BOOTSTRAP_SERVERS_CONFIG, "localhost:9092");
        props.put(ConsumerConfig.GROUP_ID_CONFIG, "java-playground");
        props.put(ConsumerConfig.GROUP_INSTANCE_ID_CONFIG, "1");
        props.put(ConsumerConfig.AUTO_OFFSET_RESET_CONFIG, "earliest");
        props.put(ConsumerConfig.KEY_DESERIALIZER_CLASS_CONFIG, IntegerDeserializer.class);
        props.put(ConsumerConfig.VALUE_DESERIALIZER_CLASS_CONFIG, KafkaAvroDeserializer.class);
        props.put(KafkaAvroDeserializerConfig.SCHEMA_REGISTRY_URL_CONFIG, "http://localhost:8081");
        props.put(KafkaAvroDeserializerConfig.SPECIFIC_AVRO_READER_CONFIG, "true");

        props.put(ConsumerConfig.ENABLE_AUTO_COMMIT_CONFIG, "true");
        props.put(ConsumerConfig.AUTO_COMMIT_INTERVAL_MS_CONFIG, "5000");

        props.put(ConsumerConfig.HEARTBEAT_INTERVAL_MS_CONFIG, "3000");
        props.put(ConsumerConfig.SESSION_TIMEOUT_MS_CONFIG, "10000");
        props.put(ConsumerConfig.MAX_POLL_INTERVAL_MS_CONFIG, "300000"); // Tempo máximo para processar mensagens recebidas. Se ultrapassar, será feito rebalance
        props.put(ConsumerConfig.MAX_POLL_RECORDS_CONFIG, "10000"); // Quantidade de registros para buscar por pacote

        props.put(ConsumerConfig.MAX_PARTITION_FETCH_BYTES_CONFIG, "10485760"); // 10 mb
        props.put(ConsumerConfig.FETCH_MAX_BYTES_CONFIG, "10485760"); // 10 mb

        final var consumer = new KafkaConsumer<Integer, Pessoa>(props);
        consumer.subscribe(Collections.singletonList("avro-playground"));

        // get a reference to the current thread
        final Thread mainThread = Thread.currentThread();

        // adding the shutdown hook
        Runtime.getRuntime().addShutdownHook(new Thread() {
            public void run() {
//                Main.ShutdownRequested = true;
                consumer.wakeup();

                // join the main thread to allow the execution of the code in the main thread
                try {
                    mainThread.join();
                } catch (InterruptedException e) {
                    e.printStackTrace();
                }
            }
        });

        var concluido = false;
        try {
            NumberFormat myFormat = NumberFormat.getInstance();
            var startTime = System.currentTimeMillis();
            var count = 0;
            while (true) {
                var records = consumer.poll(100);
                if (records.count() > 0) {
//                    for (var result : records) {
//                        System.out.println(result.key() + ": " + result.value().getNome());
//                    }
                    final var itr = records.iterator();
                    var last = itr.next();
                    while (itr.hasNext()) {
                        last = itr.next();
                    }

                    count += records.count();
                    System.out.println("Count: " + myFormat.format(count) + " - " + last.key() + ": " + last.value().getNome());
//                    Thread.sleep(1000);
//                    consumer.commitAsync(); // Auto commit habilitado. Analise os possíveis casos de exceptions no seu caso de uso para decidir sobre isso!
                } else if (count >= 10_000_000 && !concluido) {
                    var endTime = System.currentTimeMillis();
                    long millis = endTime - startTime;
                    String hms = String.format("%02d:%02d:%02d", TimeUnit.MILLISECONDS.toHours(millis),
                            TimeUnit.MILLISECONDS.toMinutes(millis) - TimeUnit.HOURS.toMinutes(TimeUnit.MILLISECONDS.toHours(millis)),
                            TimeUnit.MILLISECONDS.toSeconds(millis) - TimeUnit.MINUTES.toSeconds(TimeUnit.MILLISECONDS.toMinutes(millis)));

                    // 10 milhões: Concluído em 00:00:09. Millis: 9.465
                    // 50 milhões: Concluído em 00:00:38. Millis: 38.270
                    System.out.println("Concluído em " + hms + ". Millis: " + myFormat.format(millis));
                    concluido = true;
                }
            }
        } catch (WakeupException e) {
            System.out.println("### Execução cancelada ###");
            consumer.close();
//        } catch (InterruptedException e) {
        } finally {
            System.out.println("Fim");
        }
    }
}