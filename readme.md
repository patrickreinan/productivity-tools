
docker compose up -d --build


docker run --rm --network devcon-network confluentinc/cp-kafka:7.4.0 /usr/bin/kafka-console-consumer --bootstrap-server kafka:9092 --topic orders.public.orders 
docker run --rm --network devcon-network confluentinc/cp-kafka:7.4.0 /usr/bin/kafka-console-consumer --bootstrap-server kafka:9092 --topic  orders.public.orders_items --from-beginning



docker compose exec  postgres psql -U ordersuser -d orders -w -h localhost -f /tmp/sample/sample.sql







docker run --rm --network devcon-network confluentinc/cp-kafka:7.4.0 /usr/bin/kafka-topics --bootstrap-server kafka:9092 --list