using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalCentral.Api.DbContext;
using SalCentral.Api.DTOs;
using SalCentral.Api.Models;
using static SalCentral.Api.Pagination.PaginationRequestQuery;

namespace SalCentral.Api.Logics
{
    public class AttendanceLogic
    {
        private readonly ApiDbContext _context;
        private readonly UserLogic _userLogic;
        private readonly BranchLogic _branchLogic;
        private readonly BranchAssignmentLogic _branchAssignmentLogic;

        public AttendanceLogic(ApiDbContext context, UserLogic userLogic, BranchLogic branchLogic, BranchAssignmentLogic branchAssignmentLogic)
        {
            _userLogic = userLogic;
            _context = context;
            _branchLogic = branchLogic;
            _branchAssignmentLogic = branchAssignmentLogic;
        }

        public async Task<object> GetAttendance([FromQuery] PaginationRequest paginationRequest)
        {
            try
            {
                IQueryable<AttendanceDTO> query = from q in _context.Attendance
                                                  select new AttendanceDTO()
                                                  {
                                                      AttendanceId = q.AttendanceId,
                                                      BranchId = q.BranchId,
                                                      UserId = q.UserId,
                                                      Date = q.Date,
                                                      TimeIn = q.TimeIn,
                                                      TimeOut = q.TimeOut,
                                                  };

                if (query == null) throw new Exception("No attendance found.");

                var responsewrapper = await PaginationLogic.PaginateData(query, paginationRequest);
                var attendance = responsewrapper.Results;

                if (attendance.Any())
                {
                    return responsewrapper;
                }

                return null;

            } catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<object> GetAttendanceOfEmployee([FromQuery] PaginationRequest paginationRequest, string? SMEmployeeId, string? password, Guid? BranchId)
        {
            try
            {
                var userId = _context.User.Where(e => e.SMEmployeeID == SMEmployeeId && e.Password == password).Select(e => e.UserId).FirstOrDefault();
                if (userId == null) throw new Exception("Incorrect user details. Please try again.");

                IQueryable<AttendanceDTO> query = from q in _context.Attendance
                                                  where q.UserId == userId && q.BranchId == BranchId
                                                  select new AttendanceDTO()
                                                  {
                                                      AttendanceId = q.AttendanceId,
                                                      BranchId = q.BranchId,
                                                      UserId = q.UserId,
                                                      Date = q.Date,
                                                      TimeIn = q.TimeIn,
                                                      TimeOut = q.TimeOut,
                                                  };

                if (query == null) throw new Exception("No attendance found for user.");

                var responsewrapper = await PaginationLogic.PaginateData(query, paginationRequest);
                var attendance = responsewrapper.Results;

                if (attendance.Any())
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

        public async Task<Attendance> TimeIn([FromBody] AttendanceDTO payload)
        {
            try
            {
                //var timeIn = new Attendance()
                //{
                //    Date = DateTime.Now,
                //    TimeIn = DateTime.Now,
                //    BranchId = (Guid)payload.BranchId,
                //    UserId = (Guid)payload.UserId,
                //};

                var timeIn = new Attendance()
                {
                    Date = DateTime.UtcNow,
                    TimeIn = DateTime.UtcNow,
                    BranchId = (Guid)payload.BranchId,
                    UserId = (Guid)payload.UserId,
                };

                _context.Add(timeIn);
                await _context.SaveChangesAsync();

                return timeIn;

            } catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<object> TimeOut([FromBody] AttendanceDTO payload)
        {
            try
            {
                var attendance = await _context.Attendance.Where(u => u.UserId == payload.UserId && u.BranchId == payload.BranchId)
                                                          .OrderByDescending(u => u.Date)
                                                          .FirstOrDefaultAsync();

                attendance.TimeOut = DateTime.Now;
                TimeSpan timeRendered = attendance.TimeOut - attendance.TimeIn;
                attendance.HoursRendered = (int)timeRendered.TotalHours;

                // attendance.OverTimeHours = ___;

                _context.Update(attendance);
                await _context.SaveChangesAsync();

                return attendance;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
