using Microsoft.EntityFrameworkCore;
using SalCentral.Api.Pagination;
using static SalCentral.Api.Pagination.PaginationRequestQuery;

namespace SalCentral.Api.Logics
{
    public class PaginationLogic
    {
        public static async Task<PaginationResponseWrapper<T>> PaginateData<T>(IQueryable<T> query, PaginationRequest paginationRequest)
        {   
            var totalElements = await query.CountAsync();
            var totalPages = (totalElements + paginationRequest.ElementsPerPage - 1) / paginationRequest.ElementsPerPage;

            var response = new PaginationResponseWrapper<T>()
            {
                CurrentPage = paginationRequest.CurrentPage,
                ElementsPerPage = paginationRequest.ElementsPerPage,
                TotalElements = totalElements,
                TotalPages = totalPages,
                Results = await query.Skip((paginationRequest.CurrentPage - 1) * paginationRequest.ElementsPerPage).Take(paginationRequest.ElementsPerPage).ToListAsync()
            };

            return response;
        }
    }
}
