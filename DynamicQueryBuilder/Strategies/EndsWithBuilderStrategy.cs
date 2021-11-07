using DynamicQueryBuilder.Extensions;
using System.Linq.Expressions;
namespace DynamicQueryBuilder.Strategies
{
    public class EndsWithBuilderStrategy : IFilterBuilderStrategy
    {
        public Expression Build(Expression parentMember, Expression constant)
        {
            return Expression.Call(parentMember, ExtensionMethods.StringEndsWithMethod, constant);
        }
    }
}
