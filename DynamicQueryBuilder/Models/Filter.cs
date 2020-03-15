using System;
using DynamicQueryBuilder.Models.Enums;

namespace DynamicQueryBuilder.Models
{
    public enum LogicalOperator
    {
        None = 0,
        And = 1,
        Or = 2
    }
    
    public sealed class Filter
    {
        public string PropertyName { get; set; }

        public object Value { get; set; }

        public FilterOperation Operator { get; set; } = FilterOperation.Contains;

        public bool CaseSensitive { get; set; } = false;

        public LogicalOperator LogicalOperator { get; set; }
    }
}
