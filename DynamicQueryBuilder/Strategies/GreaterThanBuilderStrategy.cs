using DynamicQueryBuilder.Models.Enums;
using System.Linq.Expressions;
namespace DynamicQueryBuilder.Strategies
{
    public class GreaterThanBuilderStrategy : IFilterBuilderStrategy
    {
        public Expression Build(Expression parentMember, Expression constant)
        {
            if (parentMember.Type.IsEnum)
            {
                return StrategyUtils.CompareEnums(FilterOperation.GreaterThan, parentMember, constant);
            }
            else if (parentMember.Type == typeof(string))
            {
                return StrategyUtils.CompareStrings(FilterOperation.GreaterThan, parentMember, constant);
            }

            return Expression.GreaterThan(parentMember, constant);
        }
    }
}
