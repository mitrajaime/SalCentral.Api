using Microsoft.AspNetCore.Mvc;
using SalCentral.Api.DbContext;
using SalCentral.Api.DTOs;
using SalCentral.Api.Models;
using static SalCentral.Api.Pagination.PaginationRequestQuery;


namespace SalCentral.Api.Logics
{
    public class PayrollLogic
    {
        private readonly ApiDbContext _context;

        public PayrollLogic(ApiDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetUserById([FromQuery] PaginationRequest paginationRequest, Guid UserId)
        {
            try
            {

                IQueryable<PayrollDTO> query = from u in _context.User
                                                where u.UserId == UserId
                                                select new UserDTO()
                                                {
                                                    UserId = u.UserId,
                                                    FullName = u.FirstName + ' ' + u.LastName,
                                                    FirstName = u.FirstName,
                                                    LastName = u.LastName,
                                                    Email = u.Email,
                                                    ContactNo = u.ContactNo,
                                                    SMEmployeeID = u.SMEmployeeID,
                                                    HireDate = u.HireDate,
                                                    //Photo = u.Photo,
                                                    Password = u.Password,
                                                    RoleId = u.RoleId,
                                                    RoleName = _context.Role
                                                                   .Where(r => r.RoleId == u.RoleId)
                                                                   .Select(r => r.RoleName)
                                                                   .FirstOrDefault(),
                                                    BranchId = u.BranchId,
                                                };

                if (query == null) throw new Exception("No users found.");

                var responsewrapper = await PaginationLogic.PaginateData(query, paginationRequest);
                var users = responsewrapper.Results;

                if (users.Any())
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


        public async Task<Payroll> CreatePayroll([FromBody] PayrollDTO payload)
        {
            var payroll = new Payroll()
            {
                PayrollId = new Guid(),
                HireDate = DateTime.Now,
                Password = HashingLogic.HashData(payload.Password),
                RoleId = (Guid)payload.RoleId,
                BranchId = (Guid)payload.BranchId,
            };

            var exists = _context.User.Where(u => u.SMEmployeeID == payload.SMEmployeeID).Any();

            if (exists)
            {
                throw new Exception("The user provided already exists.");
            }

            await _context.User.AddAsync(user);

            return user;
        }

        public async Task<User> EditUser([FromBody] UserDTO payload)
        {
            try
            {
                var user = await _context.User.Where(u => u.UserId == payload.UserId).FirstOrDefaultAsync();

                user.FirstName = payload.FirstName;
                user.LastName = payload.LastName;
                user.Email = payload.Email;
                user.ContactNo = payload.ContactNo;
                user.Password = HashingLogic.HashData(payload.Password);
                user.RoleId = (Guid)payload.RoleId;

                _context.User.Update(user);
                await _context.SaveChangesAsync();

                return user;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
