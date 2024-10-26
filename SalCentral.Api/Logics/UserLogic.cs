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

        public async Task<object> AuthenticateUser([FromQuery] UserDTO payload)
        {
            try
            {
                payload.Password = HashingLogic.HashData(payload.Password);
                var user = await _context.User
                                .Where(u => u.SMEmployeeID == payload.SMEmployeeID && u.Password == payload.Password)
                                .Select(u => new UserDTO
                                {
                                    UserId = u.UserId,
                                    FullName = u.FirstName + ' ' + u.LastName,
                                    FirstName = u.FirstName,
                                    LastName = u.LastName,
                                    Email = u.Email,
                                    ContactNo = u.ContactNo,
                                    SMEmployeeID = u.SMEmployeeID,
                                    HireDate = u.HireDate,
                                    Password = u.Password,
                                    RoleId = u.RoleId,
                                    RoleName = _context.Role
                                    .Where(r => r.RoleId == u.RoleId)
                                    .Select(r => r.RoleName)
                                    .FirstOrDefault(),
                                    BranchId = u.BranchId,
                                    BranchName = _context.Branch
                                    .Where(b => b.BranchId == u.BranchId)
                                    .Select(b => b.BranchName)
                                    .FirstOrDefault(),
                                    AuthorizationKey = u.AuthorizationKey,
                                })
                                .FirstOrDefaultAsync();

                if (user == null) { throw new Exception("Incorrect account details. Please try again."); }

                return user;
            } catch (Exception ex) { 
                throw new Exception(ex.Message);
            }
        }

        public async Task<User> AuthenticateAdmin([FromQuery] UserDTO payload)
        {
            try
            {
                var user = await _context.User.Where(u => u.SMEmployeeID == payload.SMEmployeeID && u.AuthorizationKey == payload.AuthorizationKey).FirstOrDefaultAsync();
                if (user == null) { throw new Exception("Incorrect authorization key. Please try again."); }
                if (user.RoleId.ToString() == "f711d87e-f3e9-4ebd-9d2d-08dcbd237523") { throw new Exception("You do not have the permissions to authorize this action."); }

                await GenerateAuthorizationKey((Guid)user.UserId);

                return user;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        // GET, PUT, POST, DELETE

        public async Task<object> GetUsers([FromQuery] PaginationRequest paginationRequest, [FromQuery] UserFilter userFilter)
        {
            try
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
                                                Password = u.Password,
                                                RoleId = u.RoleId,
                                                RoleName = _context.Role
                                                               .Where(r => r.RoleId == u.RoleId)
                                                               .Select(r => r.RoleName)
                                                               .FirstOrDefault(),
                                                BranchId = u.BranchId,
                                                BranchName = _context.Branch
                                                                .Where(b => b.BranchId == u.BranchId)
                                                                .Select(b => b.BranchName)
                                                                .FirstOrDefault(),
                                                SalaryRate = u.SalaryRate,
                                                AuthorizationKey = u.AuthorizationKey,
                                            };

                if (query == null) throw new Exception("No users found.");

                if (userFilter.BranchId.HasValue)
                {
                    query = query.Where(i => i.BranchId.ToString().Contains(userFilter.BranchId.ToString()));
                }

                if (!string.IsNullOrWhiteSpace(userFilter.FullName))
                {
                    string SearchQuery = userFilter.FullName.Trim().ToLower();
                    query = query.Where(i => (i.FirstName + " " + i.LastName).ToLower().Contains(SearchQuery));
                }

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
            catch (Exception ex) 
            {
                throw new Exception(ex.Message);
            }
            
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
                                                Password = u.Password,
                                                RoleId = u.RoleId,
                                                RoleName = _context.Role
                                                               .Where(r => r.RoleId == u.RoleId)
                                                               .Select(r => r.RoleName)
                                                               .FirstOrDefault(),
                                                SalaryRate = u.SalaryRate,
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
            } catch (Exception ex) { 
                throw new Exception(ex.Message);
            }

        }


        public async Task<User> PostUser([FromBody] UserDTO payload)
        {
            try
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
                    Password = HashingLogic.HashData(payload.Password),
                    RoleId = (Guid)payload.RoleId,
                    BranchId = (Guid)payload.BranchId,
                    AuthorizationKey = payload.RoleId == Guid.Parse("f711d87e-f3e9-4ebd-9d2d-08dcbd237523") ? null : Guid.NewGuid(),
                    SalaryRate = (decimal)payload.SalaryRate
                };

                var exists = _context.User.Where(u => u.SMEmployeeID == payload.SMEmployeeID).Any();

                if (exists)
                {
                    throw new Exception("The user provided already exists.");
                }

                await _context.User.AddAsync(user);

                return user;

            } catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
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
                user.BranchId = (Guid)payload.BranchId;
                user.RoleId = (Guid)payload.RoleId;
                user.SalaryRate = (decimal)payload.SalaryRate;

                _context.User.Update(user);
                await _context.SaveChangesAsync();

                return user;

            } catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<User> GenerateAuthorizationKey(Guid UserId)
        {
            try
            {
                var user = await _context.User.Where(u => u.UserId == UserId).FirstOrDefaultAsync();

                user.AuthorizationKey = Guid.NewGuid();

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
