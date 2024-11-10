using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalCentral.Api.DbContext;
using SalCentral.Api.DTOs.UserDTO;
using SalCentral.Api.Models;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
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
                                    SSS = u.SSS,
                                    PagIbig = u.PagIbig,
                                    PhilHealth = u.PhilHealth,
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
                                                SSS = u.SSS,
                                                PagIbig = u.PagIbig,
                                                TIN = u.TIN,
                                                PhilHealth = u.PhilHealth,
                                            };

                if (query == null) throw new Exception("No users found.");

                // Apply searchKeyword filter
                if (!string.IsNullOrWhiteSpace(userFilter.SearchKeyword))
                {
                    string searchQuery = userFilter.SearchKeyword.Trim().ToLower();
                    query = query.Where(i => i.FirstName.ToLower().Contains(searchQuery)
                                          || i.LastName.ToLower().Contains(searchQuery)
                                          || i.SMEmployeeID.ToLower().Contains(searchQuery));
                }

                if (userFilter.BranchId.HasValue)
                {
                    query = query.Where(i => i.BranchId.ToString().Contains(userFilter.BranchId.ToString()));
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
                                                TIN = u.TIN,
                                                SSS = u.SSS,
                                                PagIbig = u.PagIbig,
                                                PhilHealth = u.PhilHealth,
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
                    AuthorizationKey = payload.RoleId == Guid.Parse("f711d87e-f3e9-4ebd-9d2d-08dcbd237523") ? null : GetRandomlyGenerateBase64String(8),
                    SalaryRate = (decimal)payload.SalaryRate,
                    SSS = payload.SSS == null ? null : payload.SSS,
                    PagIbig = payload.PagIbig == null ? null : payload.PagIbig,
                    PhilHealth = payload.PhilHealth == null ? null : payload.PhilHealth,
                    TIN = payload.TIN == null ? null : payload.TIN,
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

                if (!string.IsNullOrEmpty(payload.FirstName)) user.FirstName = payload.FirstName;
                if (!string.IsNullOrEmpty(payload.LastName)) user.LastName = payload.LastName;
                if (!string.IsNullOrEmpty(payload.Email)) user.Email = payload.Email;
                if (!string.IsNullOrEmpty(payload.ContactNo)) user.ContactNo = payload.ContactNo;
                if (!string.IsNullOrEmpty(payload.Password)) user.Password = HashingLogic.HashData(payload.Password);
                if (payload.BranchId.HasValue) user.BranchId = payload.BranchId.Value;
                if (payload.RoleId.HasValue) user.RoleId = payload.RoleId.Value;
                if (payload.SalaryRate.HasValue) user.SalaryRate = payload.SalaryRate.Value;
                if (payload.SSS != null) user.SSS = payload.SSS;
                if (payload.PagIbig != null) user.PagIbig = payload.PagIbig;
                if (payload.PhilHealth != null) user.PhilHealth = payload.PhilHealth;
                if (payload.TIN != null) user.TIN = payload.TIN;

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

                user.AuthorizationKey = GetRandomlyGenerateBase64String(8);

                _context.User.Update(user);
                await _context.SaveChangesAsync();

                return user;

            } catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static string GetRandomlyGenerateBase64String(int count)
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(count));
        }

        public async Task<object> GetUsersWithSchedule(PaginationRequest paginationRequest, UserFilter userFilter)
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
                                                SSS = u.SSS,
                                                PagIbig = u.PagIbig,
                                                PhilHealth = u.PhilHealth,
                                                Schedule = _context.Schedule
                                                               .Where(s => s.UserId == u.UserId)
                                                               .FirstOrDefault(),
                  
                                            };

                if (query == null) throw new Exception("No users found.");

                // Apply searchKeyword filter
                if (!string.IsNullOrWhiteSpace(userFilter.SearchKeyword))
                {
                    string searchQuery = userFilter.SearchKeyword.Trim().ToLower();
                    query = query.Where(i => i.FirstName.ToLower().Contains(searchQuery)
                                          || i.LastName.ToLower().Contains(searchQuery)
                                          || i.SMEmployeeID.ToLower().Contains(searchQuery));
                }

                if (userFilter.BranchId.HasValue)
                {
                    query = query.Where(i => i.BranchId.ToString().Contains(userFilter.BranchId.ToString()));
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
    }
}
