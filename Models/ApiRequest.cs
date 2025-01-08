// Models/SearchByIDRequest.cs
namespace ApiRequest.Models
{
    public class SearchByIDRequest
    {
        public string? Index { get; set; }
        public string? Id { get; set; }
    }

    public class SearchByUserDateRequest
    {
        public string? Index { get; set; }
        public string? UserName { get; set; }
        public string? Date { get; set; }
    }

    public class SearchRequest
    {
        public string ?Email { get; set; }
        public int Months { get; set; }
    }

    public class SearchPrDetails
    {
        public int WorkItemId { get; set; }
    }
}
