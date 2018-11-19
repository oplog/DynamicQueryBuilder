using DynamicQueryBuilder.Models.Enums;

namespace DynamicQueryBuilder.Models
{
    public sealed class Filter
    {
        public string PropertyName { get; set; }

        public object Value { get; set; }

        public FilterOperation Operator { get; set; } = FilterOperation.Contains;

        public bool CaseSensitive { get; set; } = false;
    }
}
