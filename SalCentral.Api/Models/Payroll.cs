using System.ComponentModel.DataAnnotations;

namespace SalCentral.Api.Models
{
    public class Payroll
    {
        [Key]
        public Guid PayrollId { get; set; }
        public string? PayrollName { get; set; }
        public decimal TotalAmount { get; set; }
        public Guid GeneratedBy { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsPaid { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
