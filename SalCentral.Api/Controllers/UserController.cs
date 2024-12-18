﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly DeductionLogic _deductionLogic;

        public UserController(ApiDbContext context, UserLogic userLogic, BranchLogic branchLogic, ScheduleLogic scheduleLogic, DeductionLogic deductionLogic)
        {
            _userLogic = userLogic;
            _context = context;
            _branchLogic = branchLogic;
            _scheduleLogic = scheduleLogic;
            _deductionLogic = deductionLogic;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] PaginationRequest paginationRequest, [FromQuery] UserFilter userFilter)
        {
            try
            {
                if(userFilter.retriveAllUsers != null)
                {
                    var allresult = await _userLogic.GetAllUsers(paginationRequest, userFilter);
                    return Ok(allresult);
                }
                var result = await _userLogic.GetUsers(paginationRequest, userFilter);
                return Ok(result);

            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }

        [HttpGet("Schedule")]
        public async Task<IActionResult> GetUsersWithSchedule([FromQuery] PaginationRequest paginationRequest, [FromQuery] UserFilter userFilter)
        {
            try
            {
                var result = await _userLogic.GetUsersWithSchedule(paginationRequest, userFilter);
                return Ok(result);

            }
            catch (Exception ex)
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
        public async Task<ActionResult> PostUser([FromBody] UserDTO payload)
        {
            try
            {

                var result = await _userLogic.PostUser(payload);
                if (result == null)
                {
                    return NotFound();
                }
                await _context.SaveChangesAsync();
                var createSchedule = await _scheduleLogic.CreateSchedule(payload.Schedule, result.UserId);
                var addDeduction = await _deductionLogic.CreateDeduction(new DeductionDTO
                {
                    UserId = result.UserId,
                    deductionList = payload.deductionList,
                    SSS = payload.SSS,
                    PhilHealth = payload.PhilHealth,
                    PagIbig = payload.PagIbig,
                });

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
                var user = await _context.User.Where(u => u.UserId == UserId).FirstOrDefaultAsync();
                if(user == null) { return NotFound(); }

                user.IsDeleted = !user.IsDeleted;
                _context.User.Update(user);
                await _context.SaveChangesAsync();

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

        [HttpPut("GenerateAuthorizationKey")]
        public async Task<IActionResult> GenerateAuthorizationKey(Guid UserId)
        {
            try
            {
                var result = await _userLogic.GenerateAuthorizationKey(UserId);
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
                var user = await _context.User.Where(u => u.SMEmployeeID == payload.SMEmployeeID && u.IsDeleted == false).FirstOrDefaultAsync();
                if(user == null)
                {
                    throw new Exception("Login failed. Check your account details or contact the administrator for more information.");
                }

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