using Microsoft.EntityFrameworkCore;
using SalCentral.Api.Models;
using System.Data;

namespace SalCentral.Api.DbContext
{
    public class ApiDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) { }

        public virtual DbSet<Role>? Role { get; set; }
        public virtual DbSet<User>? User { get; set; }
        public virtual DbSet<Branch>? Branch { get; set; }
        public virtual DbSet<Attendance>? Attendance { get; set; }
        public virtual DbSet<Deduction>? Deduction { get; set; }
        public virtual DbSet<DeductionAssignment>? DeductionAssignment { get; set; }
        public virtual DbSet<Schedule>? Schedule { get; set; }
        public virtual DbSet<Payroll>? Payroll { get; set; }
        public virtual DbSet<PayrollDetails>? PayrollDetails { get; set; }
    }
}
