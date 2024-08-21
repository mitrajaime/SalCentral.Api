using System.ComponentModel.DataAnnotations;

namespace SalCentral.Api.DTOs
{
    public class DeductionAssignment
    {
        [Key]
        public Guid DeductionAssignmentId { get; set; }
        public Guid DeductionId { get; set; }
        public Guid UserId { get; set; }
    }
}
