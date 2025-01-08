using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.dbdata;

public class EmployeesService
{
    private readonly HttpClient _httpClient;
    private readonly string _url;
    private readonly ApplicationDbContext _context;

    // Inject HttpClient via constructor
    public EmployeesService(HttpClient httpClient, ApplicationDbContext context)
    {
        _httpClient = httpClient;
        _context = context;
        
    }

    public class SubordinateInfo
    {
        public string EmpId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public async Task<Dictionary<string, List<SubordinateInfo>>> FetchSubordinatesHierarchyAsync(string supervisorId)
    {
        var result = new Dictionary<string, List<SubordinateInfo>>();
        var employees = await _context.Employees
                                    .Where(e => e.supervisorid == supervisorId)
                                    .ToListAsync();
        if (!employees.Any())
        {
            return result;
        }

        List<SubordinateInfo> subordinateInfos = new List<SubordinateInfo>();
        foreach (var employee in employees)
        {
            subordinateInfos.Add(new SubordinateInfo
            {
                EmpId = employee.empid.ToString(),
                Name = employee.name,
                Email = employee.email
            });

            var subordinates = await FetchSubordinatesHierarchyAsync(employee.empid.ToString());
            
            foreach (var kvp in subordinates)
            {
                if (!result.ContainsKey(kvp.Key))
                {
                    result[kvp.Key] = new List<SubordinateInfo>();
                }
                result[kvp.Key].AddRange(kvp.Value);
            }
        }

        result[supervisorId] = subordinateInfos;
        return result;

    }


    public async Task<Dictionary<string, List<SubordinateInfo>>> fetchSubordinates(string email){
        
        // Fetch the TaskScore for the given TaskId
        Console.WriteLine(email);

        var employeeId = await _context.Employees
                                    .Where(e => e.email == email)
                                    .Select(e => e.empid)
                                    .FirstOrDefaultAsync();

        Dictionary<string, List<SubordinateInfo>> subordinatesHierarchy = await FetchSubordinatesHierarchyAsync(employeeId);
        
        
        return subordinatesHierarchy;

    }

}

