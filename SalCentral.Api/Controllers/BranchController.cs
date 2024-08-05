using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalCentral.Api.DbContext;
using SalCentral.Api.DTOs;
using SalCentral.Api.Logics;
using SalCentral.Api.Models;

namespace SalCentral.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BranchController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly BranchLogic _branchLogic;

        public BranchController(ApiDbContext context, BranchLogic branchLogic)
        {
            _context = context;
            _branchLogic = branchLogic;
        }

        [HttpGet]
        public ActionResult GetBranch()
        {
            try
            {
                var branch = from b in _context.Branch
                             select new Branch()
                             {
                                 BranchId = b.BranchId,
                                 BranchName = b.BranchName,
                             };

                return Ok(branch);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostBranch([FromBody] BranchDTO payload)
        {
            try
            {
                var result = await _branchLogic.CreateBranch(payload);

                return Ok(result);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
