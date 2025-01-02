curl -X POST -H "Content-Type: application/json" \
-d '{
  "name": "elasticsearch-sink-connector-tfs-v8",
  "config": {
    "connector.class": "io.confluent.connect.elasticsearch.ElasticsearchSinkConnector",
    "tasks.max": "1",
    "topics": "ENRICHED_WORK_ITEMS_PARTITIONED,INPROGRESS_WORK_ITEMS_PARTITIONED,COMPLETED_WORK_ITEMS_PARTITIONED",
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
	"index.name": "tfs_index",
    "errors.log.include.messages": "true",
    "flush.synchronously": "true",
    "transforms": "route",
    "transforms.route.type": "org.apache.kafka.connect.transforms.RegexRouter",
    "transforms.route.regex": "(.*)",
    "transforms.route.replacement": "tfs_index"
  }
}' http://localhost:8083/connectors