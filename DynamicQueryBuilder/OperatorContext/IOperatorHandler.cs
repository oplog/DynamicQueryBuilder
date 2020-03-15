using System.Linq.Expressions;
using DynamicQueryBuilder.Models;

namespace DynamicQueryBuilder.OperatorContext
{
    internal interface IOperatorHandler
    {
        Expression Build(ParameterExpression param, Filter filter, bool usesCaseInsensitiveSource = false);    
    }
}