# JIRA KSQL DB COMMANDS TO CREATE STREAMS


## Command Number 1: (Unnormalized Stream for jira updated issues)

```
CREATE STREAM jira_issues_unnormalised(
  timestamp BIGINT,
  webhookEvent VARCHAR,
  issue_event_type_name VARCHAR,
  issue STRUCT<
    key VARCHAR,
	fields STRUCT<
		priority STRUCT <
			name VARCHAR,
			id VARCHAR
		>,
		summary VARCHAR,
		status STRUCT<
			statusCategory STRUCT<
				key VARCHAR,
				name VARCHAR
			>
		>,
		customfield_10270 STRUCT<
			displayName VARCHAR
		>,
		customfield_10271 STRUCT<
			displayName VARCHAR
		>,
		customfield_10272 STRUCT<
			displayName VARCHAR
		>,
		customfield_10273 STRUCT<
			displayName VARCHAR
		>,
		customfield_10365 VARCHAR,
		customfield_10366 VARCHAR,
		customfield_10152 STRUCT<
			id VARCHAR,
			name VARCHAR,
			ongoingCycle STRUCT<
				breached BOOLEAN,
				goalDuration STRUCT<
					millis BIGINT,
					friendly VARCHAR
				>,
				elapsedTime STRUCT<
					millis BIGINT,
					friendly VARCHAR
				>,
				remainingTime STRUCT<
					millis BIGINT,
					friendly VARCHAR
				>
			>
		>,
		customfield_10286 STRUCT<
			id VARCHAR,
			name VARCHAR,
			ongoingCycle STRUCT<
				breached BOOLEAN,
				goalDuration STRUCT<
					millis BIGINT,
					friendly VARCHAR
				>,
				elapsedTime STRUCT<
					millis BIGINT,
					friendly VARCHAR
				>,
				remainingTime STRUCT<
					millis BIGINT,
					friendly VARCHAR
				>
			>
		>,
		customfield_10287 STRUCT<
			id VARCHAR,
			name VARCHAR,
			ongoingCycle STRUCT<
				breached BOOLEAN,
				goalDuration STRUCT<
					millis BIGINT,
					friendly VARCHAR
				>,
				elapsedTime STRUCT<
					millis BIGINT,
					friendly VARCHAR
				>,
				remainingTime STRUCT<
					millis BIGINT,
					friendly VARCHAR
				>
			>
		>,
		customfield_10324 STRUCT<
			id VARCHAR,
			name VARCHAR,
			ongoingCycle STRUCT<
				breached BOOLEAN,
				goalDuration STRUCT<
					millis BIGINT,
					friendly VARCHAR
				>,
				elapsedTime STRUCT<
					millis BIGINT,
					friendly VARCHAR
				>,
				remainingTime STRUCT<
					millis BIGINT,
					friendly VARCHAR
				>
			>
		>,
		customfield_10283 STRUCT<
			id VARCHAR,
			value VARCHAR
		>	
	>
 >,
 changelog STRUCT<
	id STRING,
	items ARRAY<STRUCT<
	field STRING,
	fromString STRING,
	toString STRING
	>>
	>
) WITH (
  KAFKA_TOPIC='jira-issue_updated',
  VALUE_FORMAT='JSON'
);
```


## Command Number 2: (Transformed Stream for jira updated issues):

```
CREATE STREAM jira_transformed_issue AS
SELECT 
  TIMESTAMP,
  ISSUE->fields->status->statusCategory->name AS current_status,
  ISSUE->KEY AS issue_key,
  EXPLODE(CHANGELOG->ITEMS) AS changelog,
  ISSUE->fields->customfield_10152 AS l2_working,
  ISSUE->fields->customfield_10286 AS l3_working,
  ISSUE->fields->customfield_10287 AS l4_working,
  ISSUE->fields->customfield_10324 AS l5_devops_qa_working,
  ISSUE->fields->customfield_10270 AS level_2_assignee,
  ISSUE->fields->customfield_10271 AS level_3_assignee,
  ISSUE->fields->customfield_10272 AS level_4_assignee,
  ISSUE->fields->customfield_10273 AS level_5_assignee,
  ISSUE->fields->customfield_10365 AS L2_Resolution,
  ISSUE->fields->customfield_10366 AS L3_Resolution,
  ISSUE->fields->customfield_10283 AS SeverityWiseCategory,
  ISSUE->fields->priority AS priority,
  ISSUE->fields->summary AS summary
FROM jira_issues_unnormalised
EMIT CHANGES;
```

## Command Number 3: (Completed Issues Stream for jira updated issues):

```
CREATE STREAM Jira_completed_issues AS
SELECT
    TIMESTAMPTOSTRING(TIMESTAMP, 'yyyy-MM-dd''T''HH:mm:ss.SSS''Z''') AS TIMESTAMP,  -- Convert to UTC format
    CURRENT_STATUS,
    ISSUE_KEY,
    CHANGELOG->FIELD AS CHANGELOG_FIELD,
    CHANGELOG->FROMSTRING AS CHANGELOG_FROMSTRING,
    CHANGELOG->TOSTRING AS CHANGELOG_TOSTRING,
    L2_WORKING->ONGOINGCYCLE->BREACHED AS L2_WORKING_ONGOINGCYCLE_BREACHED,
    L2_WORKING->ONGOINGCYCLE->GOALDURATION->MILLIS AS L2_WORKING_ONGOINGCYCLE_GOALDURATION_MILLIS,
    L2_WORKING->ONGOINGCYCLE->ELAPSEDTIME->MILLIS AS L2_WORKING_ONGOINGCYCLE_ELAPSEDTIME_MILLIS,
    L2_WORKING->ONGOINGCYCLE->REMAININGTIME->MILLIS AS L2_WORKING_ONGOINGCYCLE_REMAININGTIME_MILLIS,
    LEVEL_2_ASSIGNEE->DISPLAYNAME AS LEVEL_2_ASSIGNEE,
    L3_WORKING->ONGOINGCYCLE->BREACHED AS L3_WORKING_ONGOINGCYCLE_BREACHED,
    L3_WORKING->ONGOINGCYCLE->GOALDURATION->MILLIS AS L3_WORKING_ONGOINGCYCLE_GOALDURATION_MILLIS,
    L3_WORKING->ONGOINGCYCLE->ELAPSEDTIME->MILLIS AS L3_WORKING_ONGOINGCYCLE_ELAPSEDTIME_MILLIS,
    L3_WORKING->ONGOINGCYCLE->REMAININGTIME->MILLIS AS L3_WORKING_ONGOINGCYCLE_REMAININGTIME_MILLIS,
    LEVEL_3_ASSIGNEE->DISPLAYNAME AS LEVEL_3_ASSIGNEE,
    L4_WORKING->ONGOINGCYCLE->BREACHED AS L4_WORKING_ONGOINGCYCLE_BREACHED,
    L4_WORKING->ONGOINGCYCLE->GOALDURATION->MILLIS AS L4_WORKING_ONGOINGCYCLE_GOALDURATION_MILLIS,
    L4_WORKING->ONGOINGCYCLE->ELAPSEDTIME->MILLIS AS L4_WORKING_ONGOINGCYCLE_ELAPSEDTIME_MILLIS,
    L4_WORKING->ONGOINGCYCLE->REMAININGTIME->MILLIS AS L4_WORKING_ONGOINGCYCLE_REMAININGTIME_MILLIS,
    LEVEL_4_ASSIGNEE->DISPLAYNAME AS LEVEL_4_ASSIGNEE,
    L5_DEVOPS_QA_WORKING->ONGOINGCYCLE->BREACHED AS L5_DEVOPS_QA_WORKING_ONGOINGCYCLE_BREACHED,
    L5_DEVOPS_QA_WORKING->ONGOINGCYCLE->GOALDURATION->MILLIS AS L5_DEVOPS_QA_WORKING_ONGOINGCYCLE_GOALDURATION_MILLIS,
    L5_DEVOPS_QA_WORKING->ONGOINGCYCLE->ELAPSEDTIME->MILLIS AS L5_DEVOPS_QA_WORKING_ONGOINGCYCLE_ELAPSEDTIME_MILLIS,
    L5_DEVOPS_QA_WORKING->ONGOINGCYCLE->REMAININGTIME->MILLIS AS L5_DEVOPS_QA_WORKING_ONGOINGCYCLE_REMAININGTIME_MILLIS,
    LEVEL_5_ASSIGNEE->DISPLAYNAME AS LEVEL_5_ASSIGNEE,
    SEVERITYWISECATEGORY->VALUE AS SEVERITYWISECATEGORY_VALUE,
    PRIORITY->NAME AS PRIORITY_NAME,
    SUMMARY
FROM jira_transformed_issue
WHERE CHANGELOG->FIELD = 'status' AND CHANGELOG->TOSTRING = 'Close'
EMIT CHANGES;
```


