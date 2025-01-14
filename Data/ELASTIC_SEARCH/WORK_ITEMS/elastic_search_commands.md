## bug count against workitem
curl -X POST "http://cpm.eastus.cloudapp.azure.com/tfs_index/_search/elasticsearch" -H "Content-Type: application/json" -d'
{
  "size": 0,
  "query": {
    "bool": {
      "must": [
        { 
          "term": { 
            "STREAM_NAME.keyword": "created_work_items" 
          }
        },
        { 
          "wildcard": { 
            "PARENT_ASSIGNEE.keyword": "*hamza01961@gmail.com*" 
          }
        },
        {
          "range": {
            "CREATED_DATE": {
              "gte": "now-1M/M",  // filter for documents created in the last month
              "lte": "now/M"      // up to the current month
            }
          }
        }
      ]
    }
  },
  "aggs": {
    "workitem_bugs_count": {
      "terms": {
        "field": "PARENT_WORKITEMID",
        "size": 10000
      },
      "aggs": {
        "bugs": {
          "filter": {
            "term": {
              "WORK_ITEM_TYPE.keyword": "Bug"
            }
          },
          "aggs": {
            "parent_assignee": {
              "top_hits": {
                "_source": {
                  "includes": ["PARENT_ASSIGNEE"]
                },
                "size": 1
              }
            }
          }
        }
      }
    }
  }
}'

## count of bug assigned, bug completed
curl -X POST "http://172.174.172.29:9200/tfs_index/_search?pretty" -H 'Content-Type: application/json' -d '{
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
                "term": {
                  "CURRENT_ASSIGNEE.keyword": "hamza01961 <hamza01961@gmail.com>"
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
                "term": {
                  "ASSIGNED_TO.keyword": "hamza01961 <hamza01961@gmail.com>"
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
  },
  "aggs": {
    "months": {
      "date_histogram": {
        "field": "CREATED_DATE",
        "calendar_interval": "month",
        "format": "yyyy-MM"
      },
      "aggs": {
        "bugs_assigned": {
          "filter": {
            "term": {
              "STREAM_NAME.keyword": "created_work_items"
            }
          }
        },
        "bugs_completed": {
          "filter": {
            "term": {
              "STREAM_NAME.keyword": "completed_work_items"
            }
          }
        }
      }
    }
  }
}'

## count of task assigned and task completed month wise

curl -X POST "http://172.174.172.29:9200/tfs_index/_search?pretty" -H 'Content-Type: application/json' -d '{
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
                "wildcard": {
                  "CURRENT_ASSIGNEE.keyword": "*hamza01961@gmail.com*"
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
                "wildcard": {
                  "ASSIGNED_TO.keyword": "*hamza01961@gmail.com*"
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
  },
  "aggs": {
    "months": {
      "date_histogram": {
        "field": "CREATED_DATE",
        "calendar_interval": "month",
        "format": "yyyy-MM"
      },
      "aggs": {
        "tasks_assigned": {
          "filter": {
            "term": {
              "STREAM_NAME.keyword": "created_work_items"
            }
          }
        },
        "tasks_completed": {
          "filter": {
            "term": {
              "STREAM_NAME.keyword": "completed_work_items"
            }
          }
        }
      }
    }
  }
}'

## task completion rate
curl -X POST "http://localhost:9200/tfs_index/_search?pretty" -H 'Content-Type: application/json' -d '{
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
                "wildcard": {
                  "CURRENT_ASSIGNEE.keyword": "*hamza01961@gmail.com*"
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
                "wildcard": {
                  "ASSIGNED_TO.keyword": "*hamza01961@gmail.com*"
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
  },
  "aggs": {
    "months": {
      "date_histogram": {
        "field": "CREATED_DATE",
        "calendar_interval": "month",
        "format": "yyyy-MM"
      },
      "aggs": {
        "tasks_assigned": {
          "filter": {
            "term": {
              "STREAM_NAME.keyword": "created_work_items"
            }
          }
        },
        "tasks_completed": {
          "filter": {
            "term": {
              "STREAM_NAME.keyword": "completed_work_items"
            }
          }
        },
        "completion_rate": {
          "bucket_script": {
            "buckets_path": {
              "assigned": "tasks_assigned._count",
              "completed": "tasks_completed._count"
            },
            "script": "params.assigned > 0 ? (params.completed / params.assigned) * 100 : 0"
          }
        }
      }
    }
  }
}'