using System.ComponentModel.DataAnnotations;

namespace SalCentral.Api.Pagination
{
    public class PaginationRequestQuery
    {
        public record PaginationRequest
        {
            public PaginationRequest() { }

            [Range(1, int.MaxValue)]
            public int CurrentPage { get; set; } = 1;
            public int ElementsPerPage { get; set; } = 10;
        }
    }
}
