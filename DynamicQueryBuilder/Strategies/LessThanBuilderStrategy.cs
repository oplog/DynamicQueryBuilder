using System.Linq.Expressions;
namespace DynamicQueryBuilder.Strategies
{
    public class LessThanBuilderStrategy : IFilterBuilderStrategy
    {
        public Expression Build(Expression parentMember, Expression constant)
        {
            return Expression.LessThan(parentMember, constant);
        }
    }
}
