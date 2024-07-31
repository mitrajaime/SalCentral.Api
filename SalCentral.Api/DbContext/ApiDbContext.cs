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
        public virtual DbSet<BranchAssignment>? BranchAssignment { get; set; }
        public virtual DbSet<Branch>? Branch { get; set; }
        public virtual DbSet<Attendance>? Attendance { get; set; }
        
    }
}
