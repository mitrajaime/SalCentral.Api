using Microsoft.EntityFrameworkCore;
using SalCentral.Api.Models;
using System.Data;

namespace SalCentral.Api.DbContext
{
    public class ApiDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) { }

        public virtual DbSet<Role>? Role { get; set; }
        
    }
}
