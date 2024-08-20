using System.ComponentModel.DataAnnotations;

namespace SalCentral.Api.Models
{
    public class Deduction
    {
        [Key]
        public Guid DeductionId { get; set; }
        public Guid BranchId { get; set; }
        public string DeductionName { get; set; }
        public double Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
