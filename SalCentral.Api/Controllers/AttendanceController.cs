using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalCentral.Api.DbContext;
using SalCentral.Api.DTOs.AttendanceDTO;
using SalCentral.Api.Logics;
using static SalCentral.Api.Pagination.PaginationRequestQuery;

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
        public async Task<IActionResult> GetAttendanceRecords([FromQuery] PaginationRequest paginationRequest, [FromQuery] AttendanceFilter attendanceFilter) 
        { 
            try
            {
                if (attendanceFilter.SMEmployeeId != null || attendanceFilter.password != null || attendanceFilter.BranchId != null)
                {
                    if (attendanceFilter.Today == true)
                    {
                        var attendanceToday = await _attendanceLogic.GetEmployeeAttendanceToday(paginationRequest, attendanceFilter);
                        return Ok(attendanceToday);
                    }

                    var result = await _attendanceLogic.GetAttendanceOfEmployee(paginationRequest, attendanceFilter);
                    return Ok(result);
                }

                var results = await _attendanceLogic.GetAttendance(paginationRequest);

                return Ok(results);
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("TimeIn")]
        public async Task<IActionResult> TimeIn([FromBody] AttendanceDTO payload)
        {
            try
            {
                var results = await _attendanceLogic.TimeIn(payload);

                return Ok(results);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }  

        [HttpPut("TimeOut")]
        public async Task<IActionResult> TimeOut([FromBody] AttendanceDTO payload, bool? undoTimeOut)
        {
            try
            {
                if(undoTimeOut == true)
                {
                    var timeOut = await _attendanceLogic.UndoTimeOut(payload);
                    return Ok(timeOut);
                }

                var results = await _attendanceLogic.TimeOut(payload);

                return Ok(results);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
