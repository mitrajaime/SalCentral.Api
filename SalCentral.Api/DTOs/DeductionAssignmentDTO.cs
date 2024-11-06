namespace SalCentral.Api.DTOs
{
    public class DeductionAssignmentDTO
    {
        public Guid? DeductionAssignmentId { get; set; }
        public Guid? DeductionId { get; set; }
        public string? DeductionName { get; set; }
        public Guid? UserId { get; set; }
        public string? FullName { get; set; }
        public string? smEmployeeId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? Date { get; set; }
        public bool? IsMandatory { get; set; }
        public string? DeductionDescription { get; set; }
        public string? SSS { get; set; }
        public string? PagIbig { get; set; }
        public string? PhilHealth { get; set; }
        public string? Type { get; set; }
    }
    public class DeductionAssignmentUserList
    {
        public Guid? UserId { get; set; }
    }
}
