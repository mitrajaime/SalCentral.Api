using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalCentral.Api.DbContext;
using SalCentral.Api.DTOs.ScheduleDTO;
using SalCentral.Api.DTOs.UserDTO;
using SalCentral.Api.Logics;
using SalCentral.Api.Models;
using static SalCentral.Api.Pagination.PaginationRequestQuery;

namespace SalCentral.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly UserLogic _userLogic;
        private readonly BranchLogic _branchLogic;
        private readonly ScheduleLogic _ScheduleLogic;

        public ScheduleController(ApiDbContext context, UserLogic userLogic, BranchLogic branchLogic, ScheduleLogic ScheduleLogic)
        {
            _userLogic = userLogic;
            _context = context;
            _branchLogic = branchLogic;
            _ScheduleLogic = ScheduleLogic;
        }

        [HttpGet]
        public async Task<IActionResult> GetSchedules([FromQuery] PaginationRequest paginationRequest, [FromQuery] ScheduleFilter scheduleFilter)
        {
            try
            {
                var result = await _ScheduleLogic.GetSchedule(paginationRequest, scheduleFilter);
                return Ok(result);

            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }
        [HttpGet("{UserId}")]

        public async Task<IActionResult> GetScheduleByUserId([FromQuery] PaginationRequest paginationRequest, [FromQuery] ScheduleFilter scheduleFilter, Guid UserId)
        {
            try
            {
                var result = await _ScheduleLogic.GetScheduleByUserId(paginationRequest, scheduleFilter, UserId);
                return Ok(result);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPost]
        public async Task<ActionResult> PostSchedule([FromBody] Schedule payload)
        {
            try
            {
                var result = await _ScheduleLogic.CreateSchedule(payload);
                if (result == null)
                {
                    return NotFound();
                }

                return Ok(result);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpDelete]
        public async Task<ActionResult> DeleteSchedule(Guid ScheduleId)
        {
            try
            {
                var Schedule = _context.Schedule.Where(u => u.ScheduleId == ScheduleId).FirstOrDefault();
                if(Schedule == null) { return NotFound(); }

                _context.Schedule.Remove(Schedule);
                _context.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> EditSchedule([FromBody] ScheduleDTO payload)
        {
            try
            {
                var result = await _ScheduleLogic.EditSchedule(payload);
                return Ok(result);

            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
