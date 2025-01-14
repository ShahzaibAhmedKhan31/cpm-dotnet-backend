using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
namespace WebApplication1.dbdata{ 

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Dept> Departments { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<Datasource> Datasources { get; set; }
    public DbSet<Kra> Kras { get; set; }
    public DbSet<MonthlyTask> MonthlyTasks { get; set; }
    public DbSet<TaskScore> TaskScores { get; set; }
    public DbSet<KpiDetail> KpiDetails { get; set; }
    public DbSet<KpiCriteria> KpiCriteria { get; set; }
    public DbSet<Competency> Competencies { get; set; }
    public DbSet<CompetencyScore> CompetencyScores { get; set; }

   protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    modelBuilder.Entity<TaskScore>().ToTable("taskscores");
    modelBuilder.Entity<Employee>().ToTable("employee");
    modelBuilder.Entity<Dept>().ToTable("departments");

    // Set ScoreId as Primary Key
    modelBuilder.Entity<TaskScore>().HasKey(ts => ts.scoreid);
    modelBuilder.Entity<Dept>().HasKey(d => d.dept_id); // Set DeptId as Primary Key
    modelBuilder.Entity<Employee>().HasKey(e => e.empid); // Set EmpId as Primary Key
    modelBuilder.Entity<Role>().HasKey(r => r.RoleId); // Set RoleId as Primary Key
    modelBuilder.Entity<Kra>().HasKey(k => k.KraId); // Set KraId as Primary Key
    modelBuilder.Entity<MonthlyTask>().HasKey(mt => mt.TaskId); // Set TaskId as Primary Key
    modelBuilder.Entity<CompetencyScore>().HasKey(cs => cs.CompScoreId); // Set CompScoreId as Primary Key

        // Additional relationship mappings can be added here if needed
        base.OnModelCreating(modelBuilder);
    }
}
}