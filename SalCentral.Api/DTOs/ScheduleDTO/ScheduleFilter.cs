namespace SalCentral.Api.DTOs.ScheduleDTO
{
    public class ScheduleFilter
    {
        public Guid? UserId { get; set; }
        public Guid? BranchId { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? SMEmployeeID { get; set; }
    }
}
