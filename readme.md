```bash
curl -X POST -H "Accept:application/json" -H "Content-Type:application/json" localhost:8083/connectors/ -d '
{
  "name": "orders-connector",
  "config": {
      "connector.class": "io.debezium.connector.postgresql.PostgresConnector",
      "tasks.max": "1",
      "database.hostname": "postgres",
      "database.port": "5432",
      "database.user": "ordersuser",
      "database.password": "orders-pwd",
      "database.dbname" : "orders",
      "database.server.name": "postgres",
      "schema.include.list": "orders",
      "topic.prefix":"orders-events",
      
      }
 }
'
```