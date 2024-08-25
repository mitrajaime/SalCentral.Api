using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalCentral.Api.DbContext;
using SalCentral.Api.DTOs.AttendanceDTO;
using SalCentral.Api.DTOs.ScheduleDTO;
using SalCentral.Api.DTOs.UserDTO;
using SalCentral.Api.Migrations;
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
                IQueryable<ScheduleDTO> query = from q in _context.Schedule
                                                  select new ScheduleDTO()
                                                  {
                                                      ScheduleId = q.ScheduleId,
                                                      UserId = q.UserId,
                                                      FullName = _context.User.Where(u => u.UserId == q.UserId).Select(u => u.FirstName).FirstOrDefault() + ' ' 
                                                               + _context.User.Where(u => u.UserId == q.UserId).Select(u => u.LastName).FirstOrDefault(),
                                                      FirstName = _context.User.Where(u => u.UserId == q.UserId).Select(u => u.FirstName).FirstOrDefault(),
                                                      LastName = _context.User.Where(u => u.UserId == q.UserId).Select(u => u.LastName).FirstOrDefault(),
                                                      SMEmployeeID = _context.User.Where(u => u.UserId == q.UserId).Select(u => u.SMEmployeeID).FirstOrDefault(),
                                                      BranchId = q.BranchId,
                                                      Monday = q.Monday,
                                                      Tuesday = q.Tuesday,
                                                      Wednesday = q.Wednesday,
                                                      Thursday = q.Thursday,
                                                      Friday = q.Friday,
                                                      ExpectedTimeIn = q.ExpectedTimeIn,
                                                      ExpectedTimeOut = q.ExpectedTimeOut,
                                                  };

                if (query == null) throw new Exception("No Schedules found.");

                if (!string.IsNullOrWhiteSpace(scheduleFilter.FirstName))
                {
                    string SearchQuery = scheduleFilter.FirstName.Trim();
                    query = query.Where(i => i.FirstName.Contains(SearchQuery));
                }
                if (!string.IsNullOrWhiteSpace(scheduleFilter.LastName))
                {
                    string SearchQuery = scheduleFilter.LastName.Trim();
                    query = query.Where(i => i.LastName.Contains(SearchQuery));
                }
                if (!string.IsNullOrWhiteSpace(scheduleFilter.SMEmployeeID))
                {
                    string SearchQuery = scheduleFilter.SMEmployeeID.Trim();
                    query = query.Where(i => i.SMEmployeeID.Contains(SearchQuery));
                }

                var responsewrapper = await PaginationLogic.PaginateData(query, paginationRequest);
                var schedule = responsewrapper.Results;

                if (schedule.Any())
                {
                    return responsewrapper;
                }

                return null;
            } catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<object> CreateSchedule([FromBody] ScheduleDTO payload)
        {
            try
            {
                var Schedule = new Schedule()
                {
                    UserId = (Guid)payload.UserId,
                    BranchId = (Guid)payload.BranchId,
                    Monday = (bool)payload.Monday,
                    Tuesday = (bool)payload.Tuesday,
                    Wednesday = (bool)payload.Wednesday,
                    Thursday = (bool)payload.Thursday,
                    Friday = (bool)payload.Friday,
                    Saturday = (bool)payload.Saturday,
                    Sunday = (bool)payload.Sunday,
                    ExpectedTimeIn = "01/01/0001 10:00 AM",
                    ExpectedTimeOut = "01/01/0001 10:00 PM",
                };

                //Mitsubishi Photo
                if(payload.BranchId.ToString() == "cfdee1be-1b99-47a7-f410-08dcbd238cc1")
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
                //
                if (payload.BranchId.ToString() == "ee6eaf8e-bd49-480d-f411-08dcbd238cc1")
                { 
                   
                }


                var exists = _context.Schedule.Where(b => b.UserId == payload.UserId).Any();
                if (exists)
                {
                    throw new Exception("This Schedule already exists.");
                }

                await _context.Schedule.AddAsync(Schedule);
                await _context.SaveChangesAsync();
                return Schedule;

            } catch (Exception ex)
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
