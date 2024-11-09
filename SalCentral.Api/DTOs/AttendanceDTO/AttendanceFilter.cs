namespace SalCentral.Api.DTOs.AttendanceDTO
{
    public class AttendanceFilter
    {
        public string? SMEmployeeId { get; set; }
        public string? password { get; set; }
        public Guid? BranchId { get; set; }
        public bool? Today { get; set; }
        public Guid? UserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
