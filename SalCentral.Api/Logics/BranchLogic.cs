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
                    BranchId = (Guid)payload.BranchId,
                    BranchName = payload.BranchName,
                    Address = payload.Address
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
    }
}
