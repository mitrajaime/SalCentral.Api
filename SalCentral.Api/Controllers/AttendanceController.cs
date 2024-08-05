using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalCentral.Api.DbContext;
using SalCentral.Api.Logics;

namespace SalCentral.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly UserLogic _userLogic;
        private readonly BranchLogic _branchLogic;
        private readonly BranchAssignmentLogic _branchAssignmentLogic;
        private readonly AttendanceLogic _attendanceLogic;

        public AttendanceController(ApiDbContext context,   UserLogic userLogic, BranchLogic branchLogic, BranchAssignmentLogic branchAssignmentLogic, AttendanceLogic attendanceLogic)
        {
            _userLogic = userLogic;
            _context = context;
            _branchLogic = branchLogic;
            _branchAssignmentLogic = branchAssignmentLogic;
            _attendanceLogic = attendanceLogic;
        }

        [HttpGet]
        public IActionResult GetAttendanceRecords() { }
    }
}
