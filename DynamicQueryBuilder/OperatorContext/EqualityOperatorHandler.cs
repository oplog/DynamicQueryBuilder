using System.Linq.Expressions;
using DynamicQueryBuilder.Models;
using DynamicQueryBuilder.Models.Enums;
using static DynamicQueryBuilder.DynamicQueryBuilderExceptions;

namespace DynamicQueryBuilder.OperatorContext
{
    internal class EqualityOperatorHandler : IOperatorHandler
    {
        public Expression Build(ParameterExpression param, Filter filter, bool usesCaseInsensitiveSource = false)
        {
            if (filter.Operator != FilterOperation.Equals
             || filter.Operator != FilterOperation.NotEqual)
            {
                throw new DynamicQueryException(@$"Invalid filter type ({filter.Operator.ToString()}) to EqualityOperatorHandler");
            }

            return filter.Operator == FilterOperation.Equals
                ? Expression.Equal(parentMember, Expression.Constant(convertedValue))
                : Expression.NotEqual(parentMember, Expression.Constant(convertedValue));
        }
    }
}