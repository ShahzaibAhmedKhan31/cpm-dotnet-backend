# JIRA ELASTIC SEARCH COMMANDS TO CREATE STREAMS

## PR_COMPLETED_COUNT for last 3 months query:

```
GET /pr_stream_with_comments_count/_search
{
  "query": {
    "bool": {
      "must": [
        { "term": { "PR_STATUS.keyword": "completed" } },
        { "term": { "CREATED_BY_NAME.keyword": "shahzaib ahmed" } }
      ],
      "filter": [
        {
          "range": {
            "PR_CLOSE_DATE": {
              "gte": "now-6M/M"
            }
          }
        }
      ]
    }
  },
  "aggs": {
    "pr_count_by_month": {
      "date_histogram": {
        "field": "PR_CLOSE_DATE",
        "calendar_interval": "month",
        "format": "yyyy-MM"
      }
    }
  },
  "size": 0
}

```

## COMMENTS PER PR of last 3 months:

```
GET /pr_stream_with_comments_count/_search
{
  "_source": ["LAST_MERGE_COMMIT_ID", "TOTAL_NUMBER_OF_COMMENTS", "PR_TITLE"],
  "query": {
    "bool": {
      "must": [
        { "term": { "CREATED_BY_NAME.keyword": "shahzaib ahmed" } }
      ],
      "filter": [
        {
          "range": {
            "PR_CREATION_DATE": {
              "gte": "now-3M/M",
              "lte": "now/M"
            }
          }
        }
      ]
    }
  },
  "size": 100
}

```

## Total Reviewed PR's of a person for last 3 months:

```
GET /pr_stream_with_comments_count/_search
{
  "query": {
    "bool": {
      "must": [
        { "term": { "CLOSED_BY_NAME.keyword": "shahzaib ahmed" } }
      ],
      "filter": [
        {
          "range": {
            "PR_CLOSE_DATE": {  // Replace with the correct date field for review
              "gte": "now-3M/M",
              "lte": "now/M"
            }
          }
        }
      ]
    }
  },
  "aggs": {
    "reviewed_pr_count_by_month": {
      "date_histogram": {
        "field": "PR_CLOSE_DATE",  // Replace with the correct date field for review
        "calendar_interval": "month",
        "format": "yyyy-MM"
      }
    }
  },
  "size": 0
}
```

