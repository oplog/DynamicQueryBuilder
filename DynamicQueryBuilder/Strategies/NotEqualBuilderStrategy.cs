using System.Linq.Expressions;

namespace DynamicQueryBuilder.Strategies
{
    public class NotEqualBuilderStrategy : IFilterBuilderStrategy
    {
        public Expression Build(Expression parentMember, Expression constant)
        {
            return Expression.NotEqual(parentMember, constant);
        }
    }
}
