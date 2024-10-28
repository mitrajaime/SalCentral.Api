namespace SalCentral.Api.DTOs
{
    public class DeductionDTO
    {
        public Guid? DeductionId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? UserId { get; set; }

        public string? BranchName { get; set; }
        public string? DeductionName { get; set; }
        public string? DeductionDescription { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? Date { get; set; }

        public List<DeductionAssignmentDTO>? deductionList { get; set; }
        public List<DeductionAssignmentDTO>? userList { get; set;}
    }
}
