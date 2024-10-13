using System.ComponentModel.DataAnnotations;

namespace SalCentral.Api.DTOs
{
    public class PayrollDetailsDTO
    {
        public Guid? PayrollDetailsId { get; set; }
        public Guid? PayrollId { get; set; }
        public string? FullName { get; set; }

        public Guid? UserId { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public decimal? DeductedAmount { get; set; }
        public decimal? NetPay { get; set; }
        public decimal? Salary { get; set; }

        public decimal? GrossSalary { get; set; }
        public DateTime? PayDate { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsPaid { get; set; }
        public decimal? SSSContribution { get; set; }
        public decimal? PagIbigContribution { get; set; }
        public decimal? PhilHealthContribution { get; set; }

        public double? TotalHoursRendered { get; set; }

    }
}
