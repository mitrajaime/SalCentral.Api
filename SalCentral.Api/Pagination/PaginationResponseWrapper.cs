namespace SalCentral.Api.Pagination
{
    public record PaginationResponseWrapper<T>
    {
        public int CurrentPage { get; set; }
        public int ElementsPerPage { get; set; }
        public int TotalElements { get; set; }
        public int TotalPages { get; set; }
        public IEnumerable<T>? Results { get; set; }
    }
}
