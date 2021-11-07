using System.Linq.Expressions;
namespace DynamicQueryBuilder.Strategies
{
    public class GreaterThanBuilderStrategy : IFilterBuilderStrategy
    {
        public Expression Build(Expression parentMember, Expression constant)
        {
            return Expression.GreaterThan(parentMember, constant);
        }
    }
}
