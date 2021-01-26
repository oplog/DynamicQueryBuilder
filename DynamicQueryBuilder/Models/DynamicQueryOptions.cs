using System.Collections.Generic;
using System.Linq;
using DynamicQueryBuilder.Models.Enums;

namespace DynamicQueryBuilder.Models
{
    public sealed class DynamicQueryOptions
    {
        public List<Filter> Filters { get; set; } = new List<Filter>();

        public List<SortOption> SortOptions { get; set; } = new List<SortOption>();

        public PaginationOption PaginationOption { get; set; }

        public bool UsesCaseInsensitiveSource { get; set; }
        public bool IgnorePredefinedOrders { get; set; }

        public bool HasFilters() =>
            Filters != null && Filters.Count > 0;

        public bool HasSortOptions() =>
            SortOptions != null && SortOptions.Count > 0;

        // I know this is a duh statement but we'll keep the fashion here that we have created above.
        public bool HasPaginationOption() =>
            PaginationOption != null;
    }
}
