﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalCentral.Api.DbContext;
using SalCentral.Api.DTOs;
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
        public async Task<IActionResult> GetPayroll([FromQuery] PaginationRequest paginationRequest, Guid BranchId, Guid UserId)
        {
            try
            {
                var result = await _payrollLogic.GetPayrollDetail(paginationRequest, BranchId, UserId);
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
    }
}