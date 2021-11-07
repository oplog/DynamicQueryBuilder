using System.Linq.Expressions;

namespace DynamicQueryBuilder.Strategies
{
    public interface IFilterBuilderStrategy
    {
        Expression Build(Expression parentMember, Expression constant);
    }
}
