using DynamicQueryBuilder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DynamicQueryBuilder.Visitors
{
    public class OrderFunctionVisitor : ExpressionVisitor
    {
        private Expression _lastOrderFunction;
        private Expression _lastThenByFunction;
        private readonly Type _setElementType;
        private readonly Expression _entryNode;
        private readonly IEnumerable<OrderOptionDetails> _orderOptionDetails;

        private bool _toApply;

        public OrderFunctionVisitor(Expression entryNode, IEnumerable<OrderOptionDetails> orderOptionDetails, Type setElementType)
        {
            _entryNode = entryNode;
            _setElementType = setElementType;
            _orderOptionDetails = orderOptionDetails;
            Visit(entryNode);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node != null && node.Method != null)
            {
                if (_toApply)
                {
                    if (node == _lastOrderFunction || node == _lastThenByFunction)
                    {
                        MethodCallExpression currentNode = node;
                        foreach (OrderOptionDetails orderOpts in _orderOptionDetails)
                        {
                            currentNode = Expression.Call(
                                LINQUtils.BuildLINQExtensionMethod(orderOpts.Direction == SortingDirection.Asc
                                ? nameof(Enumerable.ThenBy)
                                : nameof(Enumerable.ThenByDescending),
                                                         numberOfParameters: 2,
                                                         genericElementTypes: new[] { _setElementType, orderOpts.ParameterExpression.Type }),
                                currentNode,
                                Expression.Quote(orderOpts.Expression));
                        }

                        _toApply = false;
                        return base.VisitMethodCall(currentNode);
                    }
                    else
                    {

                    }
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

        public Expression ApplyOrders()
        {
            _toApply = true;
            return Visit(_entryNode);
        }
    }
}
