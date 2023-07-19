package org.example;

import org.apache.kafka.clients.consumer.ConsumerConfig;
import org.apache.kafka.clients.consumer.KafkaConsumer;
import org.apache.kafka.common.errors.WakeupException;
import org.apache.kafka.common.serialization.IntegerDeserializer;
import org.apache.kafka.common.serialization.Serdes;
import org.apache.kafka.common.serialization.StringDeserializer;

import java.text.NumberFormat;
import java.time.Duration;
import java.time.Instant;
import java.util.Collections;
import java.util.Properties;
import java.util.concurrent.TimeUnit;


// Press Shift twice to open the Search Everaywhere dialog and type `show whitespaces`,
// then press Enter. You can now see whitespace characters in your code.
public class Main {

    //    static boolean ShutdownRequested = false;
    public static void main(String[] args) {
        System.out.println(".:: Kafka Playground - Basic Java Consumer ::.");

        Properties props = new Properties();
        props.put(ConsumerConfig.BOOTSTRAP_SERVERS_CONFIG, "localhost:9092");
        props.put(ConsumerConfig.GROUP_ID_CONFIG, "java-playground");
        props.put(ConsumerConfig.GROUP_INSTANCE_ID_CONFIG, "1");
        props.put(ConsumerConfig.AUTO_OFFSET_RESET_CONFIG, "earliest");
        props.put(ConsumerConfig.KEY_DESERIALIZER_CLASS_CONFIG, IntegerDeserializer.class);
        props.put(ConsumerConfig.VALUE_DESERIALIZER_CLASS_CONFIG, StringDeserializer.class);

        props.put(ConsumerConfig.ENABLE_AUTO_COMMIT_CONFIG, "true");
        props.put(ConsumerConfig.AUTO_COMMIT_INTERVAL_MS_CONFIG, "5000");

        props.put(ConsumerConfig.HEARTBEAT_INTERVAL_MS_CONFIG, "3000");
        props.put(ConsumerConfig.SESSION_TIMEOUT_MS_CONFIG, "10000");
        props.put(ConsumerConfig.MAX_POLL_INTERVAL_MS_CONFIG, "300000"); // Tempo máximo para processar mensagens recebidas. Se ultrapassar, será feito rebalance
        props.put(ConsumerConfig.MAX_POLL_RECORDS_CONFIG, "10000");

        props.put(ConsumerConfig.MAX_PARTITION_FETCH_BYTES_CONFIG, "10485760"); // 10 mb
        props.put(ConsumerConfig.FETCH_MAX_BYTES_CONFIG, "10485760"); // 10 mb

        final var consumer = new KafkaConsumer<Integer, String>(props);
        consumer.subscribe(Collections.singletonList("basic-playground"));

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

        try {
            NumberFormat myFormat = NumberFormat.getInstance();
            var startTime = System.currentTimeMillis();
            var count = 0;
            while (true) {
                var records = consumer.poll(100);
                if (records.count() > 0) {
                    /*for (var result : records) {
                        System.out.println(result.key() + ": " + result.value());
                    }*/
                    final var itr = records.iterator();
                    var last = itr.next();
                    while (itr.hasNext()) {
                        last = itr.next();
                    }

                    count += records.count();
                    System.out.println("Count: " + myFormat.format(count) + " - " + last.key() + ": " + last.value());
//                    Thread.sleep(1000);
                    consumer.commitAsync();

                    if (count == 10_000_000) {
                        var endTime = System.currentTimeMillis();
                        long millis = endTime - startTime;
                        String hms = String.format("%02d:%02d:%02d", TimeUnit.MILLISECONDS.toHours(millis),
                                TimeUnit.MILLISECONDS.toMinutes(millis) - TimeUnit.HOURS.toMinutes(TimeUnit.MILLISECONDS.toHours(millis)),
                                TimeUnit.MILLISECONDS.toSeconds(millis) - TimeUnit.MINUTES.toSeconds(TimeUnit.MILLISECONDS.toMinutes(millis)));
                        System.out.println("Concluído em " + hms + ". Millis: " + myFormat.format(millis)); // Concluído em 00:00:02. Millis: 2.925
                    }
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