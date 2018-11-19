namespace DynamicQueryBuilder.Models
{
    public class DynamicQueryBuilderSettings
    {
        public CustomOpCodes CustomOpCodes { get; set; } = null;

        public bool UsesCaseInsensitiveSource { get; set; }

        public bool IgnorePredefinedOrders { get; set; }
    }
}
