namespace SalCentral.Api.DTOs
{
    public class DeductionAssignmentDTO
    {
        public Guid? DeductionAssignmentId { get; set; }
        public Guid? DeductionId { get; set; }
        public Guid? UserId { get; set; }
        public Guid? DeductionName { get; set; }
        public string? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
    }
}
