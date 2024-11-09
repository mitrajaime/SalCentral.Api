using System.ComponentModel.DataAnnotations;

namespace SalCentral.Api.Models
{
    public class Schedule
    {
        [Key]
        public Guid ScheduleId { get; set; }
        public Guid UserId { get; set; }
        public Guid BranchId { get; set; } 
        public bool Monday { get; set; } = false;
        public bool Tuesday { get; set; } = false;
        public bool Wednesday { get; set; } = false;
        public bool Thursday { get; set; } = false;
        public bool Friday { get; set; } = false;
        public bool? Saturday { get; set; }
        public bool? Sunday { get; set; }
        public TimeSpan ExpectedTimeIn { get; set; }
        public TimeSpan ExpectedTimeOut { get; set; }
    }
}
