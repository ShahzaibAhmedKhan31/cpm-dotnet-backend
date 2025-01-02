# Kafka Elastic Search Synchronization Connector:

## Sync JIRA_COMPLETED_ISSUES topic to Elastic Search

```
curl -X POST -H "Content-Type: application/json" \
-d '{
"name": "elasticsearch-sink-connector-v3",
"config": {
  "connector.class": "io.confluent.connect.elasticsearch.ElasticsearchSinkConnector",
  "tasks.max": "1",
  "topics": "JIRA_COMPLETED_ISSUES",
  "key.ignore": "true",
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

