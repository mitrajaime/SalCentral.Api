using System.ComponentModel.DataAnnotations;

namespace SalCentral.Api.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }
}
