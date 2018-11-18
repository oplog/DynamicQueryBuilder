using System.Linq.Expressions;

namespace DynamicQueryBuilder.Models
{
    public class OrderOptionDetails
    {
        public LambdaExpression Expression { get; set; }

        public SortingDirection Direction { get; set; }

        public Expression ParameterExpression { get; set; }
    }
}
