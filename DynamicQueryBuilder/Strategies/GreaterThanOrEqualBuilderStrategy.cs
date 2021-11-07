using System.Linq.Expressions;
namespace DynamicQueryBuilder.Strategies
{
    public class GreaterThanOrEqualBuilderStrategy : IFilterBuilderStrategy
    {
        public Expression Build(Expression parentMember, Expression constant)
        {
            return Expression.GreaterThanOrEqual(parentMember, constant);
        }    }
}
