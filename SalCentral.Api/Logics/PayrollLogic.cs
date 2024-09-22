using Microsoft.AspNetCore.Mvc;
using SalCentral.Api.DbContext;
using SalCentral.Api.DTOs;
using SalCentral.Api.Models;
using static SalCentral.Api.Pagination.PaginationRequestQuery;


namespace SalCentral.Api.Logics
{
    public class PayrollLogic
    {
        private readonly ApiDbContext _context;
        private readonly PayrollDetails _payrollDetailsLogic;

        public PayrollLogic(ApiDbContext context, PayrollDetails payrollDetails)
        {
            _context = context;
            _payrollDetailsLogic = payrollDetails;
        }

        public async Task<object> GetPayroll([FromQuery] PaginationRequest paginationRequest, Guid BranchId)
        {
            try
            {

                IQueryable<PayrollDTO> query = from p in _context.Payroll
                                               where u.BranchId == BranchId
                                               join pd in _context.PayrollDetails on p.PayrollId equals pd.PayrollId
                                               join u in _context.User on pd.UserId equals u.UserId
                                               join b in _context.Branch on u.BranchId equals b.BranchId
                                               select new PayrollDTO()
                                               {
                                                   BranchId = b.BranchId,
                                                   TotalAmount = b.TotalAmount,
                                                   GeneratedBy = b.GeneratedBy,
                                                   StartDate = b.StartDate,
                                                   EndDate = b.EndDate,
                                                   IsPaid = b.IsPaid,
                 
                                               };

                if (query == null) throw new Exception("No payroll found in this branch.");

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


        public async Task<Payroll> CreatePayroll([FromBody] PayrollDTO payload)
        {

            //Initial Create of Payroll
            var payroll = new Payroll()
            {
                PayrollId = new Guid(),
                TotalAmount = 0,
                GeneratedBy = (Guid)payload.UserId,
                StartDate = (DateTime)payload.StartDate,
                EndDate= (DateTime)payload.EndDate,
                IsPaid = (bool)payload.IsPaid,
            };



            //var exists = _context.User.Where(u => u.SMEmployeeID == payload.SMEmployeeID).Any();

            //if (exists)
            //{
            //    throw new Exception("The user provided already exists.");
            //}

            await _context.Payroll.AddAsync(payroll);

            return payroll;
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
