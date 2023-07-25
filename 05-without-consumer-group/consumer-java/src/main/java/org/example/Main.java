package org.example;

import org.apache.kafka.clients.admin.AdminClient;
import org.apache.kafka.clients.admin.AdminClientConfig;
import org.apache.kafka.clients.consumer.ConsumerConfig;
import org.apache.kafka.clients.consumer.KafkaConsumer;
import org.apache.kafka.common.TopicPartition;
import org.apache.kafka.common.errors.WakeupException;
import org.apache.kafka.common.serialization.IntegerDeserializer;
import org.apache.kafka.common.serialization.StringDeserializer;

import java.text.NumberFormat;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Properties;
import java.util.concurrent.ExecutionException;


// Press Shift twice to open the Search Everaywhere dialog and type `show whitespaces`,
// then press Enter. You can now see whitespace characters in your code.
public class Main {

    private static final String TopicName = "without-consumer-group-playground";

    public static void main(String[] args) throws ExecutionException, InterruptedException {
        System.out.println(".:: Kafka Playground - Without Consumer Group (Consumer) - Java ::.");

        // Configurações:
        // 1. Consumer não deve ter group id e não será utilizado o método de "subscribe"
        // 2. Não realizar subscribe do consumer no tópico
        // 3. Realizar assignment manual no tópico/partição
        // 4. Auto commit deve estar desabilitado
        // Pontos de atenção:
        // - Sem consumer group não há lag para observabilidade pelo broker e você precisará ter mecanismos a parte para ter visão de atrasos

        // AdminClient - Consumir tópico para obter quantidade de partições
        Properties adminProps = new Properties();
        adminProps.put(AdminClientConfig.BOOTSTRAP_SERVERS_CONFIG, "localhost:9092");
        final var admin = AdminClient.create(adminProps);
        var topicInfo = admin.describeTopics(Collections.singletonList(TopicName));
        var values = topicInfo.values();
        var topicDescription = values.get(TopicName);
        var partitions = topicDescription.get().partitions().size();

        // Consumer Properties
        Properties consumerProps = new Properties();
        consumerProps.put(ConsumerConfig.BOOTSTRAP_SERVERS_CONFIG, "localhost:9092");
//        consumerProps.put(ConsumerConfig.GROUP_ID_CONFIG, "java-playground");
//        consumerProps.put(ConsumerConfig.GROUP_INSTANCE_ID_CONFIG, "1");
        consumerProps.put(ConsumerConfig.AUTO_OFFSET_RESET_CONFIG, "earliest");
        consumerProps.put(ConsumerConfig.KEY_DESERIALIZER_CLASS_CONFIG, IntegerDeserializer.class);
        consumerProps.put(ConsumerConfig.VALUE_DESERIALIZER_CLASS_CONFIG, StringDeserializer.class);

        consumerProps.put(ConsumerConfig.ENABLE_AUTO_COMMIT_CONFIG, "false");
        consumerProps.put(ConsumerConfig.AUTO_COMMIT_INTERVAL_MS_CONFIG, "5000");

        consumerProps.put(ConsumerConfig.HEARTBEAT_INTERVAL_MS_CONFIG, "3000");
        consumerProps.put(ConsumerConfig.SESSION_TIMEOUT_MS_CONFIG, "10000");
        consumerProps.put(ConsumerConfig.MAX_POLL_INTERVAL_MS_CONFIG, "300000"); // Tempo máximo para processar mensagens recebidas. Se ultrapassar, será feito rebalance
        consumerProps.put(ConsumerConfig.MAX_POLL_RECORDS_CONFIG, "10000"); // Quantidade de registros para consultar por pacote

        consumerProps.put(ConsumerConfig.MAX_PARTITION_FETCH_BYTES_CONFIG, "10485760"); // 10 mb
        consumerProps.put(ConsumerConfig.FETCH_MAX_BYTES_CONFIG, "10485760"); // 10 mb

        final var consumer = new KafkaConsumer<Integer, String>(consumerProps);

        // Atribuir manualmente as partições para consumir - Lib não permite informar apenas o tópico
        var topicPartitions = new ArrayList<TopicPartition>();
        for (int i = 0; i < partitions; i++)
            topicPartitions.add(new TopicPartition(TopicName, i));
        consumer.assign(topicPartitions);

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
            while (true) {
                var records = consumer.poll(100);
                if (records.count() > 0) {
                    for (var result : records) {
                        System.out.println(result.key() + ": " + result.value());
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