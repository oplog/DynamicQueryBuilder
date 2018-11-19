// <copyright file="DynamicQueryAttribute.cs" company="Oplog">
// Copyright (c) Oplog. All rights reserved.
// </copyright>

using DynamicQueryBuilder.Models;
using DynamicQueryBuilder.Models.Enums;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using static DynamicQueryBuilder.DynamicQueryBuilderExceptions;

namespace DynamicQueryBuilder
{
    /// <summary>
    /// Enables the DynamicQueryParser functionality for the given action.
    /// </summary>
    public sealed class DynamicQueryAttribute : ActionFilterAttribute
    {
        internal readonly int _maxCountSize = 0;
        internal readonly bool _includeDataSetCountToPagination;
        internal readonly PaginationBehaviour _exceededPaginationCountBehaviour;
        internal readonly string _resolveFromParameter;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicQueryAttribute"/> class.
        /// Also, Finds and constructs DynamicQueryOptions class in the parameters.
        /// </summary>
        /// <param name="maxCountSize">Max data set count for the result set.</param>
        /// <param name="includeDataSetCountToPagination">Includes the total data set count to the options class.</param>
        /// <param name="exceededPaginationCountBehaviour">Behaviour when the requested data set count greater than max count size.</param>
        /// <param name="resolveFromParameter">Resolves the dynamic query string from the given query parameter value.</param>
        public DynamicQueryAttribute(
            int maxCountSize = 100,
            bool includeDataSetCountToPagination = true,
            PaginationBehaviour exceededPaginationCountBehaviour = PaginationBehaviour.GetMax,
            string resolveFromParameter = "")
        {
            _maxCountSize = maxCountSize;
            _includeDataSetCountToPagination = includeDataSetCountToPagination;
            _exceededPaginationCountBehaviour = exceededPaginationCountBehaviour;
            _resolveFromParameter = resolveFromParameter;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ParameterDescriptor dynamicQueryParameter =
                context.ActionDescriptor
                       .Parameters
                       .FirstOrDefault(x => x.ParameterType == typeof(DynamicQueryOptions));

            DynamicQueryBuilderSettings dqbSettings = context
                .HttpContext
                .RequestServices?
                .GetService(typeof(DynamicQueryBuilderSettings)) as DynamicQueryBuilderSettings
                ?? new DynamicQueryBuilderSettings();

            if (dynamicQueryParameter != null)
            {
                DynamicQueryOptions parsedOptions =
                    ExpressionBuilder.ParseQueryOptions(
                        context.HttpContext.Request.QueryString.Value,
                        _resolveFromParameter,
                        dqbSettings.CustomOpCodes);

                parsedOptions.UsesCaseInsensitiveSource = dqbSettings.UsesCaseInsensitiveSource;
                parsedOptions.IgnorePredefinedOrders = dqbSettings.IgnorePredefinedOrders;
                if (parsedOptions.PaginationOption != null)
                {
                    parsedOptions.PaginationOption.AssignDataSetCount = _includeDataSetCountToPagination;
                    if (parsedOptions.PaginationOption.Count > _maxCountSize)
                    {
                        if (_exceededPaginationCountBehaviour == PaginationBehaviour.GetMax)
                        {
                            parsedOptions.PaginationOption.Count = _maxCountSize;
                        }
                        else
                        {
                            throw new MaximumResultSetExceededException($"Given count {parsedOptions.PaginationOption.Count} exceeds the maximum amount");
                        }
                    }
                    else if (parsedOptions.PaginationOption.Count <= 0)
                    {
                        parsedOptions.PaginationOption.Count = 1;
                    }

                    if (parsedOptions.PaginationOption.Offset < 0)
                    {
                        parsedOptions.PaginationOption.Offset = 0;
                    }
                }
                else if (_includeDataSetCountToPagination && parsedOptions.PaginationOption == null)
                {
                    parsedOptions.PaginationOption = new PaginationOption
                    {
                        AssignDataSetCount = true
                    };
                }

                context.ActionArguments[dynamicQueryParameter.Name] = parsedOptions;
            }

            base.OnActionExecuting(context);
        }
    }
}
