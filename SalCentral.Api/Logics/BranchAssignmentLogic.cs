﻿using Microsoft.AspNetCore.Mvc;
using SalCentral.Api.DbContext;
using SalCentral.Api.DTOs;
using SalCentral.Api.DTOs.UserDTO;
using static SalCentral.Api.Pagination.PaginationRequestQuery;

namespace SalCentral.Api.Logics
{
    public class BranchAssignmentLogic
    {
        private readonly ApiDbContext _context;

        public BranchAssignmentLogic(ApiDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetBranchAssignment([FromQuery] PaginationRequest paginationRequest, Guid UserId)
        {
            IQueryable<BranchAssignmentDTO> query = from u in _context.BranchAssignment
                                                    where u.UserId == UserId
                                                    select new BranchAssignmentDTO()
                                                    {
                                                        BranchAssignmentId = u.BranchAssignmentId,
                                                        BranchId = u.BranchId,
                                                        UserId = u.UserId,
                                                        FirstName = _context.User
                                                                    .Where(y => y.UserId == u.UserId)
                                                                    .Select(f => f.FirstName)
                                                                    .FirstOrDefault(),
                                                        LastName = _context.User
                                                                    .Where(y => y.UserId == u.UserId)
                                                                    .Select(f => f.LastName)
                                                                    .FirstOrDefault(),
                                                        BranchName = _context.Branch
                                                                     .Where(b => b.BranchId == u.BranchId)
                                                                     .Select(b => b.BranchName)
                                                                     .FirstOrDefault(),
                                                    };

            if (query == null) throw new Exception("No assignments found for user.");

            var responsewrapper = await PaginationLogic.PaginateData(query, paginationRequest);
            var users = responsewrapper.Results;

            if (users.Any())
            {
                return responsewrapper;
            }

            return null;
        }
    }
}
