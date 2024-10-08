using SalCentral.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace SalCentral.Api.DTOs.UserDTO
{
    public class UserDTO
    {
        public Guid? UserId { get; set; }
        public string? FullName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? ContactNo { get; set; }
        public string? SMEmployeeID { get; set; }
        public DateTime? HireDate { get; set; }
        public string? Password { get; set; }
        public Guid? RoleId { get; set; }
        public string? RoleName { get; set;}
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public Guid? AuthorizationKey { get; set; }
        public Schedule? Schedule { get; set; }
    }
}
