# Ferramentas de Produtividade

## Executando as aplicações

```
docker compose up -d --build
```

## Adicione o plugin ao Kafka Connect
* Execute uma requisição ao endpoint ```add postgres connector```

> Se o Kafka Connect não estiver inicializado, aguarde uns segundos e tente novamente

* Verifique os conectores executando uma requisição ao endpoint ```connectors```

## Gerando eventos
* Execute uma requisição ```post order```
* Verifique se o evento chegou no Kafka acessando os logs do consumidor 

```sh
docker compose logs --no-log-prefix kafka-consumer  --tail 1 
```

* Verifique se o log chegou ao Logstash
```sh
docker compose logs --no-log-prefix logstash --tail 1
```

> Use o ```jq``` se estiver disponível para formatar a saída do log.

```sh
docker compose logs --no-log-prefix logstash --tail 1 | jq
```

## Adicionando destinos aos eventos

### Enviando os logs para o ElasticSearch
* Adicione o bloco de codigo ao output no arquivo logstash/kafka-to-elastic-pipeline.conf
```
# audit logs
    elasticsearch { 
        hosts => ["${ELASTIC_HOST}"]
        index => "orders-%{+YYYY.MM.dd}"
        codec => json

    }


```
* Reinicie o logstash
```sh
docker rm logstash -f && docker compose up logstash -d
```

* Faça uma requisição novamente e verifique se o log aparece no Kibana (http://localhost:5601)

### Enviando os logs para um endereço HTTP

* Adicione o bloco de codigo ao output no arquivo logstash/kafka-to-elastic-pipeline.conf

```
    #notify http
    http {
        http_method => "post"
        url => "${HTTP_URL}/customers/%{[payload][after][customer_id]}/orders/%{[payload][after][id]}/notify/%{[payload][op]}"
        mapping => { "status"=> "%{[payload][after][status]}" }
    }

```

* Reinicie o logstash
```sh
docker rm logstash -f && docker compose up logstash -d
```

* Faça uma nova requisição

Verifique os logs do echo-server
```sh
docker compose logs --no-log-prefix echo-server --tail 1 
```

> Use o ```jq``` se estiver disponível para formatar a saída do log.


```sh
docker compose logs --no-log-prefix echo-server --tail 1 | jq
```
