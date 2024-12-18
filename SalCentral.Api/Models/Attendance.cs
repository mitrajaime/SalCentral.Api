﻿using System.ComponentModel.DataAnnotations;

namespace SalCentral.Api.Models
{
    public class Attendance
    {
        [Key]
        public Guid AttendanceId { get; set; }
        public DateTime Date { get; set; }
        public DateTime TimeIn { get; set; }
        public DateTime TimeOut { get; set; }   
        public Guid UserId { get; set; }
        public int HoursRendered { get; set; }
        public int OverTimeHours { get; set; } = 0;
        public int AllowedOvertimeHours { get; set; } = 0;
        public bool IsDayOff { get; set; }
    }
}
