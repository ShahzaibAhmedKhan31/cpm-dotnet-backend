
using Microsoft.EntityFrameworkCore;
using WebApplication1.dbdata;

public class EmployeesService
{
    private readonly HttpClient _httpClient;
    private readonly ApplicationDbContext _context;

    public EmployeesService(HttpClient httpClient, ApplicationDbContext context)
    {
        _httpClient = httpClient;
        _context = context;
    }

    // Updated SubordinateInfo class to include a nested "teams" property
    public class SubordinateInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        
        public string Level { get; set; }
        
        public string deptid { get; set; }
        public List<SubordinateInfo> Teams { get; set; } = new List<SubordinateInfo>();
    }

    // Recursive function to build the nested structure
    public async Task<SubordinateInfo> BuildHierarchyAsync(string supervisorId)
    {
        // Get the supervisor's information
        var supervisor = await _context.Employees
            .Where(e => e.empid == supervisorId)
            .Select(e => new SubordinateInfo
            {
                Id = int.Parse(e.empid),
                Name = e.name,
                Email = e.email,
                Level = e.level
            })
            .FirstOrDefaultAsync();

        if (supervisor == null)
        {
            return null;
        }

        // Get the supervisor's direct subordinates
        var subordinates = await _context.Employees
            .Where(e => e.supervisorid == supervisorId)
            .Select(e => new SubordinateInfo
            {
                Id = int.Parse(e.empid),
                Name = e.name,
                Email = e.email,
                Level=e.level,
                deptid=e.deptid
            })
            .ToListAsync();

        // Recursively build the team structure for each subordinate
        foreach (var subordinate in subordinates)
        {
            var subordinateHierarchy = await BuildHierarchyAsync(subordinate.Id.ToString());
            if (subordinateHierarchy != null)
            {
                subordinate.Teams = subordinateHierarchy.Teams;
            }
            supervisor.Teams.Add(subordinate);
        }

        return supervisor;
    }

    // Public function to fetch the hierarchy starting from an email
    public async Task<SubordinateInfo> FetchSubordinates(string email)
    {
        // Get the employee ID for the given email
        var supervisorId = await _context.Employees
            .Where(e => e.email == email)
            .Select(e => e.empid)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(supervisorId))
        {
            return null; // Supervisor not found
        }

        // Build and return the hierarchy
        var hierarchy = await BuildHierarchyAsync(supervisorId);
        return hierarchy;
    }
}
