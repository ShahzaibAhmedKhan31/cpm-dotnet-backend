using Newtonsoft.Json;
using System.Collections.Generic;
namespace WorkItemBugCountResponse.Models
{
public class WorkItemBugCountResponse
{
    [JsonProperty("took")]
    public int Took { get; set; }

    [JsonProperty("timed_out")]
    public bool TimedOut { get; set; }

    [JsonProperty("_shards")]
    public ShardsInfo Shards { get; set; }

    [JsonProperty("hits")]
    public HitsInfo Hits { get; set; }

    [JsonProperty("aggregations")]
    public Aggregations Aggregations { get; set; }
}

public class ShardsInfo
{
    [JsonProperty("total")]
    public int Total { get; set; }

    [JsonProperty("successful")]
    public int Successful { get; set; }

    [JsonProperty("skipped")]
    public int Skipped { get; set; }

    [JsonProperty("failed")]
    public int Failed { get; set; }
}

public class HitsInfo
{
    [JsonProperty("total")]
    public TotalHits Total { get; set; }

    [JsonProperty("max_score")]
    public object MaxScore { get; set; } // Replace with actual type if known

    [JsonProperty("hits")]
    public List<object> Hits { get; set; } // Replace with actual type if known
}

public class TotalHits
{
    [JsonProperty("value")]
    public int Value { get; set; }

    [JsonProperty("relation")]
    public string Relation { get; set; }
}

public class Aggregations
{
    [JsonProperty("workitem_bugs_count")]
    public WorkitemBugsCount WorkitemBugsCount { get; set; }
}

public class WorkitemBugsCount
{
    [JsonProperty("doc_count_error_upper_bound")]
    public int DocCountErrorUpperBound { get; set; }

    [JsonProperty("sum_other_doc_count")]
    public int SumOtherDocCount { get; set; }

    [JsonProperty("buckets")]
    public List<BugBucket> Buckets { get; set; }
}

public class BugBucket
{
    [JsonProperty("key")]
    public int Key { get; set; }

    [JsonProperty("doc_count")]
    public int DocCount { get; set; }

    [JsonProperty("bugs")]
    public BugDetails Bugs { get; set; }
}

public class BugDetails
{
    [JsonProperty("doc_count")]
    public int DocCount { get; set; }

    [JsonProperty("parent_assignee")]
    public ParentAssignee ParentAssignee { get; set; }
}

public class ParentAssignee
{
    [JsonProperty("hits")]
    public ParentAssigneeHits Hits { get; set; }
}

public class ParentAssigneeHits
{
    [JsonProperty("total")]
    public TotalHits Total { get; set; }

    [JsonProperty("max_score")]
    public double MaxScore { get; set; }

    [JsonProperty("hits")]
    public List<ParentAssigneeHit> Hits { get; set; }
}

public class ParentAssigneeHit
{
    [JsonProperty("_index")]
    public string Index { get; set; }

    [JsonProperty("_id")]
    public string Id { get; set; }

    [JsonProperty("_score")]
    public double Score { get; set; }

    [JsonProperty("_source")]
    public ParentAssigneeSource Source { get; set; }
}

public class ParentAssigneeSource
{
    [JsonProperty("PARENT_ASSIGNEE")]
    public string ParentAssignee { get; set; }
}
}