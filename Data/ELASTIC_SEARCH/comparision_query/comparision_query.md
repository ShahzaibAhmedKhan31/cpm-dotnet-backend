# Elasticsearch Queries Reference

## List All Indices
```bash
curl -X GET "http://172.174.172.29:9200/_cat/indices?v"
```

## Search PR Index
```bash
curl --location 'http://localhost:9200/pr_index/_search?pretty' \
--header 'Content-Type: application/json' \
--data '{
  "size": 100,
  "query": {
    "match_all": {}
  }
}'
```

## Task Analysis - 12 Month Period
```bash
curl -X POST "http://172.174.172.29:9200/work_items_index/_search?pretty" \
-H 'Content-Type: application/json' \
-d '{
  "size": 0,
  "query": {
    "bool": {
      "should": [
        {
          "bool": {
            "must": [
              {
                "term": {
                  "STREAM_NAME.keyword": "created_work_items"
                }
              },
              {
                "bool": {
                  "should": [
                    {
                      "wildcard": {
                        "CURRENT_ASSIGNEE.keyword": "*hamza01961@gmail.com*"
                      }
                    },
                    {
                      "wildcard": {
                        "CURRENT_ASSIGNEE.keyword": "*shahzaib_pakistan@hotmail.com*"
                      }
                    },
                    {
                      "wildcard": {
                        "CURRENT_ASSIGNEE.keyword": "*third_email@example.com*"
                      }
                    }
                  ]
                }
              }
            ]
          }
        },
        {
          "bool": {
            "must": [
              {
                "term": {
                  "STREAM_NAME.keyword": "completed_work_items"
                }
              },
              {
                "bool": {
                  "should": [
                    {
                      "wildcard": {
                        "ASSIGNED_TO.keyword": "*hamza01961@gmail.com*"
                      }
                    },
                    {
                      "wildcard": {
                        "ASSIGNED_TO.keyword": "*shahzaib_pakistan@hotmail.com*"
                      }
                    },
                    {
                      "wildcard": {
                        "ASSIGNED_TO.keyword": "*third_email@example.com*"
                      }
                    }
                  ]
                }
              }
            ]
          }
        }
      ],
      "minimum_should_match": 1,
      "filter": [
        {
          "term": {
            "WORK_ITEM_TYPE.keyword": "Task"
          }
        },
        {
          "range": {
            "CREATED_DATE": {
              "gte": "now-12M/M",
              "lte": "now/M"
            }
          }
        }
      ]
    }
  }
}'
```

## Bug Analysis - 12 Month Period
```bash
curl -X POST "http://172.174.172.29:9200/work_items_index/_search?pretty" \
-H 'Content-Type: application/json' \
-d '{
    "size": 0,
    "query": {
      "bool": {
        "should": [
          {
            "bool": {
              "must": [
                {
                  "term": {
                    "STREAM_NAME.keyword": "created_work_items"
                  }
                },
                {
                  "bool": {
                    "should": [
                      {
                        "wildcard": {
                          "CURRENT_ASSIGNEE.keyword": "*hamza01961@gmail.com*"
                        }
                      },
                      {
                        "wildcard": {
                          "CURRENT_ASSIGNEE.keyword": "*shahzaib_pakistan@hotmail.com*"
                        }
                      },
                      {
                        "wildcard": {
                          "CURRENT_ASSIGNEE.keyword": "*third_email@example.com*"
                        }
                      }
                    ]
                  }
                }
              ]
            }
          }
        ],
        "minimum_should_match": 1,
        "filter": [
          {
            "term": {
              "WORK_ITEM_TYPE.keyword": "Bug"
            }
          }
        ]
      }
    }
}'
```

## Monthly Task Completion Rate - 3 Month Period
```bash
curl -X POST "http://localhost:9200/work_items_index/_search?pretty" \
-H 'Content-Type: application/json' \
-d '{
    "size": 0,
    "query": {
      "bool": {
        "should": [
          {
            "bool": {
              "must": [
                {
                  "term": {
                    "STREAM_NAME.keyword": "created_work_items"
                  }
                }
              ]
            }
          }
        ],
        "minimum_should_match": 1,
        "filter": [
          {
            "term": {
              "WORK_ITEM_TYPE.keyword": "Task"
            }
          },
          {
            "range": {
              "CREATED_DATE": {
                "gte": "now-3M/M",
                "lte": "now/M"
              }
            }
          }
        ]
      }
    }
}'
```

## Pull Request Status - 3 Month Period 
```bash
curl -X GET "http://localhost:9200/pr_index/_search?pretty" \
-H "Content-Type: application/json" \
-d '{
    "query": {
      "bool": {
        "must": [
          { "term": { "PR_STATUS.keyword": "completed" } },
          {
            "bool": {
              "should": [
                { "wildcard": { "CREATED_BY_NAME.keyword": "*shahzaib ahmed*" } },
                { "wildcard": { "CREATED_BY_NAME.keyword": "*ali ahmad*" } },
                { "wildcard": { "CREATED_BY_NAME.keyword": "*sara khan*" } }
              ],
              "minimum_should_match": 1
            }
          }
        ],
        "filter": [
          {
            "range": {
              "PR_CLOSE_DATE": {
                "gte": "now-3M/M",
                "lte": "now/M"
              }
            }
          }
        ]
      }
    }
}'
```