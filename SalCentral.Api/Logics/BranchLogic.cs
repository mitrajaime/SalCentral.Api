using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalCentral.Api.DbContext;
using SalCentral.Api.DTOs;
using SalCentral.Api.DTOs.UserDTO;
using SalCentral.Api.Models;
using System.Linq.Expressions;

namespace SalCentral.Api.Logics
{
    public class BranchLogic
    {
        private readonly ApiDbContext _context;

        public BranchLogic(ApiDbContext context)
        {
            _context = context;
        }

        public async Task<object> CreateBranch([FromBody] BranchDTO payload)
        {
            try
            {
                var branch = new Branch()
                {
                    BranchName = payload.BranchName,
                };

                var exists = _context.Branch.Where(b => b.BranchName == payload.BranchName).Any();
                if(exists)
                {
                    throw new Exception("This branch already exists.");
                }

                await _context.Branch.AddAsync(branch);
                await _context.SaveChangesAsync();
                return branch;

            } catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<object> PostUsersBranch([FromBody] UserDTO payload, Guid UserId)
        {
            try
            {
                var duplicates = payload.assignmentList
                    .GroupBy(a => new { a.UserId, a.BranchId })
                    .Where(l => l.Count() > 1)
                    .Select(l => l.Key)
                    .ToList();

                if (duplicates.Any())
                {
                    throw new Exception("Duplicate assignments found");
                }

                foreach (var a in payload.assignmentList)
                {

                    var assignment = new BranchAssignment()
                    {
                        UserId = UserId,
                        BranchId = a.BranchId,
                    };
                    _context.BranchAssignment.Add(assignment);
                }

                await _context.SaveChangesAsync();

                return payload;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
