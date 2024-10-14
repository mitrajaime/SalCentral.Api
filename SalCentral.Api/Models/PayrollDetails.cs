using System.ComponentModel.DataAnnotations;

namespace SalCentral.Api.Models
{
    public class PayrollDetails
    {
        [Key]
        public Guid PayrollDetailsId { get; set; }
        public Guid PayrollId { get; set; }
        public Guid UserId { get; set; }
        public decimal CurrentSalaryRate { get; set; }
        public decimal DeductedAmount { get; set; }
        public decimal NetPay { get; set; }
        public decimal GrossSalary { get; set; }
        public bool IsPaid { get; set; }
        public DateTime PayDate { get; set; }
        public decimal SSSContribution { get; set; }
        public decimal PagIbigContribution { get; set; }
        public decimal PhilHealthContribution { get; set; }
    }
}
