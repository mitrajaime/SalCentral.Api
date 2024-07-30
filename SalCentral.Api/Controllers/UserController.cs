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

        public UserController(ApiDbContext context, UserLogic userLogic)
        {
            _userLogic = userLogic;
            _context = context;
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
        [HttpPost]
        public async Task<ActionResult> PostUsers([FromBody] UserDTO payload)
        {
            try
            {
                var result = _userLogic.PostUsers(payload);

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
    }
}
