namespace SalCentral.Api.DTOs.PayrollDTO
{
    public class PayrollFields
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? UserId { get; set; }
        public decimal? SSSContribution { get; set; }
        public decimal? PagIbigContribution { get; set; }
        public decimal? PhilHealthContribution { get; set; }
        public decimal? SalaryRate { get; set; }
        public decimal? HolidayPay { get; set; }
        public decimal? OvertimePay { get; set; }
        public decimal? TotalDeductions { get; set; }
        public List<holidayList>? holidayList { get; set; }
    }
}
