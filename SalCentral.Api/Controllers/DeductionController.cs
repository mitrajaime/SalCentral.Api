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
    public class DeductionController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly UserLogic _userLogic;
        private readonly BranchLogic _branchLogic;
        private readonly DeductionLogic _deductionLogic;

        public DeductionController(ApiDbContext context, UserLogic userLogic, BranchLogic branchLogic, DeductionLogic deductionLogic)
        {
            _userLogic = userLogic;
            _context = context;
            _branchLogic = branchLogic;
            _deductionLogic = deductionLogic;
        }
        [HttpGet]
        public async Task<IActionResult> GetDeductions([FromQuery] PaginationRequest paginationRequest, [FromQuery] DeductionDTO payload)
        {
            try
            {
                var result = await _deductionLogic.GetDeduction(paginationRequest, payload);
                return Ok(result);

            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }

        [HttpPost]
        public async Task<ActionResult> PostDeduction([FromBody] DeductionDTO payload)
        {
            try
            {
                var result = await _deductionLogic.CreateDeduction(payload);
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
        public async Task<ActionResult> DeleteDeduction(Guid DeductionId)
        {
            try
            {
                var result = await _deductionLogic.DeleteDeduction(DeductionId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> EditDeduction([FromBody] DeductionDTO payload, bool? IsMandatory)
        {
            try
            {
                if(IsMandatory == true)
                {
                    var mandatoryResult = await _deductionLogic.EditMandatoryDeduction(payload);
                    return Ok(mandatoryResult);
                }

                var result = await _deductionLogic.EditNonMandatoryDeduction(payload);
                return Ok(result);

            } 
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
