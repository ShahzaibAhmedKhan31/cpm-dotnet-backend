using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Tenant
    {
        [Key]
        public string TenantId { get; set; }
        public string AdminId { get; set; }
        public string AdminEmail { get; set; }
        public string AdUrl { get; set; }

        // public ICollection<Dept> Departments { get; set; }
    }

    public class Dept
    {
        [Key]
        public string DeptId { get; set; }
        public string HodId { get; set; }
        public string TenantId { get; set; }

        // public Tenant Tenant { get; set; }
        // public ICollection<Employee> Employees { get; set; }
        // public ICollection<Datasource> Datasources { get; set; }
        // public ICollection<Role> Roles { get; set; }
    }

    public class Employee
    {
        [Key]
        public string EmpId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Level { get; set; }
        public string RoleId { get; set; }
        public string SupervisorId { get; set; }
        public string TenantId { get; set; }
        public string DeptId { get; set; }
        public string Active { get; set; }

        // public Dept Department { get; set; }
        // public Role Role { get; set; }
        // public Tenant Tenant { get; set; }
        // public ICollection<Kra> Kras { get; set; }
    }

    public class Role
    {
        [Key]
        public string RoleId { get; set; }
        public string TenantId { get; set; }
        public string DeptId { get; set; }

        // public Dept Department { get; set; }
        // public Tenant Tenant { get; set; }
        // public ICollection<Permission> Permissions { get; set; }
        // public ICollection<Employee> Employees { get; set; }
    }

    public class Permission
    {
        [Key]
        public string PermId { get; set; }
        public string RoleId { get; set; }
        public string PermissionName { get; set; }

        // public Role Role { get; set; }
    }

    public class Datasource
    {
        [Key]
        public string SourceId { get; set; }
        public string DeptId { get; set; }
        public string SourceLink { get; set; }
        public string Comments { get; set; }

        // public Dept Department { get; set; }
    }

    public class Kra
    {
        [Key]
        public string KraId { get; set; }
        public string EmpId { get; set; }
        public DateTime Date { get; set; }
        public string KraType { get; set; }
        public string ReportedTo { get; set; }

        // public Employee Employee { get; set; }
        // public ICollection<MonthlyTask> MonthlyTasks { get; set; }
    }

    public class MonthlyTask
    {
        [Key]
        public string TaskId { get; set; }
        public string TaskType { get; set; }
        public string KraId { get; set; }
        public DateTime Date { get; set; }

        // public Kra Kra { get; set; }
        // public ICollection<TaskScore> TaskScores { get; set; }
        // public ICollection<CompetencyScore> CompetencyScores { get; set; }
    }

    public class TaskScore
    {
        [Key]
        public int? scoreid { get; set; } // Nullable ScoreId since it is auto-incremented by the DB
        public string? taskid { get; set; } // TaskId can be nullable if you allow empty values
        public float? superrating { get; set; } // Nullable SuperRating
        public float? selfrating { get; set; } // Nullable SuperRating
        public float? automatedrating { get; set; } // Nullable AutomatedRating
        public float? comments { get; set; } // Nullable Comments
    }

    public class KpiDetail
    {
        [Key]
        public string KpiId { get; set; }
        public string KpiName { get; set; }
        public float KpiScore { get; set; }
    }

    public class KpiCriteria
    {
        [Key]
        public int CriteriaId { get; set; }
        public string Level { get; set; }
        public string TaskType { get; set; }
        public int Count { get; set; }
        public int NoOfComments { get; set; }
        public string KpiId { get; set; }

        // public KpiDetail KpiDetail { get; set; }
    }

    public class Competency
    {
        [Key]
        public string CompId { get; set; }
        public string Level { get; set; }
        public string Department { get; set; }
        public string Tenant { get; set; }
    }

    public class CompetencyScore
    {
        [Key]
        public string CompScoreId { get; set; }
        public string CompId { get; set; }
        public string TaskId { get; set; }

        // public Competency Competency { get; set; }
        // public MonthlyTask MonthlyTasks { get; set; }
    }
}
