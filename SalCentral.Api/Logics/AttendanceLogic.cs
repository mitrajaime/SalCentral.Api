using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalCentral.Api.DbContext;
using SalCentral.Api.DTOs.AttendanceDTO;
using SalCentral.Api.Models;
using System;
using static SalCentral.Api.Pagination.PaginationRequestQuery;

namespace SalCentral.Api.Logics
{
    public class AttendanceLogic
    {
        private readonly ApiDbContext _context;
        private readonly UserLogic _userLogic;
        private readonly BranchLogic _branchLogic;

        public AttendanceLogic(ApiDbContext context, UserLogic userLogic, BranchLogic branchLogic)
        {
            _userLogic = userLogic;
            _context = context;
            _branchLogic = branchLogic;
        }

        public async Task<object> GetAttendance([FromQuery] PaginationRequest paginationRequest)
        {
            try
            {
                IQueryable<AttendanceDTO> query = from q in _context.Attendance
                                                  join u in _context.User on q.UserId equals u.UserId
                                                  join b in _context.Branch on u.BranchId equals b.BranchId
                                                  select new AttendanceDTO()
                                                  {
                                                      AttendanceId = q.AttendanceId,
                                                      BranchId = b.BranchId,
                                                      UserId = q.UserId,
                                                      User = _context.User.Where(u => u.UserId == q.UserId).Select(u => u.FirstName).FirstOrDefault() + ' ' + _context.User.Where(u => u.UserId == q.UserId).Select(u => u.LastName).FirstOrDefault(),
                                                      SMEmployeeId = _context.User.Where(u => u.UserId == q.UserId).Select(u => u.SMEmployeeID).FirstOrDefault(),
                                                      Date = q.Date,
                                                      TimeIn = q.TimeIn,
                                                      TimeOut = q.TimeOut,
                                                      HoursRendered = q.HoursRendered,
                                                      OverTimeHours = q.OverTimeHours,
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

        public async Task<object> GetAttendanceOfEmployee([FromQuery] PaginationRequest paginationRequest, [FromQuery] AttendanceFilter attendanceFilter) 
        {
            try
            {
                IQueryable<AttendanceDTO> query = from q in _context.Attendance
                                                  join u in _context.User on q.UserId equals u.UserId
                                                  join b in _context.Branch on u.BranchId equals b.BranchId
                                                  where q.UserId == attendanceFilter.UserId && b.BranchId == attendanceFilter.BranchId
                                                  orderby q.Date descending
                                                  select new AttendanceDTO()
                                                  {
                                                      AttendanceId = q.AttendanceId,
                                                      BranchId = b.BranchId,
                                                      UserId = q.UserId,
                                                      User = _context.User.Where(u => u.UserId == q.UserId).Select(u => u.FirstName).FirstOrDefault() + ' ' + _context.User.Where(u => u.UserId == q.UserId).Select(u => u.LastName).FirstOrDefault(),
                                                      SMEmployeeId = _context.User.Where(u => u.UserId == q.UserId).Select(u => u.SMEmployeeID).FirstOrDefault(),
                                                      Date = q.Date,
                                                      TimeIn = q.TimeIn,
                                                      TimeOut = q.TimeOut,
                                                      HoursRendered = q.HoursRendered,
                                                      OverTimeHours = q.OverTimeHours,
                                                  };

                var responsewrapper = await PaginationLogic.PaginateData(query, paginationRequest);
                var attendance = responsewrapper.Results;

                if (attendance.Any())
                {
                    return responsewrapper;
                }

                throw new Exception("No attendance found for user.");

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<object> GetEmployeeAttendanceToday([FromQuery] PaginationRequest paginationRequest, [FromQuery] AttendanceFilter attendanceFilter)
        {
            try
            {
                IQueryable<AttendanceDTO> query = from q in _context.Attendance
                                                  join u in _context.User on q.UserId equals u.UserId
                                                  join b in _context.Branch on u.BranchId equals b.BranchId
                                                  where q.UserId == attendanceFilter.UserId && b.BranchId == attendanceFilter.BranchId && q.Date.Date == DateTime.Today
                                                  orderby q.Date descending
                                                  select new AttendanceDTO()
                                                  {
                                                      AttendanceId = q.AttendanceId,
                                                      BranchId = b.BranchId,
                                                      UserId = q.UserId,
                                                      User = _context.User.Where(u => u.UserId == q.UserId).Select(u => u.FirstName).FirstOrDefault() + ' ' + _context.User.Where(u => u.UserId == q.UserId).Select(u => u.LastName).FirstOrDefault(),
                                                      SMEmployeeId = _context.User.Where(u => u.UserId == q.UserId).Select(u => u.SMEmployeeID).FirstOrDefault(),
                                                      Date = q.Date,
                                                      TimeIn = q.TimeIn,
                                                      TimeOut = q.TimeOut,
                                                      HoursRendered = q.HoursRendered,
                                                      OverTimeHours = q.OverTimeHours,
                                                  };

                var responsewrapper = await PaginationLogic.PaginateData(query, paginationRequest);
                var attendance = responsewrapper.Results;

                if (attendance.Any())
                {
                    return responsewrapper;
                }

                throw new Exception("No attendance found for user on: " + DateTime.Today.ToString("d"));

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

                // for debugging
                var timeIn = new Attendance()
                {
                    Date = DateTime.UtcNow,
                    TimeIn = DateTime.UtcNow,
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
                var attendance = await (from a in _context.Attendance
                                        join u in _context.User
                                        on a.UserId equals u.UserId
                                        join b in _context.Branch
                                        on u.BranchId equals b.BranchId
                                        where a.UserId == payload.UserId && b.BranchId == payload.BranchId
                                        orderby a.Date descending
                                        select a).FirstOrDefaultAsync();

                attendance.TimeOut = DateTime.Now;
                TimeSpan timeRendered = attendance.TimeOut - attendance.TimeIn;
                attendance.HoursRendered = (int)timeRendered.TotalHours - 1;

                if(attendance.HoursRendered > 8)
                {
                    attendance.OverTimeHours = attendance.HoursRendered - 8;
                    attendance.HoursRendered = 8;
                }

                _context.Update(attendance);
                await _context.SaveChangesAsync();

                return attendance;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<object> UndoTimeOut([FromBody] AttendanceDTO payload)
        {
            try
            {
                var attendance = await _context.Attendance.Where(u => u.UserId == payload.UserId)
                                                          .OrderByDescending(u => u.Date)
                                                          .FirstOrDefaultAsync();

                attendance.TimeOut = DateTime.MinValue;
                attendance.HoursRendered = 0;
                attendance.OverTimeHours = 0;

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
