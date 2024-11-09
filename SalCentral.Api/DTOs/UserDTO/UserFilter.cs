namespace SalCentral.Api.DTOs.UserDTO
{
    public class UserFilter
    {
        public string? SearchKeyword { get; set; }
        public Guid? BranchId { get; set; }
    }
}
