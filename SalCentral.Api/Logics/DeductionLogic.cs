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
                                d.Date,
                                d.Type
                            } into g
                            select new DeductionDTO
                            {
                                DeductionId = g.Key.DeductionId,
                                DeductionName = g.Key.DeductionName,
                                DeductionDescription = g.Key.DeductionDescription,
                                IsMandatory = g.Key.IsMandatory,
                                Amount = g.Key.Amount,
                                Date = g.Key.Date,
                                Type = g.Key.Type
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
                    switch (d.DeductionName)
                    {
                        case "SSS":
                            if (string.IsNullOrEmpty(payload.SSS)) { throw new Exception("Please fill up the SSS ID field before adding a new SSS deduction."); };
                            break;
                        case "Pagibig":
                            if (string.IsNullOrEmpty(payload.PagIbig)) { throw new Exception("Please fill up the Pag-Ibig ID field before adding a new Pag-Ibig deduction."); };
                            break;
                        case "PhilHealth":
                            if (string.IsNullOrEmpty(payload.PhilHealth)) { throw new Exception("Please fill up the PhilHealth ID field before adding a new PhilHealth deduction."); };
                            break;
                    }
                }

                foreach (var d in payload.deductionList)
                {
                    var deduction = new Deduction()
                    {
                        DeductionName = d.DeductionName,
                        Amount = (decimal)d.Amount,
                        IsMandatory = d.IsMandatory,
                        DeductionDescription = d.DeductionDescription,
                        Type = d.Type,
                        Date = DateTime.Now,
                    };

                    await _context.Deduction.AddAsync(deduction);
                    await _context.SaveChangesAsync();

                    var deductionAssignment = new DeductionAssignment()
                    {
                        DeductionId = deduction.DeductionId,
                        UserId = (Guid)payload.UserId,
                    };

                    var user = await _context.User.Where(u => u.UserId == payload.UserId).FirstOrDefaultAsync();
                    if (user == null) { throw new Exception("User does not exist"); }

                    switch (deduction.DeductionName)
                    {
                        case "SSS":
                            user.SSS = payload.SSS;
                            break;
                        case "Pagibig":
                            user.PagIbig = payload.PagIbig;
                            break;
                        case "PhilHealth":
                            user.PhilHealth = payload.PhilHealth;
                            break;
                    }

                    deductionAssignments.Add(deductionAssignment);
                };

                await _context.DeductionAssignment.AddRangeAsync(deductionAssignments);
                await _context.SaveChangesAsync();
                
                return payload;

            } catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<DeductionDTO> EditMandatoryDeduction([FromBody] DeductionDTO payload)
        {
            try
            {
                var user = await _context.User.Where(u => u.UserId == payload.UserId).FirstOrDefaultAsync();
                if (user == null) { throw new Exception("User does not exist"); }

                var deductionIds = payload.deductionList.Select(d => d.DeductionId).ToList();

                var deductionAssignmentsOfUser = await _context.DeductionAssignment.Where(u => u.UserId == user.UserId).ToListAsync();
                var mandatoryDeductionsFromDB = await _context.Deduction.Where(d => d.IsMandatory == true).Select(d => d.DeductionId).ToListAsync();

                var filteredDeductionAssignments = deductionAssignmentsOfUser.Where(d => mandatoryDeductionsFromDB.Contains(d.DeductionId)).ToList();

                if (filteredDeductionAssignments.Any())
                {
                    _context.DeductionAssignment.RemoveRange(filteredDeductionAssignments);

                    var deductionIdToDelete = filteredDeductionAssignments.Select(d => d.DeductionId);

                    var deductionsToRemove = await _context.Deduction
                        .Where(d => deductionIdToDelete.Contains(d.DeductionId))
                        .ToListAsync();

                    foreach(var d in deductionsToRemove)
                    {
                        switch (d.DeductionName)
                        {
                            case "SSS":
                                user.SSS = "";
                                _context.User.Update(user);
                                await _context.SaveChangesAsync();
                                break;
                            case "Pagibig":
                                user.PagIbig = "";
                                _context.User.Update(user);
                                await _context.SaveChangesAsync();
                                break;
                            case "PhilHealth":
                                user.PhilHealth = "";
                                _context.User.Update(user);
                                await _context.SaveChangesAsync();
                                break;
                        }
                    }

                    _context.Deduction.RemoveRange(deductionsToRemove);

                    await _context.SaveChangesAsync();
                }
                
                foreach (var deductionDTO in payload.deductionList)
                {
                    var newDeduction = new Deduction
                    {
                        DeductionName = deductionDTO.DeductionName,
                        DeductionDescription = deductionDTO.DeductionName,
                        Amount = (decimal)deductionDTO.Amount,
                        IsMandatory = deductionDTO.IsMandatory,
                        Type = "Contribution",
                        Date = DateTime.Now,
                    };
                    _context.Deduction.Add(newDeduction);
                    await _context.SaveChangesAsync();
                    
                    var newAssignment = new DeductionAssignment
                    {
                        UserId = (Guid)payload.UserId,
                        DeductionId = newDeduction.DeductionId
                    };
                    _context.DeductionAssignment.Add(newAssignment);

                    switch (newDeduction.DeductionName)
                    {
                        case "SSS":
                            user.SSS = payload.SSS;
                            _context.User.Update(user);
                            await _context.SaveChangesAsync();
                            break;
                        case "Pagibig":
                            user.PagIbig = payload.PagIbig;
                            _context.User.Update(user);
                            await _context.SaveChangesAsync();
                            break;
                        case "PhilHealth":
                            user.PhilHealth = payload.PhilHealth;
                            _context.User.Update(user);
                            await _context.SaveChangesAsync();
                            break;
                    }
                    
                }

                await _context.SaveChangesAsync();

                return payload;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<Deduction> EditNonMandatoryDeduction([FromBody] DeductionDTO payload)
        {
            try
            {
                var deduction = await _context.Deduction.Where(u => u.DeductionId == payload.DeductionId).FirstOrDefaultAsync();

                deduction.DeductionName = payload.DeductionName;
                deduction.DeductionDescription = payload.DeductionDescription;
                deduction.Amount = (decimal)payload.Amount;
                deduction.Type = payload.Type; 

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
                var deduction = await _context.Deduction.Where(u => u.DeductionId == DeductionId).FirstOrDefaultAsync();
                
                if (deduction == null) { return "Deduction does not exist"; }

                var deductionAssignments = await _context.DeductionAssignment.Where(d => d.DeductionId == DeductionId).ToListAsync();

                if(deductionAssignments.Any()) { _context.DeductionAssignment.RemoveRange(deductionAssignments); }

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
