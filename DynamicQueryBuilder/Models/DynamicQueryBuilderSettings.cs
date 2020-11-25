using DynamicQueryBuilder.Interfaces;

namespace DynamicQueryBuilder.Models
{
    /// <summary>
    /// Configures DQB to perform under these settings.
    /// </summary>
    public class DynamicQueryBuilderSettings
    {
        /// <summary>
        /// Definitions for custom operation codes.
        /// </summary>
        public CustomOpCodes CustomOpCodes { get; set; } = null;

        /// <summary>
        /// Boolean flag for case sensitivity management. If set true, DQB will not try to handle case insensitive queries.
        /// </summary>
        public bool UsesCaseInsensitiveSource { get; set; }

        /// <summary>
        /// Boolean flag to specify to whether if DQB should ignore orderby statements if there are any with the given queryable. 
        /// If set true, DQB will override all Orders by itself. If left false, DQB will add any given SortOption as ThenBy expression calls.
        /// </summary>
        public bool IgnorePredefinedOrders { get; set; }

        /// <summary>
        /// Boolean flag to specify if null value should be used as string or sql null keyword.
        /// If set true, DQB will use null value as string. If left false, DQB will use null value as sql keyword.
        /// </summary>
        public bool IsNullValueString { get; set; } = false;

        /// <summary>
        /// Query options resolver to specify where DQB should be getting query options from.
        /// If left null, DQB will try to resolve options from the querystring.
        /// </summary>
        public IQueryResolver QueryOptionsResolver { get; set; }
    }
}
