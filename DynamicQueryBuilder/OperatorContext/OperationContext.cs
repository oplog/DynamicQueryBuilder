using System.Collections.Generic;
using System.Linq.Expressions;
using DynamicQueryBuilder.Models;
using DynamicQueryBuilder.Models.Enums;
using static DynamicQueryBuilder.DynamicQueryBuilderExceptions;

namespace DynamicQueryBuilder.OperatorContext
{
    internal class OperationContext
    {
        private readonly Dictionary<FilterOperation, IOperatorHandler> _contextScope = new Dictionary<FilterOperation, IOperatorHandler>();

        public OperationContext()
        {
            this._contextScope.TryAdd(FilterOperation.Equals, new EqualityOperatorHandler());
            this._contextScope.TryAdd(FilterOperation.In, new InOperatorHandler(this));
        }

        public Expression GetExpression(ParameterExpression param, Filter filter, bool usesCaseInsensitiveSource = false)
        {
            if (!this._contextScope.TryGetValue(filter.Operator, out IOperatorHandler operatorHandler))
            {
                throw new DynamicQueryException("Invalid initialization of OperationContext");
            }

            if (filter.Operator < FilterOperation.Any)
            {
                Expression parentMember = ExpressionBuilder.BuildParentMember(
                    param, filter, usesCaseInsensitiveSource);

                object boxedValue = ExpressionBuilder.ConvertAndBox(
                    filter.Value?.ToString(),
                    parentMember.Type,
                    filter.CaseSensitive,
                    usesCaseInsensitiveSource);

                return operatorHandler.Build((ParameterExpression)parentMember, filter, usesCaseInsensitiveSource);
            }

            return operatorHandler.Build(param, filter, usesCaseInsensitiveSource);
        }
    }
}