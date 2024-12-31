CREATE STREAM work_items_created (
    resource STRUCT<
		id INTEGER,
		fields STRUCT <
			"System.AreaPath" VARCHAR,
			"System.TeamProject" VARCHAR,
			"System.IterationPath" VARCHAR,
			"System.WorkItemType" VARCHAR,
			"System.CreatedDate" VARCHAR,
			"System.CreatedBy" VARCHAR,
			"System.Parent" INTEGER,
			"System.Title" VARCHAR,
			"System.AssignedTo" VARCHAR,
			"Microsoft.VSTS.Common.Severity" VARCHAR
		>
	>	
) WITH (
    KAFKA_TOPIC = 'tfs.workitem.created',
    VALUE_FORMAT = 'JSON'
);

CREATE STREAM work_items_updated (
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
		revision STRUCT<
			fields STRUCT<
				"System.AreaPath" VARCHAR,
				"System.TeamProject" VARCHAR,
				"System.IterationPath" VARCHAR,
				"System.State" VARCHAR,
				"System.CreatedDate" VARCHAR,
				"System.ChangedDate" VARCHAR,
				"System.AssignedTo" VARCHAR,
				"Microsoft.VSTS.Common.ActivatedDate" VARCHAR,
				"System.WorkItemType" VARCHAR,
				"System.CreatedBy" VARCHAR,
				"System.Parent" INTEGER,
				"System.Title" VARCHAR,
				"Microsoft.VSTS.Common.Severity" VARCHAR,
				"Microsoft.VSTS.Scheduling.RemainingWork" INTEGER
				
			>
		>
    >
) WITH (
    KAFKA_TOPIC = 'tfs.workitem.updated',
    VALUE_FORMAT = 'JSON'
);

CREATE STREAM created_work_items AS
SELECT
	'created_work_items' AS stream_name,
	UUID() AS unique_id,
    resource->id AS id,
    resource->fields->"System.WorkItemType" AS work_item_type,
    resource->fields->"System.CreatedDate" AS created_date,
    resource->fields->"System.CreatedBy" AS created_by,
	resource->fields->"System.AssignedTo" AS assigned_to,
    resource->fields->"System.Parent" AS parent,
    resource->fields->"System.Title" AS title,
	resource->fields->"Microsoft.VSTS.Common.Severity" AS severity
FROM work_items_created EMIT CHANGES;


CREATE STREAM inprogress_work_items AS
SELECT
	'inprogress_work_items' AS stream_name,
	UUID() AS unique_id,
    RESOURCE->WORKITEMID AS work_item_id,
    RESOURCE->REVISION->FIELDS->"Microsoft.VSTS.Scheduling.RemainingWork" AS remaining_work,
    RESOURCE->REVISION->FIELDS->"System.AssignedTo" AS assigned_to,
    RESOURCE->FIELDS->"System.ChangedDate"->NEWVALUE AS inprogress_date,
	RESOURCE->REVISION->FIELDS->"System.CreatedDate" AS created_date,
	RESOURCE->REVISION->FIELDS->"System.CreatedBy" AS created_by,
	RESOURCE->REVISION->FIELDS->"Microsoft.VSTS.Common.Severity" AS severity,
	RESOURCE->REVISION->FIELDS->"System.WorkItemType" AS work_item_type
FROM work_items_updated
WHERE
    (RESOURCE->FIELDS->"System.State"->OLDVALUE = 'New' OR RESOURCE->FIELDS->"System.State"->OLDVALUE = 'To Do') AND 
    (RESOURCE->FIELDS->"System.State"->NEWVALUE = 'Active' OR RESOURCE->FIELDS->"System.State"->NEWVALUE = 'In Progress')
EMIT CHANGES;


CREATE STREAM completed_work_items AS
SELECT
	'completed_work_items' AS stream_name,
	UUID() AS unique_id,
    RESOURCE->WORKITEMID AS work_item_id,
    RESOURCE->REVISION->FIELDS->"Microsoft.VSTS.Scheduling.RemainingWork" AS remaining_work,
    RESOURCE->REVISION->FIELDS->"System.AssignedTo" AS assigned_to,
    RESOURCE->FIELDS->"System.ChangedDate"->NEWVALUE AS closed_date,
	RESOURCE->REVISION->FIELDS->"System.CreatedDate" AS created_date,
	RESOURCE->REVISION->FIELDS->"System.CreatedBy" AS created_by,
	RESOURCE->REVISION->FIELDS->"Microsoft.VSTS.Common.Severity" AS severity,
	RESOURCE->REVISION->FIELDS->"System.WorkItemType" AS work_item_type
FROM work_items_updated
WHERE
    (RESOURCE->FIELDS->"System.State"->OLDVALUE = 'Active' OR RESOURCE->FIELDS->"System.State"->OLDVALUE = 'In Progress') AND 
    (RESOURCE->FIELDS->"System.State"->NEWVALUE = 'Closed' OR RESOURCE->FIELDS->"System.State"->NEWVALUE = 'Done')
EMIT CHANGES;


CREATE TABLE work_items_assignees AS
SELECT
    id AS workitemid,
    LATEST_BY_OFFSET(assigned_to) AS assignee
FROM created_work_items
GROUP BY id EMIT CHANGES;


CREATE STREAM enriched_work_items AS
SELECT
    cwi.stream_name,
    cwi.unique_id,
    cwi.id,
    cwi.work_item_type,
    cwi.created_date,
    cwi.created_by,
    cwi.assigned_to AS current_assignee,
    cwi.parent AS parent_workitemid,
    parent_table.assignee AS parent_assignee,
    cwi.title,
    cwi.severity
FROM created_work_items cwi
LEFT JOIN work_items_assignees parent_table
ON cwi.parent = parent_table.workitemid
EMIT CHANGES;


CREATE STREAM enriched_work_items_partitioned AS
SELECT *
FROM enriched_work_items
PARTITION BY unique_id
EMIT CHANGES;

CREATE STREAM inprogress_work_items_partitioned AS
SELECT *
FROM inprogress_work_items
PARTITION BY unique_id
EMIT CHANGES;


CREATE STREAM completed_work_items_partitioned AS
SELECT *
FROM completed_work_items
PARTITION BY unique_id
EMIT CHANGES;