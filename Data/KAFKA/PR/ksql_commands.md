# Pull Request KSQL DB COMMANDS TO CREATE STREAMS

## Command Numnber 1 (Create PR Unnormalized Data Stream):

```
CREATE STREAM PR_UNNORMALIZED(
  subscriptionId STRING,
  notificationId INT,
  id STRING,
  eventType STRING,
  publisherId STRING,
  message STRUCT <
    text STRING,
    html STRING,
    markdown STRING
  >,
  detailedMessage STRUCT <
    text STRING,
    html STRING,
    markdown STRING
  >,
  resource STRUCT <
    repository STRUCT <
      id STRING,
      name STRING,
      url STRING,
      project STRUCT <
        id STRING,
        name STRING,
        url STRING,
        state STRING,
        revision INT,
        visibility STRING,
        lastUpdateTime STRING
      >,
      remoteUrl STRING,
      sshUrl STRING,
      webUrl STRING,
      isDisabled BOOLEAN,
      isInMaintenance BOOLEAN
    >,
    pullRequestId INT,
    codeReviewId INT,
    status STRING,
    createdBy STRUCT <
      displayName STRING,
      url STRING,
      _links STRUCT <
        avatar STRUCT <
          href STRING
        >
      >,
      id STRING,
      uniqueName STRING,
      imageUrl STRING,
      descriptor STRING
    >,
    creationDate STRING,
    closedDate STRING,
    title STRING,
    description STRING,
    sourceRefName STRING,
    targetRefName STRING,
    mergeStatus STRING,
    isDraft BOOLEAN,
    mergeId STRING,
    closedBy STRUCT <
      displayName STRING,
      url STRING,
      _links STRUCT <
        avatar STRUCT <
          href STRING
        >
      >,
      id STRING,
      uniqueName STRING,
      imageUrl STRING,
      descriptor STRING
    >,
    lastMergeSourceCommit STRUCT <
      commitId STRING,
      url STRING
    >,
    lastMergeTargetCommit STRUCT <
      commitId STRING,
      url STRING
    >,
    lastMergeCommit STRUCT <
      commitId STRING,
      author STRUCT <
        name STRING,
        email STRING,
        date STRING
      >,
      committer STRUCT <
        name STRING,
        email STRING,
        date STRING
      >,
      comment STRING,
      commentTruncated BOOLEAN,
      url STRING
    >,
    reviewers ARRAY<STRUCT <
      reviewerUrl STRING,
      vote INT,
      hasDeclined BOOLEAN,
      isFlagged BOOLEAN,
      displayName STRING,
      url STRING,
      _links STRUCT <
        avatar STRUCT <
          href STRING
        >
      >,
      id STRING,
      uniqueName STRING,
      imageUrl STRING
    >>,
    completionOptions STRUCT <
      mergeCommitMessage STRING,
      deleteSourceBranch BOOLEAN,
      mergeStrategy STRING,
      transitionWorkItems BOOLEAN,
      autoCompleteIgnoreConfigIds ARRAY<STRING>
    >
  >,
  resourceVersion STRING,
  resourceContainers STRUCT <
    collection STRUCT <
      id STRING,
      baseUrl STRING
    >,
    account STRUCT <
      id STRING,
      baseUrl STRING
    >,
    project STRUCT <
      id STRING,
      baseUrl STRING
    >
  >,
  createdDate STRING
) WITH (
  KAFKA_TOPIC='tfs.git.pullrequest.merged',
  VALUE_FORMAT='JSON'
);
```


## Command Number 2 (Create PR Comments Unnormalized Data Stream):

```
CREATE STREAM PR_COMMENTS_UNNORMALIZED (
  subscriptionId STRING,
  notificationId INT,
  id STRING,
  eventType STRING,
  publisherId STRING,
  message STRUCT <
    text STRING,
    html STRING,
    markdown STRING
  >,
  detailedMessage STRUCT <
    text STRING,
    html STRING,
    markdown STRING
  >,
  resource STRUCT <
    comment STRUCT <
      id INT,
      parentCommentId INT,
      author STRUCT <
        displayName STRING,
        url STRING,
        _links STRUCT <
          avatar STRUCT <
            href STRING
          >
        >,
        id STRING,
        uniqueName STRING,
        imageUrl STRING,
        descriptor STRING
      >,
      content STRING,
      publishedDate STRING,
      lastUpdatedDate STRING,
      lastContentUpdatedDate STRING,
      commentType STRING,
      usersLiked ARRAY<STRING>,
      _links STRUCT <
        self STRUCT <
          href STRING
        >,
        repository STRUCT <
          href STRING
        >,
        threads STRUCT <
          href STRING
        >,
        pullRequests STRUCT <
          href STRING
        >
      >
    >,
    pullRequest STRUCT <
      repository STRUCT <
        id STRING,
        name STRING,
        url STRING,
        project STRUCT <
          id STRING,
          name STRING,
          url STRING,
          state STRING,
          revision INT,
          visibility STRING,
          lastUpdateTime STRING
        >,
        remoteUrl STRING,
        sshUrl STRING,
        webUrl STRING,
        isDisabled BOOLEAN,
        isInMaintenance BOOLEAN
      >,
      pullRequestId INT,
      codeReviewId INT,
      status STRING,
      createdBy STRUCT <
        displayName STRING,
        url STRING,
        _links STRUCT <
          avatar STRUCT <
            href STRING
          >
        >,
        id STRING,
        uniqueName STRING,
        imageUrl STRING,
        descriptor STRING
      >,
      creationDate STRING,
      title STRING,
      description STRING,
      sourceRefName STRING,
      targetRefName STRING,
      mergeStatus STRING,
      isDraft BOOLEAN,
      mergeId STRING,
      lastMergeSourceCommit STRUCT <
        commitId STRING,
        url STRING
      >,
      lastMergeTargetCommit STRUCT <
        commitId STRING,
        url STRING
      >,
      lastMergeCommit STRUCT <
        commitId STRING,
        author STRUCT <
          name STRING,
          email STRING,
          date STRING
        >,
        committer STRUCT <
          name STRING,
          email STRING,
          date STRING
        >,
        comment STRING,
        url STRING
      >,
      reviewers ARRAY<STRUCT <
        reviewerUrl STRING,
        vote INT,
        hasDeclined BOOLEAN,
        isFlagged BOOLEAN,
        displayName STRING,
        url STRING,
        _links STRUCT <
          avatar STRUCT <
            href STRING
          >
        >,
        id STRING,
        uniqueName STRING,
        imageUrl STRING
      >>,
      url STRING,
      supportsIterations BOOLEAN,
      artifactId STRING
    >
  >,
  resourceVersion STRING,
  resourceContainers STRUCT <
    collection STRUCT <
      id STRING,
      baseUrl STRING
    >,
    account STRUCT <
      id STRING,
      baseUrl STRING
    >,
    project STRUCT <
      id STRING,
      baseUrl STRING
    >
  >,
  createdDate STRING
) WITH (
  KAFKA_TOPIC = 'tfs.ms.vss-code.git-pullrequest-comment-event',
  VALUE_FORMAT = 'JSON'
);
```

## Command Number 3 (Create PR Completed Stream):

```
CREATE STREAM PR_COMPLETED AS 
SELECT
  UUID() AS unique_id,
  resource->pullRequestId AS PR_ID,
  resource->createdBy->displayName AS createdByName,
  resource->title AS PR_TITLE,
  resource->creationDate AS PR_Creation_Date,
  resource->closedDate AS PR_Close_Date,
  resource->status AS PR_Status,
  resource->closedBy->displayName AS closedByName,
  resource->lastMergeCommit->commitId as last_merge_commit_id,
  resource->completionOptions->mergeCommitMessage as mergeCommitMessage
FROM PR_UNNORMALIZED
WHERE resource->status = 'completed'
EMIT CHANGES;
```

## Command Number 4 (Create PR Comments Stream):

```
CREATE STREAM PR_COMMENTS AS
SELECT 
    resource->pullRequest->pullRequestId AS PR_ID,
    resource->comment->content AS Comment,
    resource->comment->publishedDate as comment_published_date
FROM 
    PR_COMMENTS_UNNORMALIZED
EMIT CHANGES;
```

### Command Number 5 (Create PR_COMMENTS_COUNT Stream):

```
CREATE TABLE PR_COMMENTS_COUNT AS
SELECT 
    PR_ID,
    COUNT(*) AS total_number_of_comments,
	  MIN(COMMENT_PUBLISHED_DATE) AS first_comment_date
FROM 
    PR_COMMENTS
GROUP BY
    PR_ID
EMIT CHANGES;
```

### Command Number 6 (Create Finalized PR Stream with comments count):

```
CREATE STREAM PR_COMPLETED_STREAM AS 
SELECT
  'PR_COMPLETED_STREAM' AS TABLE_NAME,
  S.unique_id AS unique_id,
  S.PR_ID AS PR_ID,
  S.createdByName AS created_By_Name,
  S.PR_TITLE AS PR_TITLE,
  S.PR_Creation_Date AS PR_Creation_Date,
  S.PR_Close_Date AS PR_Close_Date,
  S.PR_Status AS PR_Status,
  S.ClosedByName AS Closed_By_Name,
  S.last_merge_commit_id AS last_merge_commit_id,
  COALESCE(T.Total_Number_Of_Comments, CAST(0 AS BIGINT)) AS Total_Number_Of_Comments,
  T.first_comment_date AS PR_first_comment_date
FROM 
  PR_COMPLETED S
LEFT OUTER JOIN 
  PR_COMMENTS_COUNT T 
ON 
  S.PR_ID = T.PR_ID
PARTITION BY S.unique_id
EMIT CHANGES;
```

### Command Number 7 (Create Denormalized PR_CREATED Stream):

```
CREATE STREAM PR_CREATED_UNNORMALIZED (
  publisherId STRING,
  resource STRUCT<
    repository STRUCT<
      id STRING,
      name STRING,
      url STRING,
      project STRUCT<
        id STRING,
        name STRING,
        url STRING
      >
    >,
	createdBy STRUCT<
        displayName STRING,
        uniqueName STRING
    >,
    title STRING,
    pullRequestId INT,
    artifactId STRING
  >,
  createdDate STRING
) WITH (
  KAFKA_TOPIC='tfs.git.pullrequest.created',
  VALUE_FORMAT='JSON'
);
```

### Command Number 8 (Create Normalized PR_CREATED Stream):

```
CREATE STREAM PR_CREATED AS
SELECT
  LCASE(REGEXP_REPLACE(resource->artifactId, '.*%', '')) AS Artifacts_link,
  resource->pullRequestId AS PR_ID,
  resource->createdBy->displayName AS createdByName,
  resource->createdBy->uniqueName AS createdByEmail,
  resource->title AS title,
  createdDate,
  publisherId
FROM PR_CREATED_UNNORMALIZED
EMIT CHANGES;
```
### Command Number 9 (Create Normalized PR_CREATED Table):

```
CREATE TABLE PR_CREATED_TABLE AS
SELECT
  Artifacts_link,
  LATEST_BY_OFFSET(PR_ID) AS PR_ID,
  LATEST_BY_OFFSET(createdByName) AS createdByName,
  LATEST_BY_OFFSET(createdByEmail) AS createdByEmail,
  LATEST_BY_OFFSET(title) AS title,
  LATEST_BY_OFFSET(createdDate) AS createdDate,
  LATEST_BY_OFFSET(publisherId) AS publisherId
FROM PR_CREATED
GROUP BY Artifacts_link
EMIT CHANGES;
```

### Command Number 10 (Create Denormalized Work_item_pr Stream):

```
CREATE STREAM work_items_updated_1 (
    resource STRUCT<
        workItemId INTEGER,
		fields STRUCT<
			"System.State" STRUCT<
				oldValue VARCHAR, 
				newValue VARCHAR
>,
			"System.ChangedDate" STRUCT<
				oldValue VARCHAR,
				newValue VARCHAR
>
>,
		relations STRUCT<
			added ARRAY<STRUCT<
                rel VARCHAR,
                url VARCHAR,
                attributes STRUCT<
                    authorizedDate VARCHAR,
                    id INTEGER,
                    resourceCreatedDate VARCHAR,
                    resourceModifiedDate VARCHAR,
                    revisedDate VARCHAR,
                    name VARCHAR
>
>>
>
>
) WITH (
    KAFKA_TOPIC = 'tfs.workitem.updated',
    VALUE_FORMAT = 'JSON'
);
```

### Command Number 11 (Create Normalized Work_item_pr Stream):

```
CREATE STREAM workitem_pr_exploded AS
SELECT
	'workitem_pr_exploded' AS stream_name,
	UUID() AS unique_id,
	RESOURCE->WORKITEMID AS work_item_id,
	EXPLODE(resource->relations->added) AS artifacts
FROM work_items_updated_1
EMIT CHANGES;
```

### Command Number 12 (Create Conditional Work_item_pr Stream):

```
CREATE STREAM workitem_pr_artifacts_stream AS
SELECT
	work_item_id,
	LCASE(REGEXP_REPLACE(ARTIFACTS->URL, '.*%', '')) AS artifacts_link
FROM workitem_pr_exploded
WHERE ARTIFACTS->REL = 'ArtifactLink' AND ARTIFACTS->ATTRIBUTES->NAME = 'Pull Request'
EMIT CHANGES;

```

### Command Number 13 (Create Conditional Work_item_pr Table):

```
CREATE TABLE workitem_pr_artifacts_table AS
SELECT
	ARTIFACTS_LINK,
	LATEST_BY_OFFSET(WORK_ITEM_ID) AS WORK_ITEM_ID
FROM workitem_pr_artifacts_stream
GROUP BY ARTIFACTS_LINK
EMIT CHANGES;
```

### Command Number 14 (Create JOINED PR_WORKITEM_TABLE table):

```
CREATE TABLE PR_WORKITEM_TABLE AS 
SELECT
  'PR_WORKITEM_TABLE' AS TABLE_NAME,
  S.PR_ID AS PR_ID,
  T.WORK_ITEM_ID AS WORK_ITEM_ID,
  S.CREATEDBYNAME AS CREATEDBYNAME,
  S.CREATEDBYEMAIL AS CREATEDBYEMAIL,
  S.TITLE AS TITLE,
  S.CREATEDDATE AS CREATEDDATE,
  S.PUBLISHERID AS PUBLISHERID,
  S.ARTIFACTS_LINK AS ARTIFACTS_LINK_P,
  T.ARTIFACTS_LINK AS ARTIFACTS_LINK_W
FROM PR_CREATED_TABLE S
LEFT JOIN WORKITEM_PR_ARTIFACTS_TABLE T
ON S.ARTIFACTS_LINK = T.ARTIFACTS_LINK
EMIT CHANGES;
```





