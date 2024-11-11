using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalCentral.Api.DbContext;
using SalCentral.Api.DTOs;
using SalCentral.Api.DTOs.PayrollDTO;
using SalCentral.Api.DTOs.UserDTO;
using SalCentral.Api.Migrations;
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

                if (!string.IsNullOrWhiteSpace(payrollFilter.SearchKeyword))
                {
                    string searchQuery = payrollFilter.SearchKeyword.Trim().ToLower();
                    query = query.Where(i => i.PayrollName.ToLower().Contains(searchQuery)
                                          || i.GeneratedByName.ToLower().Contains(searchQuery));
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
                                                   Tax = pd.Tax,
                                                   PagIbigContribution = pd.PagIbigContribution,
                                                   PhilHealthContribution = pd.PhilHealthContribution,
                                                   SSSContribution = pd.SSSContribution,
                                                   CurrentSalaryRate = pd.CurrentSalaryRate
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

        public async Task<object> EditPayrollDetail(Guid PayrollDetailsId)
        {
            try
            {
                var payrollDetail = await _context.PayrollDetails.Where(p => p.PayrollDetailsId == PayrollDetailsId).FirstOrDefaultAsync();

                payrollDetail.IsPaid = !payrollDetail.IsPaid;

                _context.PayrollDetails.Update(payrollDetail);
                _context.SaveChangesAsync();

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
                                  select p).CountAsync();

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

        public async Task<decimal> CalculateTaxableIncome(PayrollFields payroll)
        {
            try
            {
                var hasTIN = await _context.User.Where(u => u.UserId == payroll.UserId).Select(u => u.TIN).FirstOrDefaultAsync();
                if (hasTIN == null) { return 0; }

                decimal tax = 0;
                var salaryRate = await _context.User.Where(u => u.UserId == payroll.UserId).Select(u => u.SalaryRate).FirstOrDefaultAsync();
                decimal annualTaxableIncome = salaryRate * 8 * 6 * 4 * 12;

                if (annualTaxableIncome < 250000)
                {
                    // No tax for income below 250,000
                    tax = 0;
                    return tax;
                }
                else if (annualTaxableIncome >= 250000 && annualTaxableIncome < 400000)
                {
                    tax = 0.15M * (annualTaxableIncome - 250000);
                    tax = (tax / 12 )/ 4;
                    return tax;
                }
                else if (annualTaxableIncome >= 400000 && annualTaxableIncome < 800000)
                {
                    tax = 22500 + (0.20M * (annualTaxableIncome - 400000));
                    tax = (tax / 12) / 4;
                    return tax;
                }
                else if (annualTaxableIncome >= 800000 && annualTaxableIncome < 2000000)
                {
                    tax = 102500 + (0.25M * (annualTaxableIncome - 800000));
                    tax = (tax / 12) / 4;
                    return tax;
                }
                else if (annualTaxableIncome >= 2000000 && annualTaxableIncome < 8000000)
                {
                    tax = 402500 + (0.30M * (annualTaxableIncome - 2000000));
                    tax = (tax / 12) / 4;
                    return tax;
                }
                else if (annualTaxableIncome >= 8000000)
                {
                    tax = 2202500 + (0.35M * (annualTaxableIncome - 8000000));
                    tax = (tax / 12) / 4;
                    return tax;
                }
                return tax;
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
                var tax = await CalculateTaxableIncome(new PayrollFields
                {
                    UserId = payload.UserId,
                });

                var holidayPay = await CalculateHolidayPay(new PayrollFields
                {
                    holidayList = payload.holidayList,
                    StartDate = (DateTime)payload.StartDate,
                    EndDate = (DateTime)payload.EndDate,
                    UserId = payload.UserId,
                });

                var overtimePay = await CalculateOvertimePay(new PayrollFields
                {
                    StartDate = (DateTime)payload.StartDate,
                    EndDate = (DateTime)payload.EndDate,
                    UserId = (Guid)payload.UserId
                    //SalaryRate = payload.CurrentSalaryRate
                });

                var totalDeductions = await CalculateTotalDeductions(new PayrollFields
                {
                    StartDate = (DateTime)payload.StartDate,
                    EndDate = (DateTime)payload.EndDate,
                    UserId = (Guid)payload.UserId
                });

                var deductions = await _context.Deduction
                    .Join(_context.DeductionAssignment,
                          d => d.DeductionId,
                          da => da.DeductionId,
                          (d, da) => new { Deduction = d, DeductionAssignment = da })
                    .Where(x => x.DeductionAssignment.UserId == payload.UserId)
                    .ToListAsync();

                // Getting the sum of mandatory and non-mandatory deductions of the user
                var mandatoryDeductions = deductions
                    .Where(x => x.Deduction.IsMandatory == true);

                var payrollDetails = new PayrollDetails()
                {
                    PayrollDetailsId = new Guid(),
                    PayrollId = (Guid)payload.PayrollId,
                    UserId = (Guid)payload.UserId,
                    DeductedAmount = totalDeductions,
                    NetPay = await CalculateNetPay(new PayrollFields
                    {
                        StartDate = (DateTime)payload.StartDate,
                        EndDate = (DateTime)payload.EndDate,
                        UserId = (Guid)payload.UserId,
                        SalaryRate = payload.CurrentSalaryRate,
                        HolidayPay = holidayPay,
                        TotalDeductions = totalDeductions,
                        OvertimePay = overtimePay,
                    }),
                    GrossSalary = await CalculateGrossSalary(new PayrollFields
                    {
                        StartDate = (DateTime)payload.StartDate,
                        EndDate = (DateTime)payload.EndDate,
                        UserId = (Guid)payload.UserId,
                        SalaryRate = payload.CurrentSalaryRate
                    }),
                    CurrentSalaryRate = (decimal)payload.CurrentSalaryRate,
                    HolidayPay = holidayPay,
                    OvertimePay = overtimePay,
                    Tax = tax,
                    IsPaid = false,
                    SSSContribution = (decimal)mandatoryDeductions
                                        .Where(d => d.Deduction.DeductionName == "SSS").Select(d => d.Deduction.Amount).FirstOrDefault(),
                    PagIbigContribution = (decimal)mandatoryDeductions
                                        .Where(d => d.Deduction.DeductionName == "Pagibig").Select(d => d.Deduction.Amount).FirstOrDefault(),
                    PhilHealthContribution = (decimal)mandatoryDeductions
                                        .Where(d => d.Deduction.DeductionName == "PhilHealth").Select(d => d.Deduction.Amount).FirstOrDefault(),
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
                    .Where(a => a.Date.Date >= payroll.StartDate && a.Date.Date <= payroll.EndDate && a.UserId == payroll.UserId && payroll.holidayList.Select(d => d.Date.Date).Contains(a.Date.Date))
                    .ToListAsync();

                var holidayPay = totalAttendance.Count * 500;

                return holidayPay;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<decimal> CalculateTotalDeductions(PayrollFields payroll)
        {
            try
            {
                // Get all deductions associated with the user
                var deductions = await _context.Deduction
                     .Join(_context.DeductionAssignment,
                           d => d.DeductionId,
                           da => da.DeductionId,
                           (d, da) => new { Deduction = d, DeductionAssignment = da })
                     .Where(x => x.DeductionAssignment.UserId == payroll.UserId)
                     .ToListAsync();

                // Sum up the amounts of mandatory deductions
                var mandatoryDeductions = deductions
                    .Where(x => x.Deduction.IsMandatory == true)
                    .Sum(x => x.Deduction.Amount);

                // Sum up the amounts of non-mandatory deductions within the specified date range
                var nonMandatoryDeductions = deductions
                    .Where(x => x.Deduction.IsMandatory != true &&
                                x.Deduction.Date.Date >= payroll.StartDate &&
                                x.Deduction.Date.Date <= payroll.EndDate && x.Deduction.Type != null)
                    .Sum(x => x.Deduction.Amount);

                // Calculate total deductions
                var totalDeductions = mandatoryDeductions + nonMandatoryDeductions;

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

                decimal grossPay = (decimal)(totalHours * payroll.SalaryRate);
                decimal netPay = grossPay - (decimal)payroll.TotalDeductions + (decimal)payroll.HolidayPay + (decimal)payroll.OvertimePay;

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

            } 
            catch (Exception ex) 
            {
                throw new Exception("Failed to compute for total amount: " + ex.Message);
            }
        }

        public async Task<decimal> GenerateServicesDeductionAmount(PayrollFields payroll)
        {
            // for mandatory and non-mandatory deductions of user
            var deductions = await _context.Deduction
                .Join(_context.DeductionAssignment,
                      d => d.DeductionId,
                      da => da.DeductionId,
                      (d, da) => new { Deduction = d, DeductionAssignment = da })
                .Where(x => x.DeductionAssignment.UserId == payroll.UserId && x.Deduction.IsDeleted == false)
                .ToListAsync();

            // getting the sum of non-mandatory deductions (under the type 'Service') of user
            var nonMandatoryDeductions = deductions
                .Where(x => x.Deduction.IsMandatory != true &&
                            x.Deduction.Type == "Service" &&
                            x.Deduction.Date >= payroll.StartDate &&
                            x.Deduction.Date <= payroll.EndDate)
                .Sum(x => x.Deduction.Amount);

            return nonMandatoryDeductions;
        }

        public async Task<decimal> GenerateProductsDeductionAmount(PayrollFields payroll)
        {
            // for mandatory and non-mandatory deductions of user
            var deductions = await _context.Deduction
                .Join(_context.DeductionAssignment,
                      d => d.DeductionId,
                      da => da.DeductionId,
                      (d, da) => new { Deduction = d, DeductionAssignment = da })
                .Where(x => x.DeductionAssignment.UserId == payroll.UserId && x.Deduction.IsDeleted == false)
                .ToListAsync();

            // getting the sum of non-mandatory deductions (under the type 'Service') of user
            var nonMandatoryDeductions = deductions
                .Where(x => x.Deduction.IsMandatory != true &&
                            x.Deduction.Type == "Product" &&
                            x.Deduction.Date >= payroll.StartDate &&
                            x.Deduction.Date <= payroll.EndDate)
                .Sum(x => x.Deduction.Amount);

            return nonMandatoryDeductions;
        }

        public async Task<object> GeneratePayslip(Guid UserId, Guid PayrollId)
        {
            try
            {
                var userFields = await (from u in _context.User
                                        join b in _context.Branch on u.BranchId equals b.BranchId
                                        join pd in _context.PayrollDetails on u.UserId equals pd.UserId
                                        join p in _context.Payroll on pd.PayrollId equals p.PayrollId
                                        join r in _context.Role on u.RoleId equals r.RoleId
                                        where u.UserId == UserId && p.PayrollId == PayrollId && pd.PayrollId == PayrollId
                                        select new
                                        {
                                            BranchName = b.BranchName,
                                            FullName = u.FirstName + " " + u.LastName,
                                            RoleName = r.RoleName,
                                            Address = b.Address,
                                            CurrentSalaryRate = pd.CurrentSalaryRate,
                                            OvertimePay = pd.OvertimePay,
                                            Holiday = pd.HolidayPay,
                                            GrossSalary = pd.GrossSalary,
                                            PayrollName = p.PayrollName,
                                            Tax = pd.Tax,
                                            SMEmployeeId = u.SMEmployeeID,
                                            NetSalary = pd.NetPay,
                                            ClaimedBy = u.FirstName + " " + u.LastName,
                                            PreparedBy = _context.User
                                                        .Where(u => u.UserId == p.GeneratedBy)
                                                        .Select(u => u.FirstName + " " + u.LastName)
                                                        .FirstOrDefault(),
                                            TotalDeductionAmount = pd.DeductedAmount,
                                            StartDate = p.StartDate,
                                            EndDate = p.EndDate,
                                        }).FirstOrDefaultAsync();

                if (userFields == null)
                {
                    throw new Exception("User or payroll details not found.");
                }

                var deductions = await _context.Deduction
                    .Join(_context.DeductionAssignment,
                          d => d.DeductionId,
                          da => da.DeductionId,
                          (d, da) => new { Deduction = d, DeductionAssignment = da })
                    .Where(x => x.DeductionAssignment.UserId == UserId)
                    .ToListAsync();

                // Getting the sum of mandatory and non-mandatory deductions of the user
                var mandatoryDeductions = deductions
                    .Where(x => x.Deduction.IsMandatory == true);

                var payslip = new
                {
                    userFields,
                    SSS = mandatoryDeductions.Where(d => d.Deduction.DeductionName == "SSS").Select(d => d.Deduction.Amount).FirstOrDefault(),
                    Pagibig = mandatoryDeductions.Where(d => d.Deduction.DeductionName == "Pagibig").Select(d => d.Deduction.Amount).FirstOrDefault(),
                    PhilHealth = mandatoryDeductions.Where(d => d.Deduction.DeductionName == "PhilHealth").Select(d => d.Deduction.Amount).FirstOrDefault(),
                    ServicesDeductionAmount = await GenerateServicesDeductionAmount(new PayrollFields
                    {
                        UserId = UserId,
                        StartDate = userFields.StartDate,
                        EndDate = userFields.EndDate,
                    }),
                    ProductsDeductionAmount = await GenerateProductsDeductionAmount(new PayrollFields
                    {
                        UserId = UserId,
                        StartDate = userFields.StartDate,
                        EndDate = userFields.EndDate,
                    })
                };

                return payslip;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to generate payslip: " + ex.Message);
            }
        }

    }
}
