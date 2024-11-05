using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalCentral.Api.DbContext;
using SalCentral.Api.DTOs;
using SalCentral.Api.DTOs.PayrollDTO;
using SalCentral.Api.DTOs.UserDTO;
using SalCentral.Api.Models;
using System.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
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

        public async Task<object> GetPayroll([FromQuery] PaginationRequest paginationRequest, [FromQuery] PayrollFilter payrollFilter)
        {
            try
            {

                IQueryable<PayrollDTO> query = from p in _context.Payroll
                                               join pd in _context.PayrollDetails on p.PayrollId equals pd.PayrollId
                                               join u in _context.User on pd.UserId equals u.UserId
                                               join b in _context.Branch on u.BranchId equals b.BranchId
                                               where u.BranchId == payrollFilter.BranchId
                                               group p by new
                                               {
                                                   p.PayrollId,
                                                   p.PayrollName,
                                                   p.TotalAmount,
                                                   p.GeneratedBy,
                                                   p.StartDate,
                                                   p.EndDate,
                                                   p.DateCreated,
                                                   b.BranchId
                                               } into g
                                               select new PayrollDTO()
                                               {
                                                   PayrollId = g.Key.PayrollId,
                                                   PayrollName = g.Key.PayrollName,
                                                   BranchId = g.Key.BranchId,
                                                   TotalAmount = g.Key.TotalAmount,
                                                   GeneratedBy = g.Key.GeneratedBy,
                                                   GeneratedByName = _context.User
                                                       .Where(u => u.UserId == g.Key.GeneratedBy)
                                                       .Select(u => u.FirstName + " " + u.LastName)
                                                       .FirstOrDefault(),
                                                   StartDate = g.Key.StartDate,
                                                   EndDate = g.Key.EndDate,
                                                   DateCreated = g.Key.DateCreated
                                               };

                if (query == null) throw new Exception("No payroll found in this branch.");

                if (!string.IsNullOrWhiteSpace(payrollFilter.PayrollName))
                {
                    string SearchQuery = payrollFilter.PayrollName.Trim();
                    query = query.Where(i => i.PayrollName.Contains(SearchQuery));
                }
                if (!string.IsNullOrWhiteSpace(payrollFilter.GeneratedByName))
                {
                    string generatedByQuery = payrollFilter.GeneratedByName.Trim();
                    query = query.Where(i => i.GeneratedByName.Contains(generatedByQuery));
                }

                var responsewrapper = await PaginationLogic.PaginateData(query, paginationRequest);
                var payroll = responsewrapper.Results;

                if (payroll.Any())
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

        public async Task<object> GetPayrollDetail([FromQuery] PaginationRequest paginationRequest, Guid PayrollId)
        {
            try
            {
                IQueryable<PayrollDetailsDTO> query = from pd in _context.PayrollDetails
                                               join p in _context.Payroll on pd.PayrollId equals p.PayrollId
                                               join u in _context.User on pd.UserId equals u.UserId
                                               join b in _context.Branch on u.BranchId equals b.BranchId
                                               where p.PayrollId == PayrollId
                                               select new PayrollDetailsDTO()
                                               {
                                                   PayrollDetailsId = pd.PayrollDetailsId,
                                                   PayrollId = pd.PayrollId,
                                                   UserId = u.UserId,
                                                   FullName = u.FirstName + ' ' + u.LastName,
                                                   BranchId = b.BranchId,
                                                   BranchName = b.BranchName,
                                                   DeductedAmount = pd.DeductedAmount,
                                                   NetPay = pd.NetPay,
                                                   HolidayPay = pd.HolidayPay,
                                                   OvertimePay = pd.OvertimePay,
                                                   GrossSalary = pd.GrossSalary,
                                                   PayDate = pd.PayDate,
                                                   IsPaid = pd.IsPaid,
                                                   TotalHoursRendered = _context.Attendance.Where(a => a.UserId == u.UserId && 
                                                                                                  a.Date >= p.StartDate && 
                                                                                                  a.Date <= p.EndDate)
                                                                                           .Select(a => a.HoursRendered)
                                                                                           .Sum(),
                                                   PagIbigContribution = pd.PagIbigContribution,
                                                   PhilHealthContribution = pd.PhilHealthContribution,
                                                   SSSContribution = pd.SSSContribution,
                                                   CurrentSalaryRate = u.SalaryRate
                                               };

                if (query == null) throw new Exception("No payroll found for this user");

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

        public async Task<Payroll> CreatePayroll([FromQuery] PayrollDTO payload)
        {
            try
            {
                var weekNumber = await (from p in _context.Payroll
                                  join pd in _context.PayrollDetails on p.PayrollId equals pd.PayrollId
                                  join u in _context.User on pd.UserId equals u.UserId
                                  join b in _context.Branch on u.BranchId equals b.BranchId
                                  where u.BranchId == payload.BranchId && p.StartDate <= (DateTime)payload.StartDate
                                  select p).CountAsync() + 1;

                if(payload.BranchId == null)
                {

                }

                // Create the payroll
                var payroll = new Payroll()
                {
                    PayrollId = Guid.NewGuid(),
                    PayrollName = $"Week {weekNumber}: {((DateTime)payload.StartDate).ToString("MMMM d yyyy")} - {((DateTime)payload.EndDate).ToString("MMMM d yyyy")}",
                    TotalAmount = 0,
                    GeneratedBy = (Guid)payload.UserId,
                    StartDate = (DateTime)payload.StartDate,
                    EndDate = (DateTime)payload.EndDate,
                    DateCreated = DateTime.Now,
                };

                await _context.Payroll.AddAsync(payroll);

                // Fetch all users with attendance within the date range and for the specified branch
                var usersWithAttendance = await _context.Attendance
                    .Where(a => a.Date >= payroll.StartDate && a.Date <= payroll.EndDate)
                    .Join(_context.User, attendance => attendance.UserId, user => user.UserId, (attendance, user) => user)
                    .Where(user => user.BranchId == payload.BranchId)
                    .Distinct()
                    .ToListAsync();

                foreach (var user in usersWithAttendance)
                {
                    var payrollDetail = new PayrollDetailsDTO
                    {
                        PayrollId = payroll.PayrollId,
                        BranchId = payload.BranchId,
                        UserId = user.UserId,
                        SSSContribution = payload.SSSContribution,
                        PhilHealthContribution = payload.PhilHealthContribution,
                        PagIbigContribution = payload.PagIbigContribution,
                        StartDate = payload.StartDate,
                        EndDate = payload.EndDate,
                        CurrentSalaryRate = user.SalaryRate,
                        holidayList = payload.holidayList
                    };

                    await CreatePayrollDetail(payrollDetail);
                }

                await _context.SaveChangesAsync();

                await CalculateTotalAmount(payroll.PayrollId);

                await _context.SaveChangesAsync();

                return payroll;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<PayrollDetails> CreatePayrollDetail(PayrollDetailsDTO payload)
        {
            try
            {
                var holidayPay = await CalculateHolidayPay(new PayrollFields
                {
                    holidayList = payload.holidayList
                });

                var overtimePay = await CalculateHolidayPay(new PayrollFields
                {
                    StartDate = (DateTime)payload.StartDate,
                    EndDate = (DateTime)payload.EndDate,
                    UserId = (Guid)payload.UserId
                    //SalaryRate = payload.CurrentSalaryRate
                });

                var payrollDetails = new PayrollDetails()
                {
                    PayrollDetailsId = new Guid(),
                    PayrollId = (Guid)payload.PayrollId,
                    UserId = (Guid)payload.UserId,
                    DeductedAmount = await CalculateDeductions(new PayrollFields
                    {
                        SSSContribution = payload.SSSContribution,
                        PagIbigContribution = payload.PagIbigContribution,
                        PhilHealthContribution = payload.PhilHealthContribution,
                        SalaryRate = payload.CurrentSalaryRate
                    }),
                    NetPay = await CalculateNetPay(new PayrollFields
                    {
                        StartDate = (DateTime)payload.StartDate,
                        EndDate = (DateTime)payload.EndDate,
                        UserId = (Guid)payload.UserId,
                        SSSContribution = payload.SSSContribution,
                        PagIbigContribution = payload.PagIbigContribution,
                        PhilHealthContribution = payload.PhilHealthContribution,
                        SalaryRate = payload.CurrentSalaryRate,
                        HolidayPay = holidayPay,
                        OvertimePay = overtimePay,
                    }),
                    GrossSalary = await CalculateGrossSalary(new PayrollFields
                    {
                        StartDate = (DateTime)payload.StartDate,
                        EndDate = (DateTime)payload.EndDate,
                        UserId = (Guid)payload.UserId,
                        SalaryRate = payload.CurrentSalaryRate
                    }),
                    HolidayPay = holidayPay,
                    OvertimePay = overtimePay,
                    IsPaid = false,
                    SSSContribution = (decimal)payload.SSSContribution,
                    PagIbigContribution = (decimal)payload.PagIbigContribution,
                    PhilHealthContribution = (decimal)payload.PhilHealthContribution
                };

                await _context.PayrollDetails.AddAsync(payrollDetails);
                return payrollDetails;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ex.StackTrace);
            }

        }

        public async Task<decimal> CalculateHolidayPay(PayrollFields payroll)
        {
            try
            {
                var totalAttendance = await _context.Attendance
                    .Where(a => a.Date >= payroll.StartDate && a.Date <= payroll.EndDate && a.UserId == payroll.UserId)
                    .ToListAsync();

                var holidayPay = totalAttendance.Count * 500;

                return holidayPay;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<decimal> CalculateDeductions(PayrollFields payroll)
        {
            try
            {
                var deductionAssignments = await _context.DeductionAssignment
                    .Where(d => d.UserId == payroll.UserId)
                    .Select(d => d.DeductionId)
                    .ToListAsync();

                var deductions = await _context.Deduction
                    .Where(d => deductionAssignments.Contains(d.DeductionId))
                    .SumAsync(d => d.Amount);

                if (!deductionAssignments.Any())
                {
                    deductions = 0;
                }

                decimal totalContributions = (decimal)(payroll.SSSContribution + payroll.PagIbigContribution + payroll.PhilHealthContribution);

                decimal totalDeductions = (decimal)deductions + totalContributions;

                return totalDeductions;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to calculate deductions: " + ex.Message);
            }

        }

        public async Task<decimal> CalculateOvertimePay(PayrollFields payroll)
        {
            try
            {
                // Retrieve all attendance records for the specified user within the date range
                var attendanceRecords = await _context.Attendance
                    .Where(a => a.Date >= payroll.StartDate && a.Date <= payroll.EndDate && a.UserId == payroll.UserId)
                    .ToListAsync();

                int totalOvertimeHours = 0;

                foreach (var attendance in attendanceRecords)
                {
                    int overtimeHours = attendance.OverTimeHours;

                    // Ensure overtime doesn't exceed allowed overtime for this record
                    if (overtimeHours > attendance.AllowedOvertimeHours)
                    {
                        overtimeHours = attendance.AllowedOvertimeHours;
                    }

                    totalOvertimeHours += overtimeHours;
                }

                if(totalOvertimeHours == 0)
                {
                    return 0;
                }

                decimal hourlyRate = (468 + (468 * 0.30M)) / 8; 
                decimal overtimePay = hourlyRate * totalOvertimeHours;

                return overtimePay;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        // has deductions
        public async Task<decimal> CalculateNetPay(PayrollFields payroll)
        {
            try
            {
                var totalHours = await _context.Attendance
                    .Where(a => a.Date >= payroll.StartDate && a.Date <= payroll.EndDate && a.UserId == payroll.UserId)
                    .SumAsync(a => a.HoursRendered);

                // for mandatory and non-mandatory deductions
                var deductions = await _context.Deduction
                    .Join(_context.DeductionAssignment,
                          d => d.DeductionId,
                          da => da.DeductionId,
                          (d, da) => new { Deduction = d, DeductionAssignment = da })
                    .Where(x => x.DeductionAssignment.UserId == payroll.UserId)
                    .ToListAsync();

                // getting the sum of mandatory and non-mandatory deductions
                var mandatoryDeductions = deductions
                    .Where(x => x.Deduction.IsMandatory == true)
                    .Sum(x => x.Deduction.Amount);

                var nonMandatoryDeductions = deductions
                    .Where(x => x.Deduction.IsMandatory != true &&
                                x.Deduction.Date >= payroll.StartDate &&
                                x.Deduction.Date <= payroll.EndDate)
                    .Sum(x => x.Deduction.Amount);

                var totalDeductions = mandatoryDeductions + nonMandatoryDeductions;

                decimal totalContributions = (decimal)(payroll.SSSContribution + payroll.PagIbigContribution + payroll.PhilHealthContribution);
                decimal grossPay = (decimal)(totalHours * payroll.SalaryRate);
                decimal netPay = grossPay - totalDeductions - totalContributions + (decimal)payroll.HolidayPay + (decimal)payroll.OvertimePay;

                return netPay;

            }
            catch (Exception ex)
            {
                throw new Exception("Failed to compute for net pay: " + ex.Message);
            }
        }


        // no deductions
        public async Task<decimal> CalculateGrossSalary(PayrollFields payroll)
        {
            try
            {
                var totalHours = await _context.Attendance
                    .Where(a => a.Date >= payroll.StartDate && a.Date <= payroll.EndDate && a.UserId == payroll.UserId)
                    .SumAsync(a => a.HoursRendered);

                decimal grossSalary = (decimal)(totalHours * payroll.SalaryRate);

                return grossSalary;

            } catch (Exception ex)
            {
                throw new Exception("Failed to compute for gross salary: " + ex.Message);
            }
            
        }

        public async Task<decimal> CalculateTotalAmount(Guid PayrollId)
        {
            try
            {
                var payrollDetailsAmount = await _context.PayrollDetails
                    .Where(p => p.PayrollId == PayrollId)
                    .Select(p => p.NetPay)
                    .SumAsync();

                var payroll = await _context.Payroll.Where(p => p.PayrollId == PayrollId).FirstOrDefaultAsync();

                if (payroll == null)
                {
                    throw new Exception("Payroll not found");
                }

                payroll.TotalAmount = (decimal)payrollDetailsAmount;

                _context.Payroll.Update(payroll);
                await _context.SaveChangesAsync();

                return payroll.TotalAmount;

            } catch (Exception ex) 
            {
                throw new Exception("Failed to compute for total amount: " + ex.Message);
            }
            
        }
    }
}
