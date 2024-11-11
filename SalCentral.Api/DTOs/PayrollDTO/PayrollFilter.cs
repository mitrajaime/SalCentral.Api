namespace SalCentral.Api.DTOs.PayrollDTO
{
    public class PayrollFilter
    {
        public string? SearchKeyword { get; set; }
        public Guid BranchId { get; set; }
    }
}
