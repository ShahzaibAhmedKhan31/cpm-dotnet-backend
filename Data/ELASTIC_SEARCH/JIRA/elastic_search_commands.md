# JIRA ELASTIC SEARCH COMMANDS TO CREATE STREAMS

## Jira Completed + breached Issues of last 3 months

```
GET jira_completed_issues_new/_search
{
  "query": {
    "bool": {
      "filter": [
        {
          "bool": {
            "should": [
              { "term": { "LEVEL_2_ASSIGNEE.keyword": "Ebad Ahmed Siddiqui" } },
              { "term": { "LEVEL_3_ASSIGNEE.keyword": "Ebad Ahmed Siddiqui" } },
              { "term": { "LEVEL_4_ASSIGNEE.keyword": "Ebad Ahmed Siddiqui" } },
              { "term": { "LEVEL_5_ASSIGNEE.keyword": "Ebad Ahmed Siddiqui" } }
            ],
            "minimum_should_match": 1
          }
        },
        {
          "range": {
            "TIMESTAMP": {
              "gte": "now-3M/M",
              "lte": "now/M"
            }
          }
        }
      ]
    }
  },
  "aggs": {
    "monthly_data": {
      "date_histogram": {
        "field": "TIMESTAMP",
        "calendar_interval": "month",
        "format": "yyyy-MM"
      },
      "aggs": {
        "completed_issues_count": {
          "value_count": {
            "field": "TIMESTAMP"
          }
        },
        "breach_issues_count": {
          "filters": {
            "filters": {
              "level_2_breach": {
                "bool": {
                  "must": [
                    { "term": { "LEVEL_2_ASSIGNEE.keyword": "Ebad Ahmed Siddiqui" } },
                    { "term": { "L2_WORKING_ONGOINGCYCLE_BREACHED": true } }
                  ]
                }
              },
              "level_3_breach": {
                "bool": {
                  "must": [
                    { "term": { "LEVEL_3_ASSIGNEE.keyword": "Ebad Ahmed Siddiqui" } },
                    { "term": { "L3_WORKING_ONGOINGCYCLE_BREACHED": true } }
                  ]
                }
              },
              "level_4_breach": {
                "bool": {
                  "must": [
                    { "term": { "LEVEL_4_ASSIGNEE.keyword": "Ebad Ahmed Siddiqui" } },
                    { "term": { "L4_WORKING_ONGOINGCYCLE_BREACHED": true } }
                  ]
                }
              },
              "level_5_breach": {
                "bool": {
                  "must": [
                    { "term": { "LEVEL_5_ASSIGNEE.keyword": "Ebad Ahmed Siddiqui" } },
                    { "term": { "L5_DEVOPS_QA_WORKING_ONGOINGCYCLE_BREACHED": true } }
                  ]
                }
              }
            }
          }
        }
      }
    }
  },
  "size": 0
}

```


## Total Breach and Non breach issues for last 3 months

```

POST jira_completed_issues_new/_search
{
  "size": 0,
  "query": {
    "bool": {
      "filter": [
        {
          "bool": {
            "should": [
              { "term": { "LEVEL_2_ASSIGNEE.keyword": "Ebad Ahmed Siddiqui" } },
              { "term": { "LEVEL_3_ASSIGNEE.keyword": "Ebad Ahmed Siddiqui" } },
              { "term": { "LEVEL_4_ASSIGNEE.keyword": "Ebad Ahmed Siddiqui" } },
              { "term": { "LEVEL_5_ASSIGNEE.keyword": "Ebad Ahmed Siddiqui" } }
            ],
            "minimum_should_match": 1
          }
        },
        {
          "range": {
            "TIMESTAMP": {
              "gte": "now-3M/M",
              "lte": "now/M"
            }
          }
        }
      ]
    }
  },
  "aggs": {
    "issue_counts": {
      "filters": {
        "filters": {
          "breach_issues": {
            "bool": {
              "should": [
                {
                  "bool": {
                    "must": [
                      { "term": { "LEVEL_2_ASSIGNEE.keyword": "Ebad Ahmed Siddiqui" } },
                      { "term": { "L2_WORKING_ONGOINGCYCLE_BREACHED": true } }
                    ]
                  }
                },
                {
                  "bool": {
                    "must": [
                      { "term": { "LEVEL_3_ASSIGNEE.keyword": "Ebad Ahmed Siddiqui" } },
                      { "term": { "L3_WORKING_ONGOINGCYCLE_BREACHED": true } }
                    ]
                  }
                },
                {
                  "bool": {
                    "must": [
                      { "term": { "LEVEL_4_ASSIGNEE.keyword": "Ebad Ahmed Siddiqui" } },
                      { "term": { "L4_WORKING_ONGOINGCYCLE_BREACHED": true } }
                    ]
                  }
                },
                {
                  "bool": {
                    "must": [
                      { "term": { "LEVEL_5_ASSIGNEE.keyword": "Ebad Ahmed Siddiqui" } },
                      { "term": { "L5_DEVOPS_QA_WORKING_ONGOINGCYCLE_BREACHED": true } }
                    ]
                  }
                }
              ],
              "minimum_should_match": 1
            }
          },
          "non_breach_issues": {
            "bool": {
              "should": [
                {
                  "bool": {
                    "must": [
                      { "term": { "LEVEL_2_ASSIGNEE.keyword": "Ebad Ahmed Siddiqui" } },
                      { "term": { "L2_WORKING_ONGOINGCYCLE_BREACHED": false } }
                    ]
                  }
                },
                {
                  "bool": {
                    "must": [
                      { "term": { "LEVEL_3_ASSIGNEE.keyword": "Ebad Ahmed Siddiqui" } },
                      { "term": { "L3_WORKING_ONGOINGCYCLE_BREACHED": false } }
                    ]
                  }
                },
                {
                  "bool": {
                    "must": [
                      { "term": { "LEVEL_4_ASSIGNEE.keyword": "Ebad Ahmed Siddiqui" } },
                      { "term": { "L4_WORKING_ONGOINGCYCLE_BREACHED": false } }
                    ]
                  }
                },
                {
                  "bool": {
                    "must": [
                      { "term": { "LEVEL_5_ASSIGNEE.keyword": "Ebad Ahmed Siddiqui" } },
                      { "term": { "L5_DEVOPS_QA_WORKING_ONGOINGCYCLE_BREACHED": false } }
                    ]
                  }
                }
              ],
              "minimum_should_match": 1
            }
          }
        }
      }
    }
  }
}
```


## JIRA Completion Rate for last 3 months:

```
POST jira_completed_issues_new/_search
{
  "size": 0,
  "query": {
    "bool": {
      "filter": [
        {
          "bool": {
            "should": [
              { "term": { "LEVEL_2_ASSIGNEE.keyword": "Ebad Ahmed Siddiqui" } },
              { "term": { "LEVEL_3_ASSIGNEE.keyword": "Ebad Ahmed Siddiqui" } },
              { "term": { "LEVEL_4_ASSIGNEE.keyword": "Ebad Ahmed Siddiqui" } },
              { "term": { "LEVEL_5_ASSIGNEE.keyword": "Ebad Ahmed Siddiqui" } }
            ],
            "minimum_should_match": 1
          }
        },
        {
          "range": {
            "TIMESTAMP": {
              "gte": "now-3M/M",
              "lte": "now/M"
            }
          }
        }
      ]
    }
  },
  "aggs": {
    "monthly_data": {
      "date_histogram": {
        "field": "TIMESTAMP",
        "calendar_interval": "month",
        "format": "yyyy-MM"
      },
      "aggs": {
        "total_issues_non_breach": {
          "filters": {
            "filters": {
              "non_breach_issues": {
                "bool": {
                  "should": [
                    {
                      "bool": {
                        "must": [
                          { "term": { "LEVEL_2_ASSIGNEE.keyword": "Ebad Ahmed Siddiqui" } },
                          { "term": { "L2_WORKING_ONGOINGCYCLE_BREACHED": false } }
                        ]
                      }
                    },
                    {
                      "bool": {
                        "must": [
                          { "term": { "LEVEL_3_ASSIGNEE.keyword": "Ebad Ahmed Siddiqui" } },
                          { "term": { "L3_WORKING_ONGOINGCYCLE_BREACHED": false } }
                        ]
                      }
                    },
                    {
                      "bool": {
                        "must": [
                          { "term": { "LEVEL_4_ASSIGNEE.keyword": "Ebad Ahmed Siddiqui" } },
                          { "term": { "L4_WORKING_ONGOINGCYCLE_BREACHED": false } }
                        ]
                      }
                    },
                    {
                      "bool": {
                        "must": [
                          { "term": { "LEVEL_5_ASSIGNEE.keyword": "Ebad Ahmed Siddiqui" } },
                          { "term": { "L5_DEVOPS_QA_WORKING_ONGOINGCYCLE_BREACHED": false } }
                        ]
                      }
                    }
                  ],
                  "minimum_should_match": 1
                }
              }
            }
          }
        },
        "total_issues_completed": {
          "value_count": {
            "field": "TIMESTAMP"
          }
        },
        "completion_rate": {
          "bucket_script": {
            "buckets_path": {
              "non_breach": "total_issues_non_breach['non_breach_issues']._count",
              "completed": "total_issues_completed"
            },
            "script": "params.completed > 0 ? (params.non_breach / params.completed) * 100 : 0"
          }
        }
      }
    }
  }
}

```