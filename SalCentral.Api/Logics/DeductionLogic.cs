using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalCentral.Api.DbContext;
using SalCentral.Api.DTOs;
using SalCentral.Api.DTOs.AttendanceDTO;
using SalCentral.Api.DTOs.UserDTO;
using SalCentral.Api.Migrations;
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

        public async Task<object> GetDeduction([FromQuery] PaginationRequest paginationRequest, [FromQuery] DeductionDTO payload)
        {
            try
            {
                IQueryable<DeductionDTO> query = from q in _context.Deduction
                                                 where q.BranchId == payload.BranchId
                                                 select new DeductionDTO()
                                                 {
                                                     DeductionId = q.DeductionId,
                                                     DeductionName = q.DeductionName,
                                                     DeductionDescription = q.DeductionDescription,
                                                     BranchId = q.BranchId,
                                                     BranchName = _context.Branch.Where(u => u.BranchId == q.BranchId).Select(b => b.BranchName).FirstOrDefault(),
                                                     Amount = q.Amount,
                                                     Date = q.Date,
                                                     userList = _context.DeductionAssignment.Where(d => d.DeductionId == q.DeductionId).Select(u => new DeductionAssignmentDTO
                                                     { 
                                                         UserId = u.UserId, 
                                                     }).ToList()
                                                 };

                // Filter by DeductionName if provided
                if (!string.IsNullOrWhiteSpace(payload.DeductionName))
                {
                    query = query.Where(i => i.DeductionName.Contains(payload.DeductionName));
                }

                if (!query.Any()) throw new Exception("No deductions found.");

                var responseWrapper = await PaginationLogic.PaginateData(query, paginationRequest);
                return responseWrapper.Results.Any() ? responseWrapper : null;
            }
            catch (Exception ex)
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

                await _context.Deduction.AddAsync(deduction);
                await _context.SaveChangesAsync();

                var deductionAssignments = new List<DeductionAssignment>();

                foreach (var d in payload.userList)
                {
                    var deductionAssignment = new DeductionAssignment()
                    {
                        DeductionId = deduction.DeductionId,
                        UserId = (Guid)d.UserId,
                    };

                    deductionAssignments.Add(deductionAssignment);
                };

                //var exists = _context.Deduction.Where(b => b.DeductionName == payload.DeductionName).Any();
                //if(exists)
                //{
                //    throw new Exception("This deduction already exists.");
                //}

                await _context.DeductionAssignment.AddRangeAsync(deductionAssignments);
                await _context.SaveChangesAsync();
                
                return payload;

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
