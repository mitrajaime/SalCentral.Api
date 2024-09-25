using SalCentral.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace SalCentral.Api.DTOs
{
    public class PayrollDTO
    {
        public Guid? PayrollId { get; set; }
        public decimal? TotalAmount { get; set; }
        public Guid? GeneratedBy { get; set; }
        public string? GeneratedByName { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsPaid { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? UserId { get; set; }

        public decimal? SSSContribution { get; set; }
        public decimal? PagIbigContribution { get; set; }
        public decimal? PhilHealthContribution { get; set; }

        public List<PayrollDetailsDTO>? PayrollDetailsList { get; set; }
    }
}
