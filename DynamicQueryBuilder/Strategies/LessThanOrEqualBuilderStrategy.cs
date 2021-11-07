using System.Linq.Expressions;
namespace DynamicQueryBuilder.Strategies
{
    public class LessThanOrEqualBuilderStrategy : IFilterBuilderStrategy
    {
        public Expression Build(Expression parentMember, Expression constant)
        {
            return Expression.LessThanOrEqual(parentMember, constant);
        }    }
}
