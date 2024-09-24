namespace SalCentral.Api.DTOs
{
    public class PayrollFields
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set;}
        public Guid? UserId { get; set; }
        public decimal? SSSContribution { get; set; }
        public decimal? PagIbigContribution { get; set; }
        public decimal? PhilHealthContribution { get; set; }
    }
}
