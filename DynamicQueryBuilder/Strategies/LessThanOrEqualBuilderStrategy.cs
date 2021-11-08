using DynamicQueryBuilder.Constants;
using DynamicQueryBuilder.Models.Enums;
using System.Linq.Expressions;
namespace DynamicQueryBuilder.Strategies
{
    public class LessThanOrEqualBuilderStrategy : IFilterBuilderStrategy
    {
        public Expression Build(Expression parentMember, Expression constant)
        {
            if (parentMember.Type.IsEnum)
            {
                return StrategyUtils.CompareEnums(FilterOperation.LessThanOrEqual, parentMember, constant);
            }

            return Expression.LessThanOrEqual(parentMember, constant);
        }
    }
}
