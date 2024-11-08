using SalCentral.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace SalCentral.Api.DTOs.PayrollDTO
{
    public class PayrollDTO
    {
        public Guid? PayrollId { get; set; }
        public string? PayrollName { get; set; }

        public decimal? TotalAmount { get; set; }
        public Guid? GeneratedBy { get; set; }
        public string? GeneratedByName { get; set; }

        public DateTime? DateCreated { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? UserId { get; set; }

        public List<PayrollDetailsDTO>? PayrollDetailsList { get; set; }
        public List<holidayList>? holidayList { get; set; }
    }
    public class holidayList
    {
        public DateTime Date { get; set; }
    }
}
