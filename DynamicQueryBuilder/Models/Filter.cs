using System.Linq;

using DynamicQueryBuilder.Models.Enums;

namespace DynamicQueryBuilder.Models
{
    public sealed class Filter
    {
        public string PropertyName { get; set; }

        public object Value { get; set; }

        public FilterOperation Operator { get; set; } = FilterOperation.Contains;

        public bool CaseSensitive { get; set; } = false;

        public LogicalOperator LogicalOperator { get; set; } = LogicalOperator.AndAlso;

        public string ToHTTPQueryString()
        {
            string valueResult = string.Empty;
            if (this.Value is DynamicQueryOptions dqbOpts)
            {
                var results = dqbOpts.Filters.Select(x => x.ToHTTPQueryString()).ToList();
                valueResult = $"({string.Join('&', results)})";
            }
            else
            {
                valueResult = (string)this.Value;
            }

            return $"o={this.Operator.ToString()}|{this.LogicalOperator.ToString()}&p={this.PropertyName}&v={valueResult}";
        }
    }
}
