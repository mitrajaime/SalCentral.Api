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
                var query = from d in _context.Deduction
                            join da in _context.DeductionAssignment on d.DeductionId equals da.DeductionId
                            join u in _context.User on da.UserId equals u.UserId
                            where u.BranchId == payload.BranchId
                            group d by new
                            {
                                d.DeductionId,
                                d.DeductionName,
                                d.DeductionDescription,
                                d.IsMandatory,
                                d.Amount,
                                d.Date
                            } into g
                            select new DeductionDTO
                            {
                                DeductionId = g.Key.DeductionId,
                                DeductionName = g.Key.DeductionName,
                                DeductionDescription = g.Key.DeductionDescription,
                                IsMandatory = g.Key.IsMandatory,
                                Amount = g.Key.Amount,
                                Date = g.Key.Date
                            };

                if (!string.IsNullOrWhiteSpace(payload.DeductionName))
                {
                    query = query.Where(i => i.DeductionName.Contains(payload.DeductionName));
                }

                if (payload.IsMandatory.HasValue && payload.IsMandatory.Value == false)
                {
                    query = query.Where(i => i.IsMandatory == false);
                }

                if (!await query.AnyAsync())
                    throw new Exception("No deductions found.");

                var responseWrapper = await PaginationLogic.PaginateData(query, paginationRequest);

                return responseWrapper.Results.Any() ? responseWrapper : null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving deductions: {ex.Message}");
            }
        }

        public async Task<object> CreateDeduction([FromBody] DeductionDTO payload)
        {
            try
            {
                var deductionAssignments = new List<DeductionAssignment>();

                foreach (var d in payload.deductionList)
                {
                    var deduction = new Deduction()
                    {
                        DeductionName = d.DeductionName,
                        Amount = (decimal)d.Amount,
                        IsMandatory = d.IsMandatory,
                        DeductionDescription = d.DeductionDescription,
                        Date = DateTime.Now,
                    };

                    await _context.Deduction.AddAsync(deduction);
                    await _context.SaveChangesAsync();

                    var deductionAssignment = new DeductionAssignment()
                    {
                        DeductionId = deduction.DeductionId,
                        UserId = (Guid)payload.UserId,
                    };

                    deductionAssignments.Add(deductionAssignment);
                };

                //var exists = _context.Deduction.Where(b => b.DeductionName == payload.DeductionName && b.IsMandatory = true).Any();
                //if (exists)
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

                var deductionAssignment = await _context.DeductionAssignment.Where(u => u.DeductionId == payload.DeductionId).ToListAsync();

                _context.DeductionAssignment.RemoveRange(deductionAssignment);

                var newDeductionAssignmentList = new List<DeductionAssignment>();

                foreach (var a in payload.userList)
                {
                    var newDeductionAssignment = new DeductionAssignment()
                    {
                        DeductionId = (Guid)payload.DeductionId,
                        UserId = (Guid)a.UserId,
                    };

                    newDeductionAssignmentList.Add(newDeductionAssignment);
                }

                await _context.DeductionAssignment.AddRangeAsync(newDeductionAssignmentList);
                _context.Deduction.Update(deduction);
                await _context.SaveChangesAsync();

                return deduction;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> DeleteDeduction(Guid DeductionId)
        {
            try
            {
                var deduction = _context.Deduction.Where(u => u.DeductionId == DeductionId).FirstOrDefault();
                if (deduction == null) { return "Deduction does not exist"; }

                _context.Deduction.Remove(deduction);
                _context.SaveChanges();

                return "Deduction deleted successfully";
            } catch (Exception ex) 
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
