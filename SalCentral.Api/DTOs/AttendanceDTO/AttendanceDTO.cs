namespace SalCentral.Api.DTOs.AttendanceDTO
{
    public class AttendanceDTO
    {
        public Guid? AttendanceId { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? UserId { get; set; }
        public string? SMEmployeeId { get; set; }
        public string? User {  get; set; }
        public int? HoursRendered { get; set; }
        public int? OverTimeHours { get; set; }
        public int? AllowedOvertimeHours { get; set; }

        public List<userList>? userList { get; set; }
    }
    public class userList
    {
        public Guid? UserId { get; set; }
        public int? allowedOvertimeHours { get; set; }
    }
}
