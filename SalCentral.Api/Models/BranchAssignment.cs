using System.ComponentModel.DataAnnotations;

namespace SalCentral.Api.Models
{
    public class BranchAssignment
    {
        [Key]
        public Guid BranchAssignmentId { get; set; }
        [Required]
        public Guid BranchId { get; set; }
        [Required]
        public Guid UserId { get; set; }
    }
}
