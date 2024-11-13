using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalCentral.Api.DbContext;
using SalCentral.Api.DTOs.AttendanceDTO;
using SalCentral.Api.DTOs.ScheduleDTO;
using SalCentral.Api.DTOs.UserDTO;
using SalCentral.Api.Models;
using System.Linq.Expressions;
using static SalCentral.Api.Pagination.PaginationRequestQuery;

namespace SalCentral.Api.Logics
{
    public class ScheduleLogic
    {
        private readonly ApiDbContext _context;

        public ScheduleLogic(ApiDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetSchedule([FromQuery] PaginationRequest paginationRequest, [FromQuery] ScheduleFilter scheduleFilter)
        {
            try
            {
                var query = from q in _context.Schedule
                            join u in _context.User on q.UserId equals u.UserId
                            join b in _context.Branch on q.BranchId equals b.BranchId
                            select new ScheduleDTO
                            {
                                ScheduleId = q.ScheduleId,
                                UserId = q.UserId,
                                FullName = u.FirstName + " " + u.LastName,
                                FirstName = u.FirstName,
                                LastName = u.LastName,
                                SMEmployeeID = u.SMEmployeeID,
                                BranchId = q.BranchId,
                                BranchName = b.BranchName,
                                Monday = q.Monday,
                                Tuesday = q.Tuesday,
                                Wednesday = q.Wednesday,
                                Thursday = q.Thursday,
                                Friday = q.Friday,
                                Saturday = q.Saturday,
                                Sunday = q.Sunday,
                                ExpectedTimeIn = q.ExpectedTimeIn.ToString(),
                                ExpectedTimeOut = q.ExpectedTimeOut.ToString(),
                            };

                if (query == null) throw new Exception("No Schedules found.");

                if (!string.IsNullOrWhiteSpace(scheduleFilter.FirstName))
                {
                    string searchQuery = scheduleFilter.FirstName.Trim();
                    query = query.Where(i => i.FirstName.Contains(searchQuery));
                }
                if (!string.IsNullOrWhiteSpace(scheduleFilter.LastName))
                {
                    string searchQuery = scheduleFilter.LastName.Trim();
                    query = query.Where(i => i.LastName.Contains(searchQuery));
                }
                if (!string.IsNullOrWhiteSpace(scheduleFilter.SMEmployeeID))
                {
                    string searchQuery = scheduleFilter.SMEmployeeID.Trim();
                    query = query.Where(i => i.SMEmployeeID.Contains(searchQuery));
                }

                var responseWrapper = await PaginationLogic.PaginateData(query, paginationRequest);
                var schedule = responseWrapper.Results;

                if (schedule.Any())
                {
                    return responseWrapper;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        //specific
        public async Task<object> GetScheduleByUserId([FromQuery] PaginationRequest paginationRequest, [FromQuery] ScheduleFilter scheduleFilter, Guid UserId)
        {
            try
            {
                var query = from q in _context.Schedule
                            join u in _context.User on q.UserId equals u.UserId
                            join b in _context.Branch on q.BranchId equals b.BranchId
                            where q.UserId == UserId && q.BranchId == scheduleFilter.BranchId
                            select new ScheduleDTO
                            {
                                ScheduleId = q.ScheduleId,
                                UserId = q.UserId,
                                FullName = u.FirstName + " " + u.LastName,
                                FirstName = u.FirstName,
                                LastName = u.LastName,
                                SMEmployeeID = u.SMEmployeeID,
                                BranchId = q.BranchId,
                                BranchName = b.BranchName,
                                Monday = q.Monday,
                                Tuesday = q.Tuesday,
                                Wednesday = q.Wednesday,
                                Thursday = q.Thursday,
                                Friday = q.Friday,
                                Saturday = q.Saturday,
                                Sunday = q.Sunday,
                                ExpectedTimeIn = q.ExpectedTimeIn.ToString(),
                                ExpectedTimeOut = q.ExpectedTimeOut.ToString(),
                            };

                if (query == null) throw new Exception("No Schedules found.");

                if (!string.IsNullOrWhiteSpace(scheduleFilter.FirstName))
                {
                    string searchQuery = scheduleFilter.FirstName.Trim();
                    query = query.Where(i => i.FirstName.Contains(searchQuery));
                }
                if (!string.IsNullOrWhiteSpace(scheduleFilter.LastName))
                {
                    string searchQuery = scheduleFilter.LastName.Trim();
                    query = query.Where(i => i.LastName.Contains(searchQuery));
                }
                if (!string.IsNullOrWhiteSpace(scheduleFilter.SMEmployeeID))
                {
                    string searchQuery = scheduleFilter.SMEmployeeID.Trim();
                    query = query.Where(i => i.SMEmployeeID.Contains(searchQuery));
                }

                var responseWrapper = await PaginationLogic.PaginateData(query, paginationRequest);
                var schedule = responseWrapper.Results;

                if (schedule.Any())
                {
                    return responseWrapper;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<object> CreateSchedule([FromBody] Schedule payload, Guid UserId)
        {
            try
            {
                // Click n Print
                if (payload.BranchId.ToString() == "ee6eaf8e-bd49-480d-f411-08dcbd238cc1")
                {
                    var cnpSchedule = new Schedule()
                    {
                        UserId = UserId,
                        BranchId = (Guid)payload.BranchId,
                        Monday = true,
                        Tuesday = true,
                        Wednesday = true,
                        Thursday = true,
                        Friday = true,
                        Saturday = false,
                        Sunday = false,
                        ExpectedTimeIn = new TimeSpan(9, 0, 0),  // 9:00 AM
                        ExpectedTimeOut = new TimeSpan(19, 0, 0), // 7:00 PM
                    };

                    await _context.Schedule.AddAsync(cnpSchedule);
                    return cnpSchedule;
                }

                var schedule = new Schedule()
                {
                    UserId = UserId,
                    BranchId = (Guid)payload.BranchId,
                    Monday = (bool)payload.Monday,
                    Tuesday = (bool)payload.Tuesday,
                    Wednesday = (bool)payload.Wednesday,
                    Thursday = (bool)payload.Thursday,
                    Friday = (bool)payload.Friday,
                    Saturday = (bool)payload.Saturday,
                    Sunday = (bool)payload.Sunday,
                    ExpectedTimeIn = new TimeSpan(10, 0, 0), // 10:00 AM
                    ExpectedTimeOut = new TimeSpan(22, 0, 0) // 10:00 PM
                };

                // Mitsubishi Photo
                if (payload.BranchId.ToString() == "cfdee1be-1b99-47a7-f410-08dcbd238cc1")
                {
                    // checks payload if it has more than one field (MTWTF) with false
                    var daysOfWeek = new[]
                    {
                        schedule.Monday,
                        schedule.Tuesday,
                        schedule.Wednesday,
                        schedule.Thursday,
                        schedule.Friday,
                        schedule.Saturday,
                        schedule.Sunday
                    };

                    int daysOffCount = daysOfWeek.Count(day => (bool)!day);

                    if (daysOffCount > 1)
                    {
                        throw new Exception("More than one day off is not allowed for Mitsubishi Photo.");
                    }
                }

                await _context.Schedule.AddAsync(schedule);
                return schedule;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<Schedule> EditSchedule([FromBody] ScheduleDTO payload)
        {
            try
            {
                var Schedule = await _context.Schedule.Where(u => u.ScheduleId == payload.ScheduleId).FirstOrDefaultAsync();

                Schedule.Monday = (bool)payload.Monday;
                Schedule.Tuesday = (bool)payload.Tuesday;
                Schedule.Wednesday = (bool)payload.Wednesday;
                Schedule.Thursday = (bool)payload.Thursday;
                Schedule.Friday = (bool)payload.Friday;
                Schedule.Saturday = (bool)payload.Saturday;
                Schedule.Sunday = (bool)payload.Sunday;

                if (payload.BranchId.ToString() == "cfdee1be-1b99-47a7-f410-08dcbd238cc1")
                {
                    // checks payload if it has more than one field(MTWTF) with false
                    var daysOfWeek = new[]
                    {
                        Schedule.Monday,
                        Schedule.Tuesday,
                        Schedule.Wednesday,
                        Schedule.Thursday,
                        Schedule.Friday,
                        Schedule.Saturday,
                        Schedule.Sunday
                    };

                    int daysOffCount = daysOfWeek.Count(day => (bool)!day);

                    if (daysOffCount > 1)
                    {
                        throw new Exception("More than one day off is not allowed for this branch.");
                    }
                }

                _context.Schedule.Update(Schedule);
                await _context.SaveChangesAsync();

                return Schedule;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
