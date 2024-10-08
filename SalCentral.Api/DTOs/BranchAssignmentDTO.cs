﻿namespace SalCentral.Api.DTOs
{
    public class BranchAssignmentDTO
    {
        public Guid? BranchAssignmentId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? BranchName { get; set; }
    }
}
