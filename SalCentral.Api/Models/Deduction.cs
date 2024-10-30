using System.ComponentModel.DataAnnotations;

namespace SalCentral.Api.Models
{
    public class Deduction
    {
        [Key]
        public Guid DeductionId { get; set; }
        public string DeductionName { get; set; }
        public string DeductionDescription { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public bool? IsMandatory { get; set; }
    }
}
