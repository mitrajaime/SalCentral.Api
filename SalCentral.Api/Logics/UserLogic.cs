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

        public async Task<User> AuthenticateUser([FromQuery] UserDTO payload)
        {
            try
            {
                var user = await _context.User.Where(u => u.SMEmployeeID == payload.SMEmployeeID && u.Password == payload.Password).FirstOrDefaultAsync();

                if (user == null) { throw new Exception("Incorrect account details. Please try again."); }

                return user;
            } catch (Exception ex) { 
                throw new Exception(ex.Message);
            }
        }

        // GET, PUT, POST, DELETE

        public async Task<object> GetUsers([FromQuery] PaginationRequest paginationRequest, [FromQuery] UserFilter userFilter)
        {
            IQueryable<UserDTO> query = from u in _context.User
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
                                            assignmentList = _context.BranchAssignment.Where(b => b.UserId == u.UserId)
                                                           .ToList(),
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
            if (!string.IsNullOrWhiteSpace(userFilter.SMEmployeeId))
            {
                string SearchQuery = userFilter.SMEmployeeId.Trim();
                query = query.Where(i => i.SMEmployeeID.Contains(SearchQuery));
            }

            var responsewrapper = await PaginationLogic.PaginateData(query, paginationRequest);
            var users = responsewrapper.Results;

            if (users.Any())
            {
                return responsewrapper;
            }

            return null;
        }

        public async Task<object> GetUserById([FromQuery] PaginationRequest paginationRequest, Guid UserId)
        {
            try
            {

                IQueryable<UserDTO> query = from u in _context.User
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
                                                assignmentList = _context.BranchAssignment.Where(b => b.UserId == u.UserId)
                                                               .ToList(),
                                            };

                if (query == null) throw new Exception("No users found.");

                var responsewrapper = await PaginationLogic.PaginateData(query, paginationRequest);
                var users = responsewrapper.Results;

                if (users.Any())
                {
                    return responsewrapper;
                }

                return null;
            } catch (Exception ex) { 
                throw new Exception(ex.Message);
            }

        }


        public async Task<User> PostUser([FromBody] UserDTO payload)
        {
            var user = new User()
            {
                UserId = new Guid(),
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

            await _context.User.AddAsync(user);
            await _context.SaveChangesAsync();

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
                user.Password = payload.Password;
                user.RoleId = (Guid)payload.RoleId;

                _context.User.Update(user);
                await _context.SaveChangesAsync();

                return user;

            } catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
