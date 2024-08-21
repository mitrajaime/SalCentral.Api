using Microsoft.AspNetCore.Mvc;
using SalCentral.Api.DbContext;
using SalCentral.Api.DTOs;
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

        //public async Task<object> GetDeductionAssignment([FromQuery] PaginationRequest paginationRequest, [FromBody] DeductionAssignmentDTO payload)
        //{
        //    try
        //    {
        //        IQueryable<DeductionAssignmentDTO> query = from q in _context.Deduction
        //                                         select new DeductionAssignmentDTO()
        //                                         {
        //                                             DeductionAssignmentId = q.DeductionAssignmentId,
        //                                             DeductionName = q.DeductionName,
        //                                             BranchId = q.BranchId,
        //                                             BranchName = _context.Branch.Where(u => u.BranchId == q.BranchId).Select(b => b.BranchName).FirstOrDefault(),
        //                                             Amount = q.Amount,
        //                                             Date = q.Date,
        //                                         };

        //        if (query == null) throw new Exception("No deductions found.");

        //        var responsewrapper = await PaginationLogic.PaginateData(query, paginationRequest);
        //        var attendance = responsewrapper.Results;

        //        if (attendance.Any())
        //        {
        //            return responsewrapper;
        //        }

        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}
    }
}
