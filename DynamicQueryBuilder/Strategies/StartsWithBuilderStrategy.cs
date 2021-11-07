using DynamicQueryBuilder.Extensions;
using System.Linq.Expressions;
namespace DynamicQueryBuilder.Strategies
{
    public class StartsWithBuilderStrategy : IFilterBuilderStrategy
    {
        public Expression Build(Expression parentMember, Expression constant)
        {
            return Expression.Call(parentMember, ExtensionMethods.StringStartsWithMethod, constant);
        }
    }
}
