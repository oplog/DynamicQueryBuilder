using DynamicQueryBuilder.Constants;
using DynamicQueryBuilder.Extensions;
using DynamicQueryBuilder.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace DynamicQueryBuilder.Strategies
{
    internal static class StrategyUtils
    {
        private static Dictionary<FilterOperation, Func<Expression, Expression>> _filterOperatorToExpressionMap 
            = new Dictionary<FilterOperation, Func<Expression, Expression>>()
        {
            { FilterOperation.GreaterThan, (exp) => Expression.GreaterThan(exp, ExpressionConstants.ZeroConstantExpression) },
            { FilterOperation.GreaterThanOrEqual, (exp) => Expression.GreaterThanOrEqual(exp, ExpressionConstants.ZeroConstantExpression) },
            { FilterOperation.LessThan, (exp) => Expression.LessThan(exp, ExpressionConstants.ZeroConstantExpression) },
            { FilterOperation.LessThanOrEqual, (exp) => Expression.LessThanOrEqual(exp, ExpressionConstants.ZeroConstantExpression) },
        };

        public static MethodInfo GetCompareToMethodForEnum(Type enumType)
        {
            return enumType.GetEnumUnderlyingType().GetMethod("CompareTo", new[] { enumType.GetEnumUnderlyingType() });
        }

        public static Expression ConvertEnumToUnderlyingType(Expression expression)
        {
            return Expression.Convert(expression, expression.Type.GetEnumUnderlyingType());
        }

        public static Expression CompareEnums(FilterOperation filterOperation, Expression parentMember, Expression constant)
        {
            var convertedParentMember = ConvertEnumToUnderlyingType(parentMember);
            var convertedConstant = ConvertEnumToUnderlyingType(constant);
            var method = GetCompareToMethodForEnum(parentMember.Type);
            var compareToResult = Expression.Call(convertedParentMember, method, convertedConstant);

            return _filterOperatorToExpressionMap.ContainsKey(filterOperation)
                ? _filterOperatorToExpressionMap[filterOperation](compareToResult)
                : null;
        }

        public static Expression CompareStrings(FilterOperation filterOperation, Expression parentMember, Expression constant)
        {
            Expression compareToExpression = Expression.Call(parentMember, ExtensionMethods.StringCompareTo, constant);

            return _filterOperatorToExpressionMap.ContainsKey(filterOperation)
                ? _filterOperatorToExpressionMap[filterOperation](compareToExpression)
                : null;
        }

        public static Expression ToLowerIfCaseInsensitive(Expression exp, bool usesCaseInsensitiveComparison)
        {
            return usesCaseInsensitiveComparison ? Expression.Call(exp, ExtensionMethods.ToLowerMethod) : exp;
        }
    }
}
