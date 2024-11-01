using Microsoft.AspNetCore.Mvc;
using SalCentral.Api.DbContext;
using SalCentral.Api.DTOs;
using SalCentral.Api.Models;
using static SalCentral.Api.Pagination.PaginationRequestQuery;

namespace SalCentral.Api.Logics
{
    public class DeductionAssignmentLogic
    {
        private readonly ApiDbContext _context;

        public DeductionAssignmentLogic(ApiDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetDeductionAssignment([FromQuery] PaginationRequest paginationRequest, Guid DeductionId)
        {
            try
            {
                IQueryable<DeductionAssignmentDTO> query = from da in _context.DeductionAssignment
                                                           join u in _context.User on da.UserId equals u.UserId
                                                           join b in _context.Branch on u.BranchId equals b.BranchId
                                                           join d in _context.Deduction on da.DeductionId equals d.DeductionId
                                                           where da.DeductionId == DeductionId
                                                           select new DeductionAssignmentDTO()
                                                           {
                                                               DeductionAssignmentId = da.DeductionAssignmentId,
                                                               DeductionName = d.DeductionName,
                                                               BranchId = b.BranchId,
                                                               BranchName = _context.Branch.Where(u => u.BranchId == b.BranchId).Select(b => b.BranchName).FirstOrDefault(),
                                                               Amount = d.Amount,
                                                               Date = d.Date,
                                                               UserId = u.UserId,
                                                               FullName = u.FirstName + " " + u.LastName,
                                                               smEmployeeId = u.SMEmployeeID,
                                                               DeductionId = da.DeductionId
                                                           };

                if (query == null) throw new Exception("No deductions found.");

                var responsewrapper = await PaginationLogic.PaginateData(query, paginationRequest);
                var deduction = responsewrapper.Results;

                if (deduction.Any())
                {
                    return responsewrapper;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<object> GetDeductionAssignmentOfUser([FromQuery] PaginationRequest paginationRequest, Guid UserId)
        {
            try
            {
                IQueryable<DeductionAssignmentDTO> query = from da in _context.DeductionAssignment
                                                           join u in _context.User on da.UserId equals u.UserId
                                                           join b in _context.Branch on u.BranchId equals b.BranchId
                                                           join d in _context.Deduction on da.DeductionId equals d.DeductionId
                                                           where u.UserId == UserId && d.IsMandatory == true
                                                           select new DeductionAssignmentDTO()
                                                           {
                                                               DeductionAssignmentId = da.DeductionAssignmentId,
                                                               DeductionName = d.DeductionName,
                                                               BranchId = b.BranchId,
                                                               BranchName = _context.Branch.Where(u => u.BranchId == b.BranchId).Select(b => b.BranchName).FirstOrDefault(),
                                                               Amount = d.Amount,
                                                               Date = d.Date,
                                                               UserId = u.UserId,
                                                               FullName = u.FirstName + " " + u.LastName,
                                                               smEmployeeId = u.SMEmployeeID,
                                                               DeductionId = da.DeductionId
                                                           };

                if (query == null) throw new Exception("No deductions found.");

                var responsewrapper = await PaginationLogic.PaginateData(query, paginationRequest);
                var deduction = responsewrapper.Results;

                if (deduction.Any())
                {
                    return responsewrapper;
                }

                return null;

            } catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
