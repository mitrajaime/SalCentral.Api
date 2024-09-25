using Microsoft.AspNetCore.Mvc;
using SalCentral.Api.DbContext;
using SalCentral.Api.DTOs;
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

        public async Task<object> GetPayroll([FromQuery] PaginationRequest paginationRequest, Guid BranchId)
        {
            try
            {

                IQueryable<PayrollDTO> query = from p in _context.Payroll
                                               join pd in _context.PayrollDetails on p.PayrollId equals pd.PayrollId
                                               join u in _context.User on pd.UserId equals u.UserId
                                               join b in _context.Branch on u.BranchId equals b.BranchId
                                               where u.BranchId == BranchId
                                               select new PayrollDTO()
                                               {
                                                   PayrollId = p.PayrollId,
                                                   BranchId = b.BranchId,
                                                   TotalAmount = p.TotalAmount,
                                                   GeneratedBy = p.GeneratedBy,
                                                   GeneratedByName = _context.User.Where(u => u.UserId == p.GeneratedBy).Select(u => u.FirstName).FirstOrDefault() + ' ' + _context.User.Where(u => u.UserId == p.GeneratedBy).Select(u => u.LastName).FirstOrDefault(),
                                                   StartDate = p.StartDate,
                                                   EndDate = p.EndDate,
                                                   IsPaid = p.IsPaid,
                                               };

                if (query == null) throw new Exception("No payroll found in this branch.");

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

        public async Task<object> GetPayrollDetail([FromQuery] PaginationRequest paginationRequest, Guid BranchId, Guid UserId)
        {
            try
            {
                IQueryable<PayrollDetailsDTO> query = from pd in _context.PayrollDetails
                                               join p in _context.Payroll on pd.PayrollId equals p.PayrollId
                                               join u in _context.User on pd.UserId equals u.UserId
                                               join b in _context.Branch on u.BranchId equals b.BranchId
                                               where u.BranchId == BranchId && u.UserId == UserId
                                               select new PayrollDetailsDTO()
                                               {
                                                   PayrollDetailsId = pd.PayrollDetailsId,
                                                   UserId = u.UserId,
                                                   BranchId = b.BranchId,
                                                   BranchName = b.BranchName,
                                                   DeductedAmount = pd.DeductedAmount,
                                                   NetPay = pd.NetPay,
                                                   GrossSalary = pd.GrossSalary,
                                                   PayDate = pd.PayDate,
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
                var weekNumber = (from p in _context.Payroll
                                  join pd in _context.PayrollDetails on p.PayrollId equals pd.PayrollId
                                  join u in _context.User on pd.UserId equals u.UserId
                                  join b in _context.Branch on u.BranchId equals b.BranchId
                                  where u.BranchId == payload.BranchId && p.StartDate <= (DateTime)payload.StartDate
                                  select p).Count() + 1;
                //Initial Create of Payroll
                var payroll = new Payroll()
                {
                    PayrollId = new Guid(),
                    PayrollName = $"Week {weekNumber}: {((DateTime)payload.StartDate).ToString("MMMM d yyyy")} - {((DateTime)payload.EndDate).ToString("MMMM d yyyy")}",
                    TotalAmount = 0,
                    GeneratedBy = (Guid)payload.UserId,
                    StartDate = (DateTime)payload.StartDate,
                    EndDate = (DateTime)payload.EndDate,
                    IsPaid = (bool)payload.IsPaid,
                    DateCreated = DateTime.Now,
                };

                // how do i separate this by branch?

                //var exists = _context.Payroll.Where(u => u.StartDate == payload.StartDate && u.EndDate == payload.EndDate).Any();

                //if (exists)
                //{
                //    throw new Exception("There is already a payroll created within the given dates.");
                //}

                await _context.Payroll.AddAsync(payroll);

                foreach (var payrollDetail in payload.PayrollDetailsList)
                {
                    payrollDetail.PayrollId = payroll.PayrollId;
                    payrollDetail.BranchId = payload.BranchId;
                    payrollDetail.SSSContribution = payload.SSSContribution;
                    payrollDetail.PhilHealthContribution = payload.PhilHealthContribution;
                    payrollDetail.PagIbigContribution = payload.PagIbigContribution;
                    payrollDetail.StartDate = payload.StartDate;
                    payrollDetail.EndDate = payload.EndDate;
                    CreatePayrollDetail(payrollDetail);
                }
                
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
                var payrollDetails = new PayrollDetails()
                {
                    PayrollDetailsId = new Guid(),
                    PayrollId = (Guid)payload.PayrollId,
                    UserId = (Guid)payload.UserId,
                    DeductedAmount = CalculateDeductions(new PayrollFields
                    {
                        SSSContribution = payload.SSSContribution,
                        PagIbigContribution = payload.PagIbigContribution,
                        PhilHealthContribution = payload.PhilHealthContribution
                    }),
                    NetPay = CalculateNetPay(new PayrollFields
                    {
                        StartDate = (DateTime)payload.StartDate,
                        EndDate = (DateTime)payload.EndDate,
                        UserId = (Guid)payload.UserId,
                        SSSContribution = payload.SSSContribution,
                        PagIbigContribution = payload.PagIbigContribution,
                        PhilHealthContribution = payload.PhilHealthContribution
                    }),
                    GrossSalary = CalculateGrossSalary(new PayrollFields
                    {
                        StartDate = (DateTime)payload.StartDate,
                        EndDate = (DateTime)payload.EndDate,
                        UserId = (Guid)payload.UserId,
                    }),
                    PayDate = (DateTime)payload.PayDate,
                    SSSContribution = (decimal)payload.SSSContribution,
                    PagIbigContribution = (decimal)payload.PagIbigContribution,
                    PhilHealthContribution = (decimal)payload.PhilHealthContribution
                };

                _context.PayrollDetails.Add(payrollDetails);
                return payrollDetails;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ex.StackTrace);
            }

        }

        public decimal CalculateDeductions(PayrollFields payroll)
        {
            var deductionAssignments = _context.DeductionAssignment
                .Where(d => d.UserId == payroll.UserId)
                .Select(d => d.DeductionId)
                .ToList();

            var deductions = _context.Deduction
                .Where(d => deductionAssignments.Contains(d.DeductionId))
                .Sum(d => d.Amount);

            if (!deductionAssignments.Any())
            {
                deductions = 0;
            }


            decimal totalContributions = (decimal)(payroll.SSSContribution + payroll.PagIbigContribution + payroll.PhilHealthContribution);

            decimal totalDeductions = (decimal)deductions + totalContributions;

            return totalDeductions;
        }

        // has deductions
        public decimal CalculateNetPay(PayrollFields payroll)
        {
            
            var totalHours = _context.Attendance
                .Where(a => a.Date >= payroll.StartDate && a.Date <= payroll.EndDate && a.UserId == payroll.UserId)
                .Sum(a => a.HoursRendered);

            var deductionAssignments = _context.DeductionAssignment
                .Where(d => d.UserId == payroll.UserId)
                .Select(d => d.DeductionId)
                .ToList();

            var deductions = _context.Deduction
                .Where(d => deductionAssignments.Contains(d.DeductionId))
                .ToList();

            var totalDeductions = deductions.Sum(d => d.Amount);

            if (!deductionAssignments.Any())
            {
                totalDeductions = 0;
            }

            decimal totalContributions = (decimal)(payroll.SSSContribution + payroll.PagIbigContribution + payroll.PhilHealthContribution);

            // 8 hours = P468; 58.5 per hour; 

            decimal grossPay = (decimal)(totalHours * 58.5);

            decimal netPay = grossPay - (decimal)totalDeductions - totalContributions;

            return netPay;
        }

        // no deductions
        public decimal CalculateGrossSalary(PayrollFields payroll)
        {
            
            var totalHours = _context.Attendance
                .Where(a => a.Date >= payroll.StartDate && a.Date <= payroll.EndDate && a.UserId == payroll.UserId)
                .Sum(a => a.HoursRendered);

            // 8 hours = P468; 58.5 per hour; 

            decimal grossSalary = (decimal)(totalHours * 58.5);

            return grossSalary;
        }

        //public async Task<User> EditUser([FromBody] UserDTO payload)
        //{
        //    try
        //    {
        //        var user = await _context.User.Where(u => u.UserId == payload.UserId).FirstOrDefaultAsync();

        //        user.FirstName = payload.FirstName;
        //        user.LastName = payload.LastName;
        //        user.Email = payload.Email;
        //        user.ContactNo = payload.ContactNo;
        //        user.Password = HashingLogic.HashData(payload.Password);
        //        user.RoleId = (Guid)payload.RoleId;

        //        _context.User.Update(user);
        //        await _context.SaveChangesAsync();

        //        return user;

        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}
    }
}
