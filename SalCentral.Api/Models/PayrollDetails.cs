using System.ComponentModel.DataAnnotations;

namespace SalCentral.Api.Models
{
    public class PayrollDetails
    {
        [Key]
        public Guid PayrollDetailsId { get; set; }
        public Guid PayrollId { get; set; }
        public Guid UserId { get; set; }
        public double DeductedAmount { get; set; }
        public double NetPay { get; set; }
        public double GrossSalary { get; set; }
        public DateTime PayDate { get; set; }
    }
}
