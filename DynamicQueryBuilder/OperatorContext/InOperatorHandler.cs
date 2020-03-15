using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DynamicQueryBuilder.Models;
using DynamicQueryBuilder.Models.Enums;
using static DynamicQueryBuilder.DynamicQueryBuilderExceptions;

namespace DynamicQueryBuilder.OperatorContext
{
    internal class InOperatorHandler : IOperatorHandler
    {
        private readonly OperationContext _optContext;

        public InOperatorHandler(OperationContext optContext)
        {
            this._optContext = optContext;
        }

        public Expression Build(ParameterExpression param, Filter filter, bool usesCaseInsensitiveSource = false)
        {
            string stringFilterValue = filter.Value?.ToString();
            if (filter.Value == null)
            {
                throw new DynamicQueryException("You can't pass type null to In. Pass null as a string instead.");
            }

            // Split all data into a list
            List<string> splittedValues = stringFilterValue
                    .Split(ExpressionBuilder.PARAMETER_OPTION_DELIMITER)
                    .ToList();

            var equalsFilter = new Filter
            {
                Operator = FilterOperation.Equals,
                PropertyName = filter.PropertyName,
                Value = !usesCaseInsensitiveSource && filter.CaseSensitive
                    ? splittedValues.First()
                    : splittedValues.First().ToLowerInvariant(),
                CaseSensitive = filter.CaseSensitive
            };

            // Create the expression with the first value.
            Expression builtInExpression = this._optContext.GetExpression(param, equalsFilter, usesCaseInsensitiveSource);
            splittedValues.RemoveAt(0); // Remove the first value

            // Create query for every splitted value and append them.
            foreach (var item in splittedValues)
            {
                equalsFilter.Value = item;
                builtInExpression = Expression.Or(builtInExpression, this._optContext.GetExpression(param, equalsFilter, usesCaseInsensitiveSource));
            }

            return builtInExpression;
        }
    }
}