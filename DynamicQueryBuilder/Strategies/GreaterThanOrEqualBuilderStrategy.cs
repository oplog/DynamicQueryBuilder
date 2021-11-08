using DynamicQueryBuilder.Models.Enums;
using System.Linq.Expressions;
namespace DynamicQueryBuilder.Strategies
{
    public class GreaterThanOrEqualBuilderStrategy : IFilterBuilderStrategy
    {
        public Expression Build(Expression parentMember, Expression constant)
        {
            if (parentMember.Type.IsEnum)
            {
                return StrategyUtils.CompareEnums(FilterOperation.GreaterThanOrEqual, parentMember, constant);
            }

            return Expression.GreaterThanOrEqual(parentMember, constant);
        }
    }
}
