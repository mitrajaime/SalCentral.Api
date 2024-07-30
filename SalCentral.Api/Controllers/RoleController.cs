using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalCentral.Api.DbContext;
using SalCentral.Api.DTOs;
using SalCentral.Api.Models;

namespace SalCentral.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public RoleController(ApiDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult GetRole() 
        {
            try
            {
                var role = from r in _context.Role
                           select new RoleDTO
                           {
                               RoleId = r.RoleId,
                               RoleName = r.RoleName,
                           };

                return Ok(role);
            }
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public ActionResult PostRole([FromBody] RoleDTO payload)
        {
            try
            {
                var role = new Role()
                {
                    RoleId = new Guid(),
                    RoleName = payload.RoleName,
                };

                var exists = _context.Role.Where(r => r.RoleName == payload.RoleName).Any();
                if (exists) {
                    throw new Exception("Role already exists");
                }

                _context.Role.Add(role);
                _context.SaveChanges();
                return Ok(role);

            } catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }
    }
}
