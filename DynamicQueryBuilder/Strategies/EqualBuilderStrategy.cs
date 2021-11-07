using System.Linq.Expressions;

namespace DynamicQueryBuilder.Strategies
{
    public class EqualBuilderStrategy : IFilterBuilderStrategy
    {
        public Expression Build(Expression parentMember, Expression constant)
        {
            return Expression.Equal(parentMember, constant);
        }
    }
}
