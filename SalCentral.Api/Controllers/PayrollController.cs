using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalCentral.Api.DbContext;
using SalCentral.Api.DTOs.PayrollDTO;
using SalCentral.Api.DTOs.UserDTO;
using SalCentral.Api.Logics;
using SalCentral.Api.Models;
using static SalCentral.Api.Pagination.PaginationRequestQuery;

namespace SalCentral.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayrollController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly PayrollLogic _payrollLogic;

        public PayrollController(ApiDbContext context, PayrollLogic payrollLogic)
        {
            _context = context;
            _payrollLogic = payrollLogic;
        }

        [HttpGet]
        public async Task<IActionResult> GetPayroll([FromQuery] PaginationRequest paginationRequest, [FromQuery] PayrollFilter payrollFilter)
        {
            try
            {
                var result = await _payrollLogic.GetPayroll(paginationRequest, payrollFilter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostPayroll([FromBody] PayrollDTO payload)
        {
            try
            {
                var result = await _payrollLogic.CreatePayroll(payload);

                return Ok(result);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("Payslip")]
        public async Task<IActionResult> GetPayslip(Guid UserId, Guid PayrollId)
        {
            try
            {
                var result = await _payrollLogic.GeneratePayslip(UserId, PayrollId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
