using System.ComponentModel.DataAnnotations;

namespace SalCentral.Api.DTOs.ScheduleDTO
{
    public class ScheduleDTO
    {
        public Guid? ScheduleId { get; set; }
        public Guid? UserId { get; set; }
        public string? FullName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? SMEmployeeID { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public bool? Monday { get; set; }
        public bool? Tuesday { get; set; }
        public bool? Wednesday { get; set; }
        public bool? Thursday { get; set; }
        public bool? Friday { get; set; }
        public bool? Saturday { get; set; }
        public bool? Sunday { get; set; }
        public string? ExpectedTimeIn { get; set; }
        public string? ExpectedTimeOut { get; set; }
    }
}
