using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalCentral.Api.DbContext;
using SalCentral.Api.DTOs.UserDTO;
using SalCentral.Api.Models;
using static SalCentral.Api.Pagination.PaginationRequestQuery;

namespace SalCentral.Api.Logics
{
    public class UserLogic
    {
        private readonly ApiDbContext _context;

        public UserLogic(ApiDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetUsers([FromQuery] PaginationRequest paginationRequest, [FromQuery] UserFilter userFilter)
        {
            IQueryable<UserDTO> query = from u in _context.User
                                        select new UserDTO()
                                        {
                                            UserId = u.UserId,
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
                                        };

           if (query == null) throw new Exception("No users found.");

           if (!string.IsNullOrWhiteSpace(userFilter.FirstName))
           {
                string SearchQuery = userFilter.FirstName.Trim();
                query = query.Where(i => i.FirstName.Contains(SearchQuery));
           }
           if (!string.IsNullOrWhiteSpace(userFilter.LastName))
           {
                string SearchQuery = userFilter.LastName.Trim();
                query = query.Where(i => i.LastName.Contains(SearchQuery));
           }

            var responsewrapper = await PaginationLogic.PaginateData(query, paginationRequest);
            var users = responsewrapper.Results;

            if (users.Any())
            {
                return responsewrapper;
            }

            return null;
        }

        public async Task<object> PostUsers([FromBody] UserDTO payload)
        {
            var user = new User()
            {
                FirstName = payload.FirstName,
                LastName = payload.LastName,
                Email = payload.Email,
                ContactNo = payload.ContactNo,
                SMEmployeeID = payload.SMEmployeeID,
                HireDate = DateTime.Now,
                Password = payload.Password,
                RoleId = (Guid)payload.RoleId,
            };

            var exists = _context.User.Where(u => u.SMEmployeeID == payload.SMEmployeeID).Any();
            if (exists)
            {
                throw new Exception("The user provided already exists.");
            }

            _context.User.AddAsync(user);
            _context.SaveChangesAsync();

            return user;
        }


    }
}
