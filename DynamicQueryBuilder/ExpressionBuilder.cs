﻿// <copyright file="ExpressionBuilder.cs" company="Oplog">
// Copyright (c) Oplog. All rights reserved.
// </copyright>

using DynamicQueryBuilder.Constants;
using DynamicQueryBuilder.Extensions;
using DynamicQueryBuilder.Models;
using DynamicQueryBuilder.Models.Enums;
using DynamicQueryBuilder.Utils;
using DynamicQueryBuilder.Utils.Extensions;
using DynamicQueryBuilder.Visitors;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web;
using static DynamicQueryBuilder.DynamicQueryBuilderExceptions;

[assembly: InternalsVisibleTo("DynamicQueryBuilder.UnitTests")]
namespace DynamicQueryBuilder
{
    public static class ExpressionBuilder
    {
        /// <summary>
        /// Applies the given DynamicQueryOptions to the IEnumerable instace.
        /// </summary>
        /// <typeparam name="T">Generic type of the IEnumerable.</typeparam>
        /// <param name="currentSet">Existing IEnumerable instance.</param>
        /// <param name="dynamicQueryOptions">Query options to apply.</param>
        /// <returns>DynamicQueryOptions applied IEnumerable instance,</returns>
        public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> currentSet, DynamicQueryOptions dynamicQueryOptions)
        {
            return ApplyFilters((IQueryable)currentSet, dynamicQueryOptions).Cast<T>();
        }

        /// <summary>
        /// Applies the given DynamicQueryOptions to the generic IEnumerable instace.
        /// </summary>
        /// <param name="currentSet">Existing IEnumerable instance.</param>
        /// <param name="dynamicQueryOptions">Query options to apply.</param>
        /// <returns>DynamicQueryOptions applied IEnumerable instance,</returns>
        public static IQueryable ApplyFilters(this IQueryable currentSet, DynamicQueryOptions dynamicQueryOptions)
        {
            try
            {
                if (dynamicQueryOptions == null || currentSet == null)
                {
                    return currentSet;
                }

                Expression exp = null;

                // Create the query parameter (x =>)
                ParameterExpression param = Expression.Parameter(currentSet.ElementType, currentSet.ElementType.Name.ToLower());

                // Check if we have any filters
                if (dynamicQueryOptions.Filters != null && dynamicQueryOptions.Filters.Count > 0)
                {
                    // Lets build the first expression and then iterate the rest and append them to this one
                    exp = BuildFilterExpression(param,
                                                dynamicQueryOptions.Filters.First(),
                                                dynamicQueryOptions.UsesCaseInsensitiveSource);

                    // We start to iterate with the second element here because we have just built the first expression up above
                    for (int i = 1; i < dynamicQueryOptions.Filters.Count; ++i)
                    {
                        // Build the current expression
                        Expression builtExpression = BuildFilterExpression(param,
                                                                           dynamicQueryOptions.Filters[i],
                                                                           dynamicQueryOptions.UsesCaseInsensitiveSource);

                        // Get the previous filter to retrieve the logical operator between the current and the next filter
                        Filter previousFilter = dynamicQueryOptions.Filters.ElementAtOrDefault(i - 1);

                        exp = LINQUtils.BuildLINQLogicalOperatorExpression(previousFilter, exp, builtExpression);
                    }
                }

                if (dynamicQueryOptions.SortOptions != null && dynamicQueryOptions.SortOptions.Count > 0)
                {
                    var orderLambdas = new List<OrderOptionDetails>();
                    foreach (SortOption so in dynamicQueryOptions.SortOptions)
                    {
                        Expression paramExpr = ExtractMember(param, so.PropertyName, false);
                        orderLambdas.Add(new OrderOptionDetails
                        {
                            Direction = so.SortingDirection,
                            Expression = Expression.Lambda(paramExpr, param),
                            ParameterExpression = paramExpr,
                            CaseSensitive = so.CaseSensitive
                        });
                    }

                    var orderVisitor = new OrderFunctionVisitor(
                        currentSet.Expression,
                        orderLambdas,
                        currentSet.ElementType,
                        dynamicQueryOptions.UsesCaseInsensitiveSource,
                        dynamicQueryOptions.IgnorePredefinedOrders);

                    currentSet = currentSet.Provider.CreateQuery(orderVisitor.ApplyOrders());
                }

                if (exp != null)
                {
                    MethodCallExpression whereFilter = Expression.Call(
                        LINQUtils.BuildLINQExtensionMethod(
                            nameof(Enumerable.Where),
                            genericElementTypes: new[] { currentSet.ElementType },
                            enumerableType: typeof(Queryable)),
                        currentSet.Expression,
                        Expression.Quote(Expression.Lambda(exp, param)));

                    currentSet = currentSet.Provider.CreateQuery(whereFilter);
                }

                if (dynamicQueryOptions.PaginationOption != null)
                {
                    if (dynamicQueryOptions.PaginationOption.AssignDataSetCount)
                    {
                        dynamicQueryOptions.PaginationOption.DataSetCount = (int)ExtensionMethods.CountFunction.Invoke(null, new[] { currentSet });
                    }

                    if (dynamicQueryOptions.PaginationOption.Offset > 0)
                    {
                        MethodCallExpression skip = Expression.Call(
                        ExtensionMethods.SkipFunction,
                        currentSet.Expression,
                        Expression.Constant(dynamicQueryOptions.PaginationOption.Offset));

                        currentSet = currentSet.Provider.CreateQuery(skip);
                    }

                    if (dynamicQueryOptions.PaginationOption.Count > 0)
                    {
                        MethodCallExpression take = Expression.Call(
                            ExtensionMethods.TakeFunction,
                            currentSet.Expression,
                            Expression.Constant(dynamicQueryOptions.PaginationOption.Count));

                        currentSet = currentSet.Provider.CreateQuery(take);
                    }
                }

                return currentSet;
            }
            catch (Exception ex)
            {
                throw new DynamicQueryException("DynamicQueryBuilder has encountered an unhandled exception", string.Empty, ex);
            }
        }

        /// <summary>
        /// Parses a Querystring into DynamicQueryOptions instance.
        /// </summary>
        /// <param name="query">QueryString to parse.</param>
        /// <param name="opShortCodes">Custom operation shortcodes.</param>
        /// <returns>Parsed DynamicQueryOptions instance.</returns>
        public static DynamicQueryOptions ParseQueryOptions(string query, CustomOpCodes opShortCodes = null)
        {
            try
            {
                var dynamicQueryOptions = new DynamicQueryOptions();
                if (string.IsNullOrEmpty(query))
                {
                    return dynamicQueryOptions;
                }

                ////+ character issue
                ////https://docs.microsoft.com/en-us/dotnet/api/system.web.httputility.urlencode?redirectedfrom=MSDN&view=net-5.0#System_Web_HttpUtility_UrlEncode_System_String_
                if (QueryStringParser.IsQueryStringEncoded(query))
                {
                    query = HttpUtility.UrlDecode(query);
                }

                DynamicQueryOptions innerQueryOptions = null;
                const string innerMemberKey = "v=(";
                int indexOfInnerMemberKey = query.IndexOf(innerMemberKey, StringComparison.Ordinal);
                if (indexOfInnerMemberKey != -1)
                {
                    indexOfInnerMemberKey += innerMemberKey.Length;
                    string innerQuery = query.Substring(indexOfInnerMemberKey, query.LastIndexOf(')') - indexOfInnerMemberKey);
                    innerQueryOptions = ParseQueryOptions(innerQuery, opShortCodes);
                    query = query.Replace(innerQuery, string.Empty);
                }

                string[] defaultArrayValue = new string[0];
                NameValueCollection queryCollection = HttpUtility.ParseQueryString(query);

                IEnumerable<QueryStringParserResult> queryStringParserResults = QueryStringParser
                                                                                    .GetAllParameterWithValue(query)
                                                                                    .Where(e => !string.IsNullOrEmpty(e.Value) && e.Value.Contains(InternalConstants.PLUS_CHARACTER)).ToList();
                if (queryStringParserResults.Any())
                {
                    QueryStringParser.ReplaceNameValueCollection(queryStringParserResults, queryCollection, InternalConstants.PARAMETER_VALUE_KEY);
                }

                string[] operations = queryCollection
                    .GetValues(InternalConstants.OPERATION_PARAMETER_KEY)
                    ?.Select(x => x.ClearSpaces())
                    .ToArray() ?? defaultArrayValue;

                string[] parameterNames = queryCollection
                    .GetValues(InternalConstants.PARAMETER_NAME_KEY)
                    ?.Select(x => x.ClearSpaces())
                    .ToArray() ?? defaultArrayValue;

                string[] parameterValues = queryCollection
                    .GetValues(InternalConstants.PARAMETER_VALUE_KEY)
                    ?.ToArray() ?? defaultArrayValue;

                string[] sortOptions = queryCollection
                    .GetValues(InternalConstants.SORT_OPTIONS_PARAMETER_KEY)
                    ?.Select(x => x.ClearSpaces())
                    .ToArray() ?? defaultArrayValue;

                string[] offsetOptions = queryCollection
                    .GetValues(InternalConstants.OFFSET_PARAMETER_KEY)
                    ?.Select(x => x.ClearSpaces())
                    .ToArray() ?? defaultArrayValue;

                string[] countOptions = queryCollection
                    .GetValues(InternalConstants.COUNT_PARAMETER_KEY)
                    ?.Select(x => x.ClearSpaces())
                    .ToArray() ?? defaultArrayValue;

                PopulateDynamicQueryOptions(
                    dynamicQueryOptions,
                    operations,
                    parameterNames,
                    parameterValues,
                    sortOptions,
                    offsetOptions,
                    countOptions,
                    opShortCodes: opShortCodes ?? Defaults.DefaultOpShortCodes,
                    memberQueryOptions: innerQueryOptions);

                return dynamicQueryOptions;
            }
            catch (Exception ex)
            {
                throw new DynamicQueryException("DynamicQueryBuilder has encountered an unhandled exception", query, ex);
            }
        }

        /// <summary>
        /// Populates an Instance of DynamicQueryOptions from parsed query string values.
        /// </summary>
        /// <param name="dynamicQueryOptions">DynamicQueryOptions ref to populate to.</param>
        /// <param name="operations">Operations array.</param>
        /// <param name="parameterNames">ParameterNames array.</param>
        /// <param name="parameterValues">ParameterValues array.</param>
        /// <param name="sortOptions">SortOptions array.</param>
        /// <param name="offsetOptions">Offset array.</param>
        /// <param name="countOptions">Count array.</param>
        /// <param name="opShortCodes">CustomOpCodes instance.</param>
        internal static void PopulateDynamicQueryOptions(
            DynamicQueryOptions dynamicQueryOptions,
            string[] operations,
            string[] parameterNames,
            string[] parameterValues,
            string[] sortOptions,
            string[] offsetOptions,
            string[] countOptions,
            CustomOpCodes opShortCodes = null,
            DynamicQueryOptions memberQueryOptions = null)
        {
            if (dynamicQueryOptions == null)
            {
                throw new DynamicQueryException("DynamicQueryOptions should not be null");
            }

            // Check the counts for every operation, since they work in tuples they should be the same.
            if (AreCountsMatching(operations, parameterNames, parameterValues))
            {
                for (int i = 0; i < operations.Length; i++)
                {
                    FilterOperation foundOperation;
                    string[] ops = operations[i]?.Split('|');
                    if (ops == null)
                    {
                        throw new OperationNotSupportedException("Invalid operation. Operation value is null.");
                    }

                    string logicalOpRaw = ops.ElementAtOrDefault(1) ?? LogicalOperator.AndAlso.ToString();
                    if (!Enum.TryParse(logicalOpRaw, ignoreCase: true, out LogicalOperator logicalOperator))
                    {
                        throw new DynamicQueryException($"Invalid logical operator formation with value of: {logicalOpRaw}");
                    }

                    // Check if we support this operation.
                    if (Enum.TryParse(ops[0], ignoreCase: true, out FilterOperation parsedOperation))
                    {
                        foundOperation = parsedOperation;
                    }
                    else if (opShortCodes != null
                        && opShortCodes.Count > 0
                        && opShortCodes.TryGetValue(ops[0], out FilterOperation shortCodeOperation)) // Whoop maybe its a short code ?
                    {
                        foundOperation = shortCodeOperation;
                    }
                    else
                    {
                        throw new OperationNotSupportedException($"Invalid operation {ops[0]}");
                    }

                    string[] splittedParameterName = parameterNames[i].Split(InternalConstants.PARAMETER_OPTION_DELIMITER);
                    bool isCaseSensitive = false;
                    if (splittedParameterName.Length > 1)
                    {
                        if (splittedParameterName[1].ToLower() == InternalConstants.CASE_SENSITIVITY_PARAMETER_OPTION)
                        {
                            isCaseSensitive = true;
                        }
                        else
                        {
                            throw new InvalidDynamicQueryException($"Invalid extra option provided for filter property {splittedParameterName[0]}. Received value was {splittedParameterName[1]}");
                        }
                    }

                    var composedFilter = new Filter
                    {
                        Operator = foundOperation,
                        PropertyName = splittedParameterName[0],
                        CaseSensitive = isCaseSensitive,
                        LogicalOperator = logicalOperator
                    };

                    if (foundOperation >= FilterOperation.Any)
                    {
                        composedFilter.Value = memberQueryOptions;
                    }
                    else
                    {
                        composedFilter.Value = parameterValues[i];
                    }

                    dynamicQueryOptions.Filters.Add(composedFilter);
                }
            }
            else
            {
                throw new QueryTripletsMismatchException("Invalid query structure. Operation, parameter name and value triplets are not matching.");
            }

            if (sortOptions != null && sortOptions.Length >= 1)
            {
                foreach (string sortOption in sortOptions)
                {
                    if (!string.IsNullOrEmpty(sortOption))
                    {
                        // Split the property name to sort and the direction.
                        string[] splittedParam = sortOption.Split(InternalConstants.PARAMETER_OPTION_DELIMITER);

                        bool isCaseSensitive = false;
                        SortingDirection direction = SortingDirection.Asc;
                        if (splittedParam.Length == 2)
                        {
                            // If we get an array of 2 we have a sorting direction, try to apply it.
                            if (!Enum.TryParse(splittedParam[1], true, out direction))
                            {
                                throw new InvalidDynamicQueryException("Invalid sorting direction");
                            }
                        }
                        else if (splittedParam.Length == 3)
                        {
                            if (splittedParam[2].ToLower() == InternalConstants.CASE_SENSITIVITY_PARAMETER_OPTION)
                            {
                                isCaseSensitive = true;
                            }
                            else
                            {
                                throw new InvalidDynamicQueryException($"Invalid extra option provided for sort property {splittedParam[0]}. Received value was {splittedParam[2]}");
                            }
                        }
                        else if (splittedParam.Length > 3) // If we get more than 3 results in the array, url must be wrong.
                        {
                            throw new InvalidDynamicQueryException("Invalid query structure. SortOption is misformed");
                        }

                        // Create the sorting option.
                        dynamicQueryOptions.SortOptions.Add(new SortOption
                        {
                            SortingDirection = direction,
                            PropertyName = splittedParam[0],
                            CaseSensitive = isCaseSensitive
                        });
                    }
                }
            }

            if (offsetOptions != null
                && countOptions != null
                && countOptions.Length > 0
                && offsetOptions.Length > 0
                && offsetOptions.Length == countOptions.Length)
            {
                if (int.TryParse(countOptions[0], out int countValue) // We only care about the first values.
                    && int.TryParse(offsetOptions[0], out int offsetValue))
                {
                    dynamicQueryOptions.PaginationOption = new PaginationOption
                    {
                        Count = countValue,
                        Offset = offsetValue
                    };
                }
                else
                {
                    throw new DynamicQueryException("Invalid pagination options");
                }
            }
        }

        /// <summary>
        /// Builds a runtime generic dynamic query with the given filters.
        /// </summary>
        /// <param name="param">Created parameter instance or current expression body.</param>
        /// <param name="filter">Filter instance to build.</param>
        /// <param name="usesCaseInsensitiveSource">Flag to detect if the query is going to run on a SQL database.</param>
        /// <returns>Built query expression.</returns>
        internal static Expression BuildFilterExpression(ParameterExpression param, Filter filter, bool usesCaseInsensitiveSource = false)
        {
            string stringFilterValue = filter.Value?.ToString();
            Expression parentMember = ExtractMember(param, filter.PropertyName, stringFilterValue == "null");
            
            // We are handling In operations seperately which are basically a list of OR=EQUALS operation. We recursively handle this operation.
            if (filter.Operator == FilterOperation.In)
            {
                if (filter.Value == null)
                {
                    throw new DynamicQueryException("You can't pass type null to In. Pass null as a string instead.");
                }

                // Split all data into a list
                List<string> splittedValues = stringFilterValue.Split(InternalConstants.PARAMETER_OPTION_DELIMITER).ToList();
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
                Expression builtInExpression = BuildFilterExpression(param, equalsFilter, usesCaseInsensitiveSource);
                splittedValues.RemoveAt(0); // Remove the first value

                // Create query for every splitted value and append them.
                foreach (var item in splittedValues)
                {
                    equalsFilter.Value = item;
                    builtInExpression = Expression.Or(builtInExpression, BuildFilterExpression(param, equalsFilter, usesCaseInsensitiveSource));
                }

                return builtInExpression;
            }

            // We should convert the data into its own type before we do any query building.
            object convertedValue = null;
            if (filter.Operator < FilterOperation.Any)
            {
                convertedValue = stringFilterValue != "null"
                    ? TypeDescriptor.GetConverter(parentMember.Type).ConvertFromInvariantString(
                       !usesCaseInsensitiveSource
                       ? filter.CaseSensitive
                            ? stringFilterValue
                            : stringFilterValue?.ToLowerInvariant()
                       : stringFilterValue?.ToLowerInvariant())
                    : null;
            }

            if (convertedValue is DateTime dateTimeValue)
            {
                convertedValue = DateTime.SpecifyKind(dateTimeValue, DateTimeKind.Utc).ToUniversalTime();
            }

            Expression constant = Expression.Constant(convertedValue);

            switch (filter.Operator)
            {
                case FilterOperation.Any:
                case FilterOperation.All:
                    ParameterExpression memberParam = Expression.Parameter(
                        parentMember.Type.GenericTypeArguments[0],
                        parentMember.Type.GenericTypeArguments[0].Name);

                    MethodInfo requestedFunction = LINQUtils.BuildLINQExtensionMethod(
                        filter.Operator.ToString(),
                        genericElementTypes: new[] { memberParam.Type },
                        enumerableType: typeof(Enumerable));

                    Expression builtMemberExpression = BuildFilterExpression(memberParam, (filter.Value as DynamicQueryOptions).Filters.First(), usesCaseInsensitiveSource);

                    return Expression.Call(
                        requestedFunction,
                        Expression.PropertyOrField(param, filter.PropertyName),
                        Expression.Lambda(builtMemberExpression, memberParam));
                default:
                    return LINQUtils.BuildLINQFilterExpression(filter,
                        parentMember,
                        constant,
                        useCaseInsensitiveComparison: usesCaseInsensitiveSource && !filter.CaseSensitive);
            }
        }

        /// <summary>
        /// Constructs a query parameter Expression with the given PropertyName.
        /// Supports nested objects.
        /// </summary>
        /// <param name="param">Current parameter body.</param>
        /// <param name="propertyName">Parameter name to construct.</param>
        /// <param name="isValueNull">Shows if the value that the query is looking for a null.</param>
        /// <returns>Constructed parameter name.</returns>
        internal static Expression ExtractMember(ParameterExpression param, string propertyName, bool isValueNull = false)
        {
            if (param == null || string.IsNullOrEmpty(propertyName))
            {
                throw new DynamicQueryException("Both parameter expression and propertyname are required");
            }

            if (propertyName == "_")
            {
                return param;
            }

            Expression parentMember = param;

            // If the property name is refering to a nested object property, we should iterate through every nested type to get to the final property target.
            if (propertyName.Contains('.'))
            {
                foreach (string innerMember in propertyName.Split('.'))
                {
                    parentMember = Expression.PropertyOrField(parentMember, innerMember);
                }
            }
            else
            {
                parentMember = Expression.PropertyOrField(parentMember, propertyName);
            }

            // Nullable Type
            if (Nullable.GetUnderlyingType(parentMember.Type) != null)
            {
                parentMember = isValueNull
                    ? parentMember
                    : Expression.PropertyOrField(parentMember, "Value");
            }
            else
            {
                if (isValueNull && parentMember.Type != typeof(string))
                {
                    throw new InvalidDynamicQueryException($"Type of property {propertyName} is not nullable but query value was received null");
                }
            }

            return parentMember;
        }

        private static bool AreCountsMatching(string[] operations, string[] parameterNames, string[] parameterValues)
        {
            return new int[] { operations.Length, parameterNames.Length, parameterValues.Length }.Distinct().Count() == 1;
        }
    }
}
