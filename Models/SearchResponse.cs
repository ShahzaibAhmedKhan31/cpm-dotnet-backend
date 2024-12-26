using Newtonsoft.Json;

public class SearchResponse
{
    public int Took { get; set; }
    public bool TimedOut { get; set; }
    public ShardInfo Shards { get; set; }
    public HitsInfo Hits { get; set; }
    public Aggregations Aggregations { get; set; }
}

public class ShardInfo
{
    public int Total { get; set; }
    public int Successful { get; set; }
    public int Skipped { get; set; }
    public int Failed { get; set; }
}

public class HitsInfo
{
    public TotalHits Total { get; set; }
    public object MaxScore { get; set; }
    public List<object> Hits { get; set; } // You can replace this with the actual object type if you expect hits to contain data
}

public class TotalHits
{
    public int Value { get; set; }
    public string Relation { get; set; }
}

public class Aggregations
{
    public MonthlyAggregation Months { get; set; }
}

public class MonthlyAggregation
{
    [JsonProperty("buckets")]
    public List<MonthBucket> Buckets { get; set; }
}

public class MonthBucket
{
    [JsonProperty("key_as_string")]
    public string Date { get; set; }

    [JsonProperty("key")]
    public long Key { get; set; }

    [JsonProperty("doc_count")]
    public int DocCount { get; set; }

    [JsonProperty("bugs_assigned")]
    public DocCountAggregation BugsAssigned { get; set; }

    [JsonProperty("bugs_completed")]
    public DocCountAggregation BugsCompleted { get; set; }
}

public class DocCountAggregation
{
    [JsonProperty("doc_count")]
    public int DocCount { get; set; }
}
