using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalCentral.Api.DbContext;
using SalCentral.Api.DTOs;
using SalCentral.Api.DTOs.UserDTO;
using SalCentral.Api.Logics;
using static SalCentral.Api.Pagination.PaginationRequestQuery;

namespace SalCentral.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly UserLogic _userLogic;
        private readonly BranchLogic _branchLogic;
        private readonly ScheduleLogic _scheduleLogic;

        public UserController(ApiDbContext context, UserLogic userLogic, BranchLogic branchLogic, ScheduleLogic scheduleLogic)
        {
            _userLogic = userLogic;
            _context = context;
            _branchLogic = branchLogic;
            _scheduleLogic = scheduleLogic;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] PaginationRequest paginationRequest, [FromQuery] UserFilter userFilter)
        {
            try
            {
                var result = await _userLogic.GetUsers(paginationRequest, userFilter);
                return Ok(result);

            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }

        [HttpGet("{UserId}")]
        public async Task<IActionResult> GetUserById([FromQuery] PaginationRequest paginationRequest, Guid UserId)
        {
            try
            {
                var result = await _userLogic.GetUserById(paginationRequest, UserId);
                return Ok(result);
            } catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> PostUsers([FromBody] UserDTO payload)
        {
            try
            {
                

                var result = await _userLogic.PostUser(payload);
                if (result == null)
                {
                    return NotFound();
                }

                var branchResult = await _branchLogic.PostUsersBranch(payload,result.UserId);
                if (branchResult == null)
                {
                    return NotFound();
                }
                
                foreach (var schedule in payload.scheduleList)
                {
                    try
                    {
                        schedule.UserId = result.UserId;
                        var createSchedule = await _scheduleLogic.CreateSchedule(schedule);

                    } catch (Exception ex)
                    {
                        return BadRequest(ex.Message);
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(result);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpDelete]
        public async Task<ActionResult> DeleteUser(Guid UserId)
        {
            try
            {
                var user = _context.User.Where(u => u.UserId == UserId).FirstOrDefault();
                if(user == null) { return NotFound(); }

                _context.User.Remove(user);
                _context.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> EditUser([FromBody] UserDTO payload)
        {
            try
            {
                var result = await _userLogic.EditUser(payload);
                return Ok(result);

            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Login")]
        public async Task<IActionResult> AuthenticateUser([FromQuery] UserDTO payload, bool? adminLogin)
        {
            try
            {
                if(adminLogin == true)
                {
                    var adminResult = await _userLogic.AuthenticateAdmin(payload);
                    return Ok(adminResult);
                }

                var result = await _userLogic.AuthenticateUser(payload);
                return Ok(result);

            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
