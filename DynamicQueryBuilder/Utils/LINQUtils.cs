using DynamicQueryBuilder.Models;
using DynamicQueryBuilder.Models.Enums;
using DynamicQueryBuilder.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DynamicQueryBuilder.Utils
{
    public static class LINQUtils
    {
        private static Dictionary<LogicalOperator, Func<Expression, Expression, Expression>> _logicalOperatorToExpressionMap = new Dictionary<LogicalOperator, Func<Expression, Expression, Expression>>()
        {
            { LogicalOperator.AndAlso, (exp, builtExpression) => Expression.AndAlso(exp, builtExpression) },
            { LogicalOperator.OrElse, (exp, builtExpression) => Expression.OrElse(exp, builtExpression) },
            { LogicalOperator.And, (exp, builtExpression) => Expression.And(exp, builtExpression) },
            { LogicalOperator.Or, (exp, builtExpression) => Expression.Or(exp, builtExpression) },
            { LogicalOperator.Xor, (exp, builtExpression) => Expression.ExclusiveOr(exp, builtExpression) },
        };

        private static Dictionary<FilterOperation, IFilterBuilderStrategy> _filterOperatorToExpressionMap = new Dictionary<FilterOperation, IFilterBuilderStrategy>()
        {
            { FilterOperation.Equals, new EqualBuilderStrategy() },
            { FilterOperation.NotEqual, new NotEqualBuilderStrategy() },
            { FilterOperation.Contains, new ContainsBuilderStrategy() },
            { FilterOperation.GreaterThan, new GreaterThanBuilderStrategy() },
            { FilterOperation.GreaterThanOrEqual, new GreaterThanOrEqualBuilderStrategy() },
            { FilterOperation.LessThan, new LessThanBuilderStrategy() },
            { FilterOperation.LessThanOrEqual, new LessThanOrEqualBuilderStrategy() },
            { FilterOperation.StartsWith, new StartsWithBuilderStrategy() },
            { FilterOperation.EndsWith, new EndsWithBuilderStrategy() },
        };

        public static MethodInfo BuildLINQExtensionMethod(
            string functionName,
            int numberOfParameters = 2,
            int overloadNumber = 0,
            Type[] genericElementTypes = null,
            Type enumerableType = null)
        {
            return (enumerableType ?? typeof(Queryable))
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(x => x.Name == functionName && x.GetParameters().Count() == numberOfParameters)
            .ElementAt(overloadNumber)
            .MakeGenericMethod(genericElementTypes ?? new[] { typeof(object) });
        }

        public static Expression BuildLINQLogicalOperatorExpression(Filter previousFilter, Expression exp, Expression builtExpression)
        {
            return _logicalOperatorToExpressionMap[previousFilter.LogicalOperator](exp, builtExpression);
        }

        public static Expression BuildLINQFilterExpression(
            Filter filter,
            Expression parentMember,
            Expression constant,
            bool useCaseInsensitiveComparison)
        {
            if (_filterOperatorToExpressionMap.ContainsKey(filter.Operator))
            {
                FilterBuilderContext builderContext = new FilterBuilderContext(_filterOperatorToExpressionMap[filter.Operator]);
                return builderContext.Build(parentMember, constant, useCaseInsensitiveComparison);
            }

            return null;
        }
    }
}
