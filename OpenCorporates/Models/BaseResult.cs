namespace OpenCorporates.Models
{
    public abstract class BaseResult
    {
        public int Page { get; set; }
        public int PerPage { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}
