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
  resource->lastMergeCommit->commitId as last_merge_commit_id
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