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

        public async Task<object> GetAttendanceById([FromQuery] PaginationRequest paginationRequest, Guid AttendanceId)
        {
            try
            {
                var attendance = await _context.Attendance.FirstOrDefaultAsync(a => a.AttendanceId == AttendanceId);

                if(attendance == null)
                {
                    throw new Exception("No attendance found");
                }

                return attendance;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<object> GetAttendanceOfEmployee([FromQuery] PaginationRequest paginationRequest, [FromQuery] AttendanceFilter attendanceFilter)
        {
            try
            {
                var schedule = await _context.Schedule.FirstOrDefaultAsync(s => s.UserId == attendanceFilter.UserId);

                if (schedule == null)
                {
                    throw new Exception("No schedule found for the specified user.");
                }

                IQueryable<AttendanceDTO> query = from q in _context.Attendance
                                                  join u in _context.User on q.UserId equals u.UserId
                                                  join b in _context.Branch on u.BranchId equals b.BranchId
                                                  where q.UserId == attendanceFilter.UserId
                                                        && b.BranchId == attendanceFilter.BranchId
                                                        && q.Date.Date >= attendanceFilter.StartDate.Value.Date
                                                        && q.Date.Date <= attendanceFilter.EndDate.Value.Date
                                                  orderby q.Date.Date ascending
                                                  select new AttendanceDTO
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
                                                      AllowedOvertimeHours = q.AllowedOvertimeHours,
                                                      IsDayOff = q.IsDayOff
                                                  };

                var attendanceList = await query.ToListAsync();
                var existingDates = attendanceList.Select(a => a.Date.Value.Date).ToHashSet();

                var currentDate = attendanceFilter.StartDate.Value.Date;
                var newAttendanceRecords = new List<Attendance>();

                while (currentDate <= attendanceFilter.EndDate.Value.Date)
                {
                    // Check if attendance already exists for this date or if it's already a day off
                    if (!existingDates.Contains(currentDate) || attendanceList.Any(a => a.Date.Value.Date == currentDate && a.IsDayOff))
                    {
                        bool isScheduledDay = (currentDate.DayOfWeek == DayOfWeek.Monday && schedule.Monday) ||
                                              (currentDate.DayOfWeek == DayOfWeek.Tuesday && schedule.Tuesday) ||
                                              (currentDate.DayOfWeek == DayOfWeek.Wednesday && schedule.Wednesday) ||
                                              (currentDate.DayOfWeek == DayOfWeek.Thursday && schedule.Thursday) ||
                                              (currentDate.DayOfWeek == DayOfWeek.Friday && schedule.Friday) ||
                                              (currentDate.DayOfWeek == DayOfWeek.Saturday && schedule.Saturday == true) ||
                                              (currentDate.DayOfWeek == DayOfWeek.Sunday && schedule.Sunday == true);

                        // Prevent creating a day-off record if it already exists for the current date
                        if (!attendanceList.Any(a => a.Date.Value.Date == currentDate && a.IsDayOff))
                        {
                            var newAttendance = new Attendance
                            {
                                AttendanceId = Guid.NewGuid(),
                                UserId = schedule.UserId,
                                Date = currentDate,
                                TimeIn = DateTime.MinValue,
                                TimeOut = DateTime.MinValue,
                                HoursRendered = 0,
                                OverTimeHours = 0,
                                AllowedOvertimeHours = 0,
                                IsDayOff = !isScheduledDay
                            };

                            newAttendanceRecords.Add(newAttendance);
                        }
                    }

                    currentDate = currentDate.AddDays(1);
                }

                if (newAttendanceRecords.Any())
                {
                    await _context.Attendance.AddRangeAsync(newAttendanceRecords);
                    await _context.SaveChangesAsync();
                }

                attendanceList.AddRange(newAttendanceRecords.Select(a => new AttendanceDTO
                {
                    AttendanceId = a.AttendanceId,
                    BranchId = schedule.BranchId,
                    UserId = a.UserId,
                    User = _context.User.Where(u => u.UserId == a.UserId).Select(u => u.FirstName).FirstOrDefault() + ' ' + _context.User.Where(u => u.UserId == a.UserId).Select(u => u.LastName).FirstOrDefault(),
                    SMEmployeeId = _context.User.Where(u => u.UserId == a.UserId).Select(u => u.SMEmployeeID).FirstOrDefault(),
                    Date = a.Date,
                    TimeIn = null,
                    TimeOut = null,
                    HoursRendered = 0,
                    OverTimeHours = 0,
                    AllowedOvertimeHours = 0,
                    IsDayOff = a.IsDayOff
                }));

                IQueryable<AttendanceDTO> queryAfterChecking = from q in _context.Attendance
                                                               join u in _context.User on q.UserId equals u.UserId
                                                               join b in _context.Branch on u.BranchId equals b.BranchId
                                                               where q.UserId == attendanceFilter.UserId
                                                                     && b.BranchId == attendanceFilter.BranchId
                                                                     && q.Date.Date >= attendanceFilter.StartDate.Value.Date
                                                                     && q.Date.Date <= attendanceFilter.EndDate.Value.Date
                                                               orderby q.Date.Date descending
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
                                                                   AllowedOvertimeHours = q.AllowedOvertimeHours,
                                                                   IsDayOff = q.IsDayOff
                                                               };

                var responsewrapper = await PaginationLogic.PaginateData(queryAfterChecking, paginationRequest);

                if (responsewrapper.Results.Any())
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

        public async Task<object> EditAttendance([FromBody] AttendanceDTO payload)
        {
            try
            {
                var attendance = await _context.Attendance.Where(a => a.AttendanceId == payload.AttendanceId).FirstOrDefaultAsync();

                if (payload.TimeIn != null) attendance.TimeIn = (DateTime)payload.TimeIn;
                if (payload.TimeOut != null) attendance.TimeOut = (DateTime)payload.TimeOut;

                TimeSpan timeRendered = attendance.TimeOut - attendance.TimeIn;
                attendance.HoursRendered = (int)timeRendered.TotalHours - 1;

                if (attendance.HoursRendered > 4)
                {
                    attendance.HoursRendered = (int)timeRendered.TotalHours - 1;
                }

                if (attendance.HoursRendered > 8)
                {
                    attendance.OverTimeHours = attendance.HoursRendered - 8;
                    attendance.HoursRendered = 8;
                }
                else
                {
                    attendance.OverTimeHours = 0;
                }

                _context.Attendance.Update(attendance);
                await _context.SaveChangesAsync();

                return attendance;
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
                                                      IsDayOff = q.IsDayOff
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
                var timeIn = new Attendance()
                {
                    Date = DateTime.Now,
                    TimeIn = DateTime.Now,
                    IsDayOff = false,
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
                
                if(attendance.HoursRendered > 4)
                {
                    attendance.HoursRendered = (int)timeRendered.TotalHours - 1;
                }

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

        public async Task<object> EditAllowedOvertimeHours([FromBody] AttendanceDTO payload)
        {
            try
            {
                foreach (var user in payload.userList)
                {
                    var attendanceOfEmployee = await (from a in _context.Attendance
                                                      where a.UserId == user.UserId && a.Date.Date == payload.Date
                                                      select a).FirstOrDefaultAsync();
                    if(attendanceOfEmployee == null) { throw new Exception("No attendance found for employee at this date."); }

                    attendanceOfEmployee.AllowedOvertimeHours = (int)user.allowedOvertimeHours;
                    _context.Attendance.Update(attendanceOfEmployee);
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
