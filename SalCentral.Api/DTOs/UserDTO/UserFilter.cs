namespace SalCentral.Api.DTOs.UserDTO
{
    public class UserFilter
    {
        public string? FullName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? SMEmployeeId {  get; set; }
        public Guid? BranchId { get; set; }
    }
}
