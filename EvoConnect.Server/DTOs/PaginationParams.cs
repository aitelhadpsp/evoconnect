using System.Collections.Generic;

namespace EvoConnect.Server.DTOs
{
    /// <summary>
    /// Pagination parameters for requests
    /// </summary>
    public class PaginationParams
    {
        private const int MaxPageSize = 100;
        private int _pageSize = 20;

        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
    }

    /// <summary>
    /// Paginated response wrapper
    /// </summary>
    public class PagedResponse<T>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious { get; set; }
        public bool HasNext { get; set; }
        public List<T> Data { get; set; }

        public PagedResponse(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            PageNumber = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            HasPrevious = pageNumber > 1;
            HasNext = pageNumber < TotalPages;
            Data = items;
        }
    }

    /// <summary>
    /// VIP Patient filter parameters
    /// </summary>
    public class VipPatientFilterParams : PaginationParams
    {
        /// <summary>
        /// Filter by status: "À risque", "VIP Actif", or null for all
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Minimum total revenue filter
        /// </summary>
        public decimal? MinRevenue { get; set; }

        /// <summary>
        /// Maximum total revenue filter
        /// </summary>
        public decimal? MaxRevenue { get; set; }

        /// <summary>
        /// Sort by: "revenue_desc", "revenue_asc", "last_visit_oldest", "last_visit_newest", "name_asc", "priority"
        /// </summary>
        public string SortBy { get; set; } = "priority";

        /// <summary>
        /// Search query for name, phone, or email
        /// </summary>
        public string? SearchQuery { get; set; }
    }
}