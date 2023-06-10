package org.example;

import org.apache.kafka.clients.consumer.ConsumerConfig;
import org.apache.kafka.common.serialization.Serde;
import org.apache.kafka.common.serialization.Serdes;
import org.apache.kafka.common.utils.Bytes;
import org.apache.kafka.streams.KafkaStreams;
import org.apache.kafka.streams.StreamsBuilder;
import org.apache.kafka.streams.StreamsConfig;
import org.apache.kafka.streams.Topology;
import org.apache.kafka.streams.kstream.*;
import org.apache.kafka.streams.state.KeyValueStore;

import java.time.Duration;
import java.util.Arrays;
import java.util.Properties;
import java.util.stream.Stream;


// Press Shift twice to open the Search Everywhere dialog and type `show whitespaces`,
// then press Enter. You can now see whitespace characters in your code.
public class Main {
    public static void main(String[] args) {
        // Press Alt+Enter with your caret at the highlighted text to see how
        // IntelliJ IDEA suggests fixing it.
        System.out.printf(".:: Kafka Streams Playground - Java Worker ::.");

        Properties props = new Properties();
        props.put(StreamsConfig.APPLICATION_ID_CONFIG, "playground-stream-java-sample-15");
        props.put(StreamsConfig.BOOTSTRAP_SERVERS_CONFIG, "localhost:9092");
        props.put(StreamsConfig.DEFAULT_KEY_SERDE_CLASS_CONFIG, Serdes.String().getClass());
        props.put(StreamsConfig.DEFAULT_VALUE_SERDE_CLASS_CONFIG, Serdes.String().getClass());
        props.put(ConsumerConfig.AUTO_OFFSET_RESET_CONFIG, "earliest");
        props.put(StreamsConfig.CACHE_MAX_BYTES_BUFFERING_CONFIG, "0");

//        StreamsBuilder builder = new StreamsBuilder();
//        builder
//                .stream("playground.transacional.venda")
//                .mapValues(v -> v.toString().toUpperCase() + " - java stream sample 5")
//                .to("playground.analitico.venda");
//        KafkaStreams streams = new KafkaStreams(builder.build(), props);
//        streams.start();

        StreamsBuilder builder = new StreamsBuilder();
        KStream<String, String> clientes = builder.stream("playground.cadastros.cliente");
        KStream<String, String> vendas = builder.stream("playground.transacional.venda");

        KStream<String, String> joined = vendas.join(clientes,
                (venda, cliente) -> venda + " => " + cliente + " java stream sample 15",
                JoinWindows.of(Duration.ofSeconds(1)))
                .filter((key, value) -> value != null);
        joined.to("playground.analitico.venda");
        Topology topology = builder.build();
        KafkaStreams streams = new KafkaStreams(topology, props);
        streams.start();
    }
}