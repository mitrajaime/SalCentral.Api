using Microsoft.AspNetCore.Mvc;
using SalCentral.Api.DbContext;
using SalCentral.Api.DTOs.UserDTO;
using SalCentral.Api.Logics;
using static SalCentral.Api.Pagination.PaginationRequestQuery;

namespace SalCentral.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BranchAssignmentController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly UserLogic _userLogic;
        private readonly BranchLogic _branchLogic;
        private readonly BranchAssignmentLogic _branchAssignmentLogic;

        public BranchAssignmentController(ApiDbContext context, UserLogic userLogic, BranchLogic branchLogic, BranchAssignmentLogic branchAssignmentLogic)
        {
            _userLogic = userLogic;
            _context = context;
            _branchLogic = branchLogic;
            _branchAssignmentLogic = branchAssignmentLogic;
        }
        [HttpGet]
        public async Task<IActionResult> GetBranchAssignment ([FromQuery] PaginationRequest paginationRequest, Guid UserId) 
        {
            try
            {
                var result = await _branchAssignmentLogic.GetBranchAssignment(paginationRequest, UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
