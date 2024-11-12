using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalCentral.Api.DbContext;
using SalCentral.Api.DTOs;
using SalCentral.Api.DTOs.PayrollDTO;
using SalCentral.Api.Logics;
using SalCentral.Api.Models;
using static SalCentral.Api.Pagination.PaginationRequestQuery;

namespace SalCentral.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayrollDetailsController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly PayrollLogic _payrollLogic;

        public PayrollDetailsController(ApiDbContext context, PayrollLogic payrollLogic)
        {
            _context = context;
            _payrollLogic = payrollLogic;
        }

        [HttpGet]
        public async Task<IActionResult> GetPayrollDetails([FromQuery] PaginationRequest paginationRequest, Guid PayrollId)
        {
            try
            {
                var result = await _payrollLogic.GetPayrollDetail(paginationRequest, PayrollId);
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

        [HttpPut("{PayrollId}")]
        public async Task<IActionResult> EditPayrollDetail(Guid PayrollDetailsId)
        {
            try
            {
                var result = await _payrollLogic.EditPayrollDetail(PayrollDetailsId);


                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
