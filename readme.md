curl --location 'localhost:8083/connectors/' \
--header 'Accept: application/json' \
--header 'Content-Type: application/json' \
--data '{
  "name": "orders-connector",
  "config": {
      "connector.class": "io.debezium.connector.postgresql.PostgresConnector",
      "tasks.max": "1",
      "database.hostname": "postgres",
      "database.port": "5432",
      "database.user": "ordersuser",
      "database.password": "orders-pwd",
      "database.dbname" : "orders",
      "table.include.list": "orders,orders_items",
      "topic.prefix":"orders",
      "schema.name":"public",
      "plugin.name": "pgoutput"
      
      }
 }'

docker run --network devcon-network confluentinc/cp-kafka:7.4.0 /usr/bin/kafka-topics --bootstrap-server kafka:9092 --topic orders-events --create --if-not-exists 
docker run --network devcon-network confluentinc/cp-kafka:7.4.0 /usr/bin/kafka-console-consumer --bootstrap-server kafka:9092 --topic orders-events



docker compose exec postgres psql -U ordersuser -d orders -w -h localhost







docker run --network devcon-network confluentinc/cp-kafka:7.4.0 /usr/bin/kafka-topics --bootstrap-server kafka:9092 --list