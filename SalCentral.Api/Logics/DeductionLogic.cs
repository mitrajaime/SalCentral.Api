using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalCentral.Api.DbContext;
using SalCentral.Api.DTOs;
using SalCentral.Api.DTOs.AttendanceDTO;
using SalCentral.Api.DTOs.UserDTO;
using SalCentral.Api.Models;
using System.Linq.Expressions;
using static SalCentral.Api.Pagination.PaginationRequestQuery;

namespace SalCentral.Api.Logics
{
    public class DeductionLogic
    {
        private readonly ApiDbContext _context;

        public DeductionLogic(ApiDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetDeduction([FromQuery] PaginationRequest paginationRequest, [FromBody] DeductionDTO payload)
        {
            try
            { 
                IQueryable<DeductionDTO> query = from q in _context.Deduction
                                                  select new DeductionDTO()
                                                  {
                                                      DeductionId = q.DeductionId,
                                                      DeductionName = q.DeductionName,
                                                      DeductionDescription = q.DeductionDescription,
                                                      BranchId = q.BranchId,
                                                      BranchName = _context.Branch.Where(u => u.BranchId == q.BranchId).Select(b => b.BranchName).FirstOrDefault(),
                                                      Amount = q.Amount,
                                                      Date = q.Date,
                                                  };

                if (payload.BranchId.HasValue)
                {
                    query = query.Where(i => i.BranchId.ToString().Contains(payload.BranchId.ToString()));
                }

                if (query == null) throw new Exception("No deductions found.");

                var responsewrapper = await PaginationLogic.PaginateData(query, paginationRequest);
                var attendance = responsewrapper.Results;

                if (attendance.Any())
                {
                    return responsewrapper;
                }

                return null;
            } catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<object> CreateDeduction([FromBody] DeductionDTO payload)
        {
            try
            {
                var deduction = new Deduction()
                {
                    DeductionName = payload.DeductionName,
                    BranchId = (Guid)payload.BranchId,
                    Amount = (decimal)payload.Amount,
                    DeductionDescription = payload.DeductionDescription,
                    Date = DateTime.Now,
                };

                //var exists = _context.Deduction.Where(b => b.DeductionName == payload.DeductionName).Any();
                //if(exists)
                //{
                //    throw new Exception("This deduction already exists.");
                //}

                await _context.Deduction.AddAsync(deduction);
                await _context.SaveChangesAsync();
                return deduction;

            } catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Deduction> EditDeduction([FromBody] DeductionDTO payload)
        {
            try
            {
                var deduction = await _context.Deduction.Where(u => u.DeductionId == payload.DeductionId).FirstOrDefaultAsync();

                deduction.DeductionName = payload.DeductionName;
                deduction.DeductionDescription = payload.DeductionDescription;
                deduction.Amount = (decimal)payload.Amount;

                _context.Deduction.Update(deduction);
                await _context.SaveChangesAsync();

                return deduction;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
