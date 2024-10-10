namespace SalCentral.Api.DTOs.PayrollDTO
{
    public class PayrollFilter
    {
        public string? PayrollName { get; set; }    
        public string? GeneratedByName { get; set; }
        public Guid BranchId { get; set; }
    }
}
