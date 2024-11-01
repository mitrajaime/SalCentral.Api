using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalCentral.Api.DbContext;
using SalCentral.Api.DTOs.PayrollDTO;
using SalCentral.Api.Logics;
using SalCentral.Api.Migrations;
using SalCentral.Api.Models;
using static SalCentral.Api.Pagination.PaginationRequestQuery;

namespace SalCentral.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeductionAssignmentController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly DeductionAssignmentLogic _deductionAssignmentLogic;

        public DeductionAssignmentController(ApiDbContext context, DeductionAssignmentLogic deductionAssignmentLogic)
        {
            _context = context;
            _deductionAssignmentLogic = deductionAssignmentLogic;
        }

        [HttpGet]
        public async Task<IActionResult> GetDeductionAssignment([FromQuery] PaginationRequest paginationRequest, Guid DeductionId, Guid? UserId )
        {
            try
            {
                if(UserId != null) {
                    var userResult = await _deductionAssignmentLogic.GetDeductionAssignmentOfUser(paginationRequest, (Guid)UserId);
                    return Ok(userResult);
                }

                var result = await _deductionAssignmentLogic.GetDeductionAssignment(paginationRequest, DeductionId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
