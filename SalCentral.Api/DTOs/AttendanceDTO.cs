namespace SalCentral.Api.DTOs
{
    public class AttendanceDTO
    {
        public Guid? AttendanceId { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? UserId { get; set; }
    }
}
