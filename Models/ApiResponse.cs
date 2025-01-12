// Models/ApiResponse.cs
namespace ApiResponse.Models
{
    



    public class GetPrDetailsApi
    {
    public string? CreatedByEmail { get; set; }
    public string? CreatedDate { get; set; }
    public string? CreatedByName { get; set; }
    public int WorkItemId { get; set; }
    public int PrId { get; set; }
    public string? Title { get; set; }
    public string? TotalNumberOfComments { get; set; }
    public string? LastMergeCommitId { get; set; }
    public string? PrClosedDate { get; set; }
    public string? PrClosedByName { get; set; }
    public string? PrFirstCommentDate { get; set; }
    public string? PrStatus { get; set; }

    }
	
	

}