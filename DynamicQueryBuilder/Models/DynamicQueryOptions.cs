using System.Collections.Generic;

namespace DynamicQueryBuilder.Models
{
    public sealed class DynamicQueryOptions
    {
        public List<Filter> Filters { get; set; } = new List<Filter>();

        public List<SortOption> SortOptions { get; set; } = new List<SortOption>();

        public PaginationOption PaginationOption { get; set; }

        public bool UsesCaseInsensitiveSource { get; set; }

        public bool IsNullValueString { get; set; }

        public bool IgnorePredefinedOrders { get; set; }
    }
}
