using Microsoft.AspNetCore.Mvc;
using SalCentral.Api.DbContext;
using static SalCentral.Api.Pagination.PaginationRequestQuery;

namespace SalCentral.Api.Logics
{
    public class AttendanceLogic
    {
        private readonly ApiDbContext _context;
        private readonly UserLogic _userLogic;
        private readonly BranchLogic _branchLogic;
        private readonly BranchAssignmentLogic _branchAssignmentLogic;

        public AttendanceLogic(ApiDbContext context, UserLogic userLogic, BranchLogic branchLogic, BranchAssignmentLogic branchAssignmentLogic)
        {
            _userLogic = userLogic;
            _context = context;
            _branchLogic = branchLogic;
            _branchAssignmentLogic = branchAssignmentLogic;
        }

        public async Task<object> GetAttendance([FromQuery] PaginationRequest paginationRequest                )
        {

        }
    }
}
