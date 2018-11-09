// <copyright file="ExpressionBuilder.cs" company="Oplog">
// Copyright (c) Oplog. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Web;
using static DynamicQueryBuilder.DynamicQueryBuilderExceptions;

[assembly: InternalsVisibleTo("DynamicQueryBuilder.UnitTests")]
namespace DynamicQueryBuilder
{
    public static class ExpressionBuilder
    {
        internal const string OPERATION_PARAMETER_KEY = "o";
        internal const string PARAMETER_NAME_KEY = "p";
        internal const string PARAMETER_VALUE_KEY = "v";
        internal const string SORT_OPTIONS_PARAMETER_KEY = "s";
        internal const string OFFSET_PARAMETER_KEY = "offset";
        internal const string COUNT_PARAMETER_KEY = "count";

        public static readonly CustomOpCodes DefaultOpShortCodes = new CustomOpCodes
        {
            { "eq", FilterOperation.Equals },
            { "lt", FilterOperation.LessThan },
            { "cts", FilterOperation.Contains },
            { "ne", FilterOperation.NotEqual },
            { "ew", FilterOperation.EndsWith },
            { "sw", FilterOperation.StartsWith },
            { "gt", FilterOperation.GreaterThan },
            { "ltoe", FilterOperation.LessThanOrEqual },
            { "gtoe", FilterOperation.GreaterThanOrEqual },
            { "any", FilterOperation.Any },
            { "all", FilterOperation.All }
        };

        #region ExtensionsMethods
        private static readonly MethodInfo _countFunction = BuildLINQExtensionMethod(nameof(Enumerable.Count), numberOfParameters: 1);

        private static readonly MethodInfo _skipFunction = BuildLINQExtensionMethod(nameof(Enumerable.Skip));

        private static readonly MethodInfo _takeFunction = BuildLINQExtensionMethod(nameof(Enumerable.Take));

        private static readonly MethodInfo _stringContainsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

        private static readonly MethodInfo _stringEndsWithMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });

        private static readonly MethodInfo _stringStartsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
        #endregion

        /// <summary>
        /// Applies the given DynamicQueryOptions to the IEnumerable instace.
        /// </summary>
        /// <typeparam name="T">Generic type of the IEnumerable.</typeparam>
        /// <param name="currentSet">Existing IEnumerable instance.</param>
        /// <param name="dynamicQueryOptions">Query options to apply.</param>
        /// <returns>DynamicQueryOptions applied IEnumerable instance,</returns>
        public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> currentSet, DynamicQueryOptions dynamicQueryOptions)
        {
            return ApplyFilters((IQueryable)currentSet, dynamicQueryOptions).OfType<T>();
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

                // Create the query parameter
                ParameterExpression param = Expression.Parameter(currentSet.ElementType, currentSet.ElementType.Name.ToLower());
                if (dynamicQueryOptions.Filters != null && dynamicQueryOptions.Filters.Count > 0)
                {
                    // Copy the array since we need to mutate it, we should avoid mutating the real list.
                    List<Filter> dqbFilters = dynamicQueryOptions.Filters.ToList();

                    // Since the expression is null at this point, we should create it with our first filter.
                    exp = BuildFilterExpression(param, dqbFilters.FirstOrDefault());
                    dqbFilters.RemoveAt(0); // Remove the first since it was added already.

                    // Append the rest
                    foreach (Filter item in dqbFilters)
                    {
                        exp = Expression.AndAlso(exp, BuildFilterExpression(param, item));
                    }
                }

                if (dynamicQueryOptions.SortOptions != null && dynamicQueryOptions.SortOptions.Count > 0)
                {
                    // OrderBy function requires a Func<T, TKey> since we don't have the TKey type plain System.Object should do the trick here.
                    bool orderedByDqb = false;
                    foreach (SortOption sortOption in dynamicQueryOptions.SortOptions)
                    {
                        Expression orderMember = Expression.Convert(ExtractMember(param, sortOption.PropertyName), typeof(object));
                        LambdaExpression orderExpression = Expression.Lambda(orderMember, param);
                        bool isOrdered =
                            currentSet.Expression.ToString().Contains($"{nameof(Enumerable.OrderBy)}")
                            || currentSet.Expression.ToString().Contains($"{nameof(Enumerable.OrderByDescending)}");

                        if (isOrdered && !orderedByDqb)
                        {
                            Match propertyOrderedBefore = Regex.Match(
                                currentSet.Expression.ToString(),
                                $@"{nameof(Enumerable.OrderBy)}(\([^\)]+{sortOption.PropertyName}\))");

                            if (propertyOrderedBefore.Length < 1)
                            {
                                propertyOrderedBefore = Regex.Match(
                                    currentSet.Expression.ToString(),
                                    $@"{nameof(Enumerable.OrderByDescending)}(\([^\)]+{sortOption.PropertyName}\))");
                            }

                            isOrdered = propertyOrderedBefore.Length < 1;
                        }

                        string methodName = isOrdered
                            ? nameof(Enumerable.ThenBy)
                            : nameof(Enumerable.OrderBy);

                        if (sortOption.SortingDirection == SortingDirection.Desc)
                        {
                            methodName = string.Concat(methodName, "Descending");
                        }

                        currentSet = currentSet.Provider.CreateQuery(
                            Expression.Call(typeof(Queryable),
                                            methodName,
                                            new Type[]
                                            {
                                                currentSet.ElementType,
                                                typeof(object)
                                            },
                                            currentSet.Expression,
                                            Expression.Quote(orderExpression)));

                        orderedByDqb = true;
                    }

                }

                if (exp != null)
                {
                    MethodCallExpression whereFilter = Expression.Call(
                        BuildLINQExtensionMethod(
                            nameof(Enumerable.Where),
                            genericElementType: currentSet.ElementType,
                            enumerableType: typeof(Queryable)),
                        currentSet.Expression,
                        Expression.Quote(Expression.Lambda(exp, param)));

                    currentSet = currentSet.Provider.CreateQuery(whereFilter);
                }

                if (dynamicQueryOptions.PaginationOption != null)
                {
                    MethodCallExpression skip = Expression.Call(
                        _skipFunction,
                        currentSet.Expression,
                        Expression.Constant(dynamicQueryOptions.PaginationOption.Offset));

                    currentSet = currentSet.Provider.CreateQuery(skip);
                    MethodCallExpression take = Expression.Call(
                        _takeFunction,
                        currentSet.Expression,
                        Expression.Constant(dynamicQueryOptions.PaginationOption.Count));

                    currentSet = currentSet.Provider.CreateQuery(take);
                    if (dynamicQueryOptions.PaginationOption.AssignDataSetCount)
                    {
                        dynamicQueryOptions.PaginationOption.DataSetCount = (int)_countFunction.Invoke(null, new[] { currentSet });
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
        /// <param name="resolveFromParameter">QueryString parameter that the Query was sent with.</param>
        /// <param name="opShortCodes">Custom operation shortcodes.</param>
        /// <returns>Parsed DynamicQueryOptions instance.</returns>
        public static DynamicQueryOptions ParseQueryOptions(string query, string resolveFromParameter = "", CustomOpCodes opShortCodes = null)
        {
            try
            {
                var dynamicQueryOptions = new DynamicQueryOptions();
                if (string.IsNullOrEmpty(query))
                {
                    return dynamicQueryOptions;
                }

                string decodedQuery;
                if (!string.IsNullOrEmpty(resolveFromParameter))
                {
                    NameValueCollection resolveParameterValues = HttpUtility.ParseQueryString(query);
                    string[] values = resolveParameterValues.GetValues(resolveFromParameter);
                    if (values == null || values.Length == 0)
                    {
                        throw new DynamicQueryException($"Couldn't resolve query from {resolveFromParameter}");
                    }

                    decodedQuery = HttpUtility.UrlDecode(values[0]);
                }
                else
                {
                    decodedQuery = HttpUtility.UrlDecode(query);
                }

                DynamicQueryOptions innerQueryOptions = null;
                const string innerMemberKey = "v=(";
                int indexOfInnerMemberKey = decodedQuery.IndexOf(innerMemberKey);
                if (indexOfInnerMemberKey != -1)
                {
                    indexOfInnerMemberKey += innerMemberKey.Length;
                    string innerQuery = decodedQuery.Substring(indexOfInnerMemberKey, decodedQuery.LastIndexOf(')') - indexOfInnerMemberKey);
                    innerQueryOptions = ParseQueryOptions(innerQuery);
                    decodedQuery = decodedQuery.Replace(innerQuery, string.Empty);
                }

                var defaultArrayValue = new List<string>().ToArray();
                NameValueCollection queryCollection = HttpUtility.ParseQueryString(decodedQuery);

                string[] operations = queryCollection.GetValues(OPERATION_PARAMETER_KEY) ?? defaultArrayValue;
                string[] parameterNames = queryCollection.GetValues(PARAMETER_NAME_KEY) ?? defaultArrayValue;
                string[] parameterValues = queryCollection.GetValues(PARAMETER_VALUE_KEY) ?? defaultArrayValue;
                string[] sortOptions = queryCollection.GetValues(SORT_OPTIONS_PARAMETER_KEY) ?? defaultArrayValue;
                string[] offsetOptions = queryCollection.GetValues(OFFSET_PARAMETER_KEY) ?? defaultArrayValue;
                string[] countOptions = queryCollection.GetValues(COUNT_PARAMETER_KEY) ?? defaultArrayValue;

                PopulateDynamicQueryOptions(
                    dynamicQueryOptions,
                    operations,
                    parameterNames,
                    parameterValues,
                    sortOptions,
                    offsetOptions,
                    countOptions,
                    opShortCodes ?? DefaultOpShortCodes,
                    innerQueryOptions);

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
        /// <param name="memberQueryOptions">TODO: Allan please remove this.</param>
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
                    FilterOperation foundOperation = default(FilterOperation);

                    // Check if we support this operation.
                    if (Enum.TryParse(operations[i], true, out FilterOperation parsedOperation))
                    {
                        foundOperation = parsedOperation;
                    }
                    else if (opShortCodes != null
                        && opShortCodes.Count > 0
                        && opShortCodes.TryGetValue(operations[i], out FilterOperation shortCodeOperation)) // Whoop maybe its a short code ?
                    {
                        foundOperation = shortCodeOperation;
                    }
                    else
                    {
                        throw new OperationNotSupportedException($"Invalid operation {operations[i]}");
                    }

                    var composedFilter = new Filter
                    {
                        Operator = foundOperation,
                        PropertyName = parameterNames[i],
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
                        string[] splittedParam = sortOption.Split(',');
                        SortingDirection direction = SortingDirection.Asc;
                        if (splittedParam.Length == 2)
                        {
                            // If we get an array of 2 we have a sorting direction, try to apply it.
                            if (!Enum.TryParse(splittedParam[1], true, out direction))
                            {
                                throw new InvalidDynamicQueryException("Invalid sorting direction");
                            }
                        }
                        else if (splittedParam.Length > 2) // If we get more than 2 results in the array, url must be wrong.
                        {
                            throw new InvalidDynamicQueryException("Invalid query structure. SortOption is misformed");
                        }

                        // Create the sorting option.
                        dynamicQueryOptions.SortOptions.Add(new SortOption
                        {
                            SortingDirection = direction,
                            PropertyName = splittedParam[0].Trim().TrimStart().TrimEnd()
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
        /// <returns>Built query expression.</returns>
        internal static Expression BuildFilterExpression(ParameterExpression param, Filter filter)
        {
            Expression parentMember = ExtractMember(param, filter.PropertyName);
            string stringFilterValue = filter.Value?.ToString();
            // We are handling In operations seperately which are basically a list of OR=EQUALS operation. We recursively handle this operation.
            if (filter.Operator == FilterOperation.In)
            {
                if (filter.Value == null)
                {
                    throw new DynamicQueryException("You can't pass type null to In. Pass null as a string instead.");
                }

                // Split all data into a list
                List<string> splittedValues = stringFilterValue.Split(',').ToList();
                var equalsFilter = new Filter
                {
                    Operator = FilterOperation.Equals,
                    PropertyName = filter.PropertyName,
                    Value = splittedValues.First()
                };

                // Create the expression with the first value.
                Expression builtInExpression = BuildFilterExpression(param, equalsFilter);
                splittedValues.RemoveAt(0); // Remove the first value

                // Create query for every splitted value and append them.
                foreach (var item in splittedValues)
                {
                    equalsFilter.Value = item;
                    builtInExpression = Expression.Or(builtInExpression, BuildFilterExpression(param, equalsFilter));
                }

                return builtInExpression;
            }

            // We should convert the data into its own type before we do any query building.
            object convertedValue = null;
            if (filter.Operator < FilterOperation.Any)
            {
                convertedValue = stringFilterValue != "null" ?
                                 TypeDescriptor.GetConverter(parentMember.Type).ConvertFromInvariantString(stringFilterValue) :
                                 null;
            }

            ConstantExpression constant = Expression.Constant(convertedValue);
            switch (filter.Operator)
            {
                case FilterOperation.Equals:
                    return Expression.Equal(parentMember, constant);

                case FilterOperation.NotEqual:
                    return Expression.NotEqual(parentMember, constant);

                case FilterOperation.Contains:
                    return Expression.Call(parentMember, _stringContainsMethod, constant);

                case FilterOperation.GreaterThan:
                    return Expression.GreaterThan(parentMember, constant);

                case FilterOperation.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(parentMember, constant);

                case FilterOperation.LessThan:
                    return Expression.LessThan(parentMember, constant);

                case FilterOperation.LessThanOrEqual:
                    return Expression.LessThanOrEqual(parentMember, constant);

                case FilterOperation.StartsWith:
                    return Expression.Call(parentMember, _stringStartsWithMethod, constant);

                case FilterOperation.EndsWith:
                    return Expression.Call(parentMember, _stringEndsWithMethod, constant);

                case FilterOperation.Any:
                case FilterOperation.All:
                    ParameterExpression memberParam = Expression.Parameter(
                        parentMember.Type.GenericTypeArguments[0],
                        parentMember.Type.GenericTypeArguments[0].Name);

                    MethodInfo requestedFunction = BuildLINQExtensionMethod(
                        filter.Operator.ToString(),
                        genericElementType: memberParam.Type,
                        enumerableType: typeof(Enumerable));

                    Expression builtMemberExpression = BuildFilterExpression(memberParam, (filter.Value as DynamicQueryOptions).Filters.First());

                    return Expression.Call(
                        requestedFunction,
                        Expression.PropertyOrField(param, filter.PropertyName),
                        Expression.Lambda(builtMemberExpression, memberParam));
                default:
                    return null;
            }
        }

        /// <summary>
        /// Constructs a query parameter Expression with the given PropertyName.
        /// Supports nested objects.
        /// </summary>
        /// <param name="param">Current parameter body.</param>
        /// <param name="propertyName">Parameter name to construct.</param>
        /// <returns>Constructed parameter name.</returns>
        internal static Expression ExtractMember(ParameterExpression param, string propertyName)
        {
            if (param == null || string.IsNullOrEmpty(propertyName))
            {
                throw new DynamicQueryException("Both parameter expression and propertyname are required");
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
                parentMember = Expression.PropertyOrField(parentMember, "Value");
            }

            return parentMember;
        }

        private static MethodInfo BuildLINQExtensionMethod(
            string functionName,
            int numberOfParameters = 2,
            int overloadNumber = 0,
            Type genericElementType = null,
            Type enumerableType = null)
        {
            return (enumerableType ?? typeof(Queryable))
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(x => x.Name == functionName && x.GetParameters().Count() == numberOfParameters)
            .ElementAt(overloadNumber)
            .MakeGenericMethod(new[] { genericElementType ?? typeof(object) });
        }

        private static bool AreCountsMatching(string[] operations, string[] parameterNames, string[] parameterValues)
        {
            return new int[] { operations.Length, parameterNames.Length, parameterValues.Length }.Distinct().Count() == 1;
        }
    }
}