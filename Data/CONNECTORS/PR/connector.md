# Kafka Elastic Search Synchronization Connector:

## Sync PR_STREAM_WITH_COMMENTS_COUNT topic to Elastic Search

```
curl -X POST -H "Content-Type: application/json" \
-d '{
"name": "elasticsearch-sink-connector-pr-v3",
"config": {
  "connector.class": "io.confluent.connect.elasticsearch.ElasticsearchSinkConnector",
  "tasks.max": "1",
  "topics": "PR_COMPLETED_STREAM",
  "key.ignore": "false",
  "value.converter": "org.apache.kafka.connect.json.JsonConverter",
  "key.converter": "org.apache.kafka.connect.storage.StringConverter",
  "connection.url": "http://elasticsearch:9200",
  "type.name": "_doc",
  "schema.ignore": "true",
  "key.converter.schemas.enable": "false",
  "value.converter.schemas.enable": "false",
  "behavior.on.malformed.documents": "ignore",
  "errors.tolerance": "all",
  "errors.log.enable": "true",
  "errors.log.include.messages": "true"
}}' http://localhost:8083/connectors

```

