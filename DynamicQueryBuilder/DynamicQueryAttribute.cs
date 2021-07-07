// <copyright file="DynamicQueryAttribute.cs" company="Oplog">
// Copyright (c) Oplog. All rights reserved.
// </copyright>

using DynamicQueryBuilder.Models;
using DynamicQueryBuilder.Models.Enums;

using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;

using System.Collections.Specialized;
using System.Linq;
using System.Web;

using static DynamicQueryBuilder.DynamicQueryBuilderExceptions;

namespace DynamicQueryBuilder
{
    /// <summary>
    /// Enables the DynamicQueryParser functionality for the given action.
    /// </summary>
    public sealed class DynamicQueryAttribute : ActionFilterAttribute
    {
        internal readonly int? _maxCountSize;
        internal readonly bool? _includeDataSetCountToPagination;
        internal readonly PaginationBehaviour? _exceededPaginationCountBehaviour;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicQueryAttribute"/> class.
        /// DynamicQueryAttributeGlobalConfig definations is used for parameters with a null value.
        /// Also, Finds and constructs DynamicQueryOptions class in the parameters.
        /// </summary>
        /// <param name="maxCountSize">Max data set count for the result set.</param>
        /// <param name="includeDataSetCountToPagination">Includes the total data set count to the options class.</param>
        /// <param name="exceededPaginationCountBehaviour">Behaviour when the requested data set count greater than max count size.</param>
        public DynamicQueryAttribute(
            int? maxCountSize = null,
            bool? includeDataSetCountToPagination = null,
            PaginationBehaviour? exceededPaginationCountBehaviour = null)
        {
            _maxCountSize = maxCountSize;
            _includeDataSetCountToPagination = includeDataSetCountToPagination;
            _exceededPaginationCountBehaviour = exceededPaginationCountBehaviour;
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
                string queryValue = context.HttpContext.Request.QueryString.Value;
                if (dqbSettings.QueryOptionsResolver != null)
                {
                    if (dqbSettings.QueryOptionsResolver is QueryStringResolver qsResolver)
                    {
                        string[] values;
                        if (!string.IsNullOrEmpty(qsResolver.ResolveFrom))
                        {
                            NameValueCollection resolveParameterValues = HttpUtility.ParseQueryString(context.HttpContext.Request.QueryString.Value);
                            values = resolveParameterValues.GetValues(qsResolver.ResolveFrom) ?? new string[] { string.Empty };
                        }
                        else
                        {
                            values = new string[] { queryValue };
                        }

                        queryValue = HttpUtility.UrlDecode(
                            qsResolver.DecodeFunction != null
                            ? qsResolver.DecodeFunction(values[0])
                            : values[0]);
                    }
                    else if (dqbSettings.QueryOptionsResolver is HttpHeaderResolver hhResolver)
                    {
                        string rawHeaderValue = context
                            .HttpContext
                            .Request
                            .Headers[hhResolver.HttpHeaderName]
                            .FirstOrDefault() ?? string.Empty;

                        queryValue = hhResolver.DecodeFunction != null
                            ? hhResolver.DecodeFunction(rawHeaderValue)
                            : rawHeaderValue;
                    }
                }

                DynamicQueryOptions parsedOptions =
                    ExpressionBuilder.ParseQueryOptions(
                        queryValue,
                        dqbSettings.CustomOpCodes);

                parsedOptions.UsesCaseInsensitiveSource = dqbSettings.UsesCaseInsensitiveSource;
                parsedOptions.IgnorePredefinedOrders = dqbSettings.IgnorePredefinedOrders;

                bool includeDataSetCountToPagination = _includeDataSetCountToPagination == null ? DynamicQueryAttributeGlobalConfig.IncludeDataSetCountToPagination : _includeDataSetCountToPagination.GetValueOrDefault();
                if (parsedOptions.PaginationOption != null)
                {
                    parsedOptions.PaginationOption.AssignDataSetCount = includeDataSetCountToPagination;

                    int maxCountSize = _maxCountSize == null ? DynamicQueryAttributeGlobalConfig.MaxCountSize : _maxCountSize.GetValueOrDefault();
                    if (parsedOptions.PaginationOption.Count > maxCountSize)
                    {
                        PaginationBehaviour exceededPaginationCountBehaviour = _exceededPaginationCountBehaviour == null ? DynamicQueryAttributeGlobalConfig.ExceededPaginationCountBehaviour : _exceededPaginationCountBehaviour.GetValueOrDefault();
                        if (exceededPaginationCountBehaviour == PaginationBehaviour.GetMax)
                        {
                            parsedOptions.PaginationOption.Count = maxCountSize;
                        }
                        else
                        {
                            throw new MaximumResultSetExceededException($"Given count {parsedOptions.PaginationOption.Count} exceeds the maximum amount of {maxCountSize}");
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
                else if (includeDataSetCountToPagination && parsedOptions.PaginationOption == null)
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
