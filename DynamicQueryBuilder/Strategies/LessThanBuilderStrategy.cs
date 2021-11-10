using DynamicQueryBuilder.Models.Enums;
using System.Linq.Expressions;
namespace DynamicQueryBuilder.Strategies
{
    public class LessThanBuilderStrategy : IFilterBuilderStrategy
    {
        public Expression Build(Expression parentMember, Expression constant)
        {
            if (parentMember.Type.IsEnum)
            {
                return StrategyUtils.CompareEnums(FilterOperation.LessThan, parentMember, constant);
            }
            else if (parentMember.Type == typeof(string))
            {
                return StrategyUtils.CompareStrings(FilterOperation.LessThan, parentMember, constant);
            }

            return Expression.LessThan(parentMember, constant);
        }
    }
}
