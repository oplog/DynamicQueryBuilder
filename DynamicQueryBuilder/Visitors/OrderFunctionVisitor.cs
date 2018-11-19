using DynamicQueryBuilder.Models;
using DynamicQueryBuilder.Models.Enums;
using DynamicQueryBuilder.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DynamicQueryBuilder.Visitors
{
    public sealed class OrderFunctionVisitor : ExpressionVisitor
    {
        private const sbyte CASE_SENSITIVE_ORDER_FUNCTION_PARAM_COUNT = 3;
        private const sbyte CASE_INSENSITIVE_ORDER_FUNCTION_PARAM_COUNT = 2;

        private readonly bool _usesCaseInsensitiveSource;
        private readonly bool _ignorePredefinedOrders;

        private readonly Type _setElementType;
        private readonly Expression _entryNode;
        private readonly IEnumerable<OrderOptionDetails> _orderOptionDetails;

        private Expression _lastOrderFunction;
        private Expression _lastThenByFunction;

        private bool _toApply;
        private bool _ordersApplied;

        public OrderFunctionVisitor(
            Expression entryNode,
            IEnumerable<OrderOptionDetails> orderOptionDetails,
            Type setElementType,
            bool usesCaseInsensitiveSource,
            bool ignorePredefinedOrders)
        {
            _usesCaseInsensitiveSource = usesCaseInsensitiveSource;
            _ignorePredefinedOrders = ignorePredefinedOrders;

            _entryNode = entryNode;
            _setElementType = setElementType;
            _orderOptionDetails = orderOptionDetails;

            Visit(entryNode);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node != null
                && node.Method != null
                && !_ordersApplied)
            {
                if (_toApply)
                {
                    bool isPreOrdered = (_lastOrderFunction != null || _lastThenByFunction != null) && !_ignorePredefinedOrders;
                    if (isPreOrdered && !(node == _lastOrderFunction || node == _lastThenByFunction))
                    {
                        return base.VisitMethodCall(node);
                    }

                    Expression currentNode = node;
                    IterateOrderOptions(ref isPreOrdered, ref currentNode);

                    _ordersApplied = true;
                    _toApply = !_ordersApplied;
                    return base.VisitMethodCall((MethodCallExpression)currentNode);
                }
                else
                {
                    if (node.Method.Name == nameof(Enumerable.OrderBy) || node.Method.Name == nameof(Enumerable.OrderByDescending))
                    {
                        _lastOrderFunction = node;
                    }
                    else if (node.Method.Name == nameof(Enumerable.ThenBy) || node.Method.Name == nameof(Enumerable.ThenByDescending))
                    {
                        _lastThenByFunction = node;
                    }
                }
            }

            return base.VisitMethodCall(node);
        }

        private void IterateOrderOptions(ref bool isPreOrdered, ref Expression currentNode)
        {
            foreach (OrderOptionDetails orderOpts in _orderOptionDetails)
            {
                if (!_usesCaseInsensitiveSource && orderOpts.ParameterExpression.Type == typeof(string))
                {
                    currentNode = Expression.Call(
                        GetSortMethod(
                            orderOpts.Direction,
                            orderOpts.ParameterExpression.Type,
                            ref isPreOrdered,
                            orderOpts.CaseSensitive),
                        currentNode,
                        Expression.Quote(orderOpts.Expression),
                        Expression.Constant(orderOpts.CaseSensitive
                            ? StringComparer.InvariantCulture
                            : StringComparer.InvariantCultureIgnoreCase));
                }
                else
                {
                    currentNode = Expression.Call(
                    GetSortMethod(
                        orderOpts.Direction,
                        orderOpts.ParameterExpression.Type,
                        ref isPreOrdered,
                        orderOpts.CaseSensitive),
                    currentNode,
                    Expression.Quote(orderOpts.Expression));
                }
            }
        }

        public Expression ApplyOrders()
        {
            _toApply = true;
            Expression result = Visit(_entryNode);
            if (!_ordersApplied)
            {
                bool preOrdered = false;
                IterateOrderOptions(ref preOrdered, ref result);
            }

            return result;
        }

        private MethodInfo GetSortMethod(
            SortingDirection sortDirection,
            Type propertyType,
            ref bool preOrdered,
            bool caseSensitive)
        {
            string methodName = sortDirection == SortingDirection.Asc
            ? preOrdered
                ? nameof(Enumerable.ThenBy)
                : nameof(Enumerable.OrderBy)
            : preOrdered
                ? nameof(Enumerable.ThenByDescending)
                : nameof(Enumerable.OrderByDescending);

            preOrdered = true;
            return LINQUtils.BuildLINQExtensionMethod(
                methodName,
                numberOfParameters: !_usesCaseInsensitiveSource
                && caseSensitive
                && propertyType == typeof(string)
                    ? CASE_SENSITIVE_ORDER_FUNCTION_PARAM_COUNT
                    : CASE_INSENSITIVE_ORDER_FUNCTION_PARAM_COUNT,
                genericElementTypes: new[] { _setElementType, propertyType });
        }
    }
}
