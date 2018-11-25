﻿// <copyright file="OnActionExecutingTests.cs" company="Oplog">
// Copyright (c) Oplog. All rights reserved.
// </copyright>

using DynamicQueryBuilder.Models;
using DynamicQueryBuilder.Models.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using static DynamicQueryBuilder.DynamicQueryBuilderExceptions;

namespace DynamicQueryBuilder.UnitTests.DynamicQueryAttributeTests
{
    public class OnActionExecutingTests : TestBase
    {
        private const int _defaultMaxCountSize = 100;
        private const bool _defaultIncludeDataSetCount = true;
        private const PaginationBehaviour _defaultPaginationBehaviour = PaginationBehaviour.GetMax;

        [Fact]
        public void OnActionExecutingShouldModifyDynamicQueryOptionsActionParameterWhenItsPresent()
        {
            ActionExecutingContext actionContext = CreateContext();

            DynamicQueryOptions expectedResult = ExpressionBuilder.ParseQueryOptions(DYNAMIC_QUERY_STRING);

            new DynamicQueryAttribute().OnActionExecuting(actionContext);

            var result = actionContext.ActionArguments[nameof(DynamicQueryOptions)] as DynamicQueryOptions;
            Assert.True(AreObjectPropertiesMatching(expectedResult, result));
        }

        [Fact]
        public void ShouldSetGivenCtorValues()
        {
            int changedMaxCountSize = 200;
            bool changedIncludedataSetCount = false;
            PaginationBehaviour changedPaginationBehaviour = PaginationBehaviour.Throw;

            var parameterlessInstance = new DynamicQueryAttribute();
            var parameteredInstance = new DynamicQueryAttribute(200, false, PaginationBehaviour.Throw);

            Assert.Equal(parameterlessInstance._maxCountSize, _defaultMaxCountSize);
            Assert.Equal(parameterlessInstance._includeDataSetCountToPagination, _defaultIncludeDataSetCount);
            Assert.Equal(parameterlessInstance._exceededPaginationCountBehaviour, _defaultPaginationBehaviour);

            Assert.Equal(parameteredInstance._maxCountSize, changedMaxCountSize);
            Assert.Equal(parameteredInstance._includeDataSetCountToPagination, changedIncludedataSetCount);
            Assert.Equal(parameteredInstance._exceededPaginationCountBehaviour, changedPaginationBehaviour);
        }

        [Fact]
        public void ShouldSetGivenAssignDataSetCountWhenOptionsNotNull()
        {
            bool changedIncludeDataSetCount = !_defaultIncludeDataSetCount;
            var parameterlessInstance = new DynamicQueryAttribute(includeDataSetCountToPagination: changedIncludeDataSetCount);
            DynamicQueryOptions executedOptions = ExecuteAction(parameterlessInstance);
            Assert.Equal(executedOptions.PaginationOption.AssignDataSetCount, changedIncludeDataSetCount);
        }

        [Fact]
        public void ShouldSetCountToMaxWhenCountExceedsAndOptionsWereSetToGetMaxOrDefault()
        {
            var behaviourSetInstance = new DynamicQueryAttribute(exceededPaginationCountBehaviour: PaginationBehaviour.GetMax);
            var defaultBehaviourInstance = new DynamicQueryAttribute();
            string queryExceedingMaxCount = DYNAMIC_QUERY_STRING.Replace("count=10", $"count={_defaultMaxCountSize + 1}");

            DynamicQueryOptions executedOptionsWithSetOptions = ExecuteAction(behaviourSetInstance, queryExceedingMaxCount);
            DynamicQueryOptions executedOptionsWithDefaultOptions = ExecuteAction(defaultBehaviourInstance, queryExceedingMaxCount);

            Assert.Equal(executedOptionsWithSetOptions.PaginationOption.Count, _defaultMaxCountSize);
            Assert.Equal(executedOptionsWithDefaultOptions.PaginationOption.Count, _defaultMaxCountSize);
        }

        [Fact]
        public void ShouldSetCountToMaxWhenCountExceedsAndOptionsWereSetToThrow()
        {
            var behaviourSetInstance = new DynamicQueryAttribute(exceededPaginationCountBehaviour: PaginationBehaviour.Throw);
            string queryExceedingMaxCount = DYNAMIC_QUERY_STRING.Replace("count=10", $"count={_defaultMaxCountSize + 1}");
            Assert.Throws<MaximumResultSetExceededException>(() => { ExecuteAction(behaviourSetInstance, queryExceedingMaxCount); });
        }

        [Fact]
        public void ShouldSetCountToOneWhenRequestedCountLessThanOrEqualToZero()
        {
            var attributeInstance = new DynamicQueryAttribute();
            string queryWithCountZero = DYNAMIC_QUERY_STRING.Replace("count=10", "count=0");
            string queryWithCountNegative = DYNAMIC_QUERY_STRING.Replace("count=10", "count=-1");
            DynamicQueryOptions executedOptionsWithCountZero = ExecuteAction(attributeInstance, queryWithCountZero);
            DynamicQueryOptions executedOptionsWithCountNegative = ExecuteAction(attributeInstance, queryWithCountNegative);
            Assert.Equal(1, executedOptionsWithCountZero.PaginationOption.Count);
            Assert.Equal(1, executedOptionsWithCountNegative.PaginationOption.Count);
        }

        [Fact]
        public void ShouldSetOffsetToZeroWhenLessThanZero()
        {
            var attributeInstance = new DynamicQueryAttribute();
            string queryWithOffsetNegative = DYNAMIC_QUERY_STRING.Replace("offset=0", "offset=-1");
            DynamicQueryOptions executedOptionsOffsetNegative = ExecuteAction(attributeInstance, queryWithOffsetNegative);
            Assert.Equal(0, executedOptionsOffsetNegative.PaginationOption.Offset);
        }

        [Fact]
        public void ShouldParseDataSetCountWhenIncludeDataSetCountAssignedAndThereIsNoPaginationOption()
        {
            var attributeInstance = new DynamicQueryAttribute();
            string queryWithOffsetNegative = DYNAMIC_QUERY_STRING.Replace("offset=0", string.Empty).Replace("count=10", string.Empty);
            DynamicQueryOptions executedOptionsOffsetNegative = ExecuteAction(attributeInstance, queryWithOffsetNegative);
            Assert.NotNull(executedOptionsOffsetNegative.PaginationOption);
            Assert.True(executedOptionsOffsetNegative.PaginationOption.AssignDataSetCount);
        }

        [Fact]
        public void ShouldDetectQueryStringResolverQueryResolutionOptionWhenItsProvided()
        {
            var instance = new DynamicQueryAttribute();
            var resultWithoutEncoding = ExecuteAction(
                instance,
                dynamicQueryWithParam,
                new DynamicQueryBuilderSettings
                {
                    QueryOptionsResolver = new QueryStringResolver(DYNAMIC_QUERY_STRING_PARAM)
                });

            var resultWithEncoding = ExecuteAction(
                instance,
                dynamicQueryWithParamValueEncoded,
                new DynamicQueryBuilderSettings
                {
                    QueryOptionsResolver = new QueryStringResolver(DYNAMIC_QUERY_STRING_PARAM, (qstring) => Encoding.UTF8.GetString(Convert.FromBase64String(qstring)))
                });

            Assert.NotNull(resultWithoutEncoding.Filters);
            Assert.NotEmpty(resultWithoutEncoding.Filters);

            Assert.NotNull(resultWithEncoding.Filters);
            Assert.NotEmpty(resultWithEncoding.Filters);
        }

        [Fact]
        public void ShouldDetectHttpHeaderResolverQueryResolutionOptionWhenItsProvided()
        {
            var instance = new DynamicQueryAttribute();
            string onlyQuery = dynamicQueryWithParam.Split($"?{DYNAMIC_QUERY_STRING_PARAM}=")[1];
            string onlyQueryEncoded = dynamicQueryWithParamValueEncoded.Split($"?{DYNAMIC_QUERY_STRING_PARAM}=")[1];
            var resultWithoutEncoding = ExecuteAction(
                instance,
                onlyQuery,
                new DynamicQueryBuilderSettings
                {
                    QueryOptionsResolver = new HttpHeaderResolver(DYNAMIC_QUERY_STRING_PARAM)
                },
                true);

            var resultWithEncoding = ExecuteAction(
                instance,
                onlyQueryEncoded,
                new DynamicQueryBuilderSettings
                {
                    QueryOptionsResolver = new HttpHeaderResolver(DYNAMIC_QUERY_STRING_PARAM, (qstring) => Encoding.UTF8.GetString(Convert.FromBase64String(qstring)))
                },
                true);

            Assert.NotNull(resultWithoutEncoding.Filters);
            Assert.NotEmpty(resultWithoutEncoding.Filters);

            Assert.NotNull(resultWithEncoding.Filters);
            Assert.NotEmpty(resultWithEncoding.Filters);
        }

        private DynamicQueryOptions ExecuteAction(
            DynamicQueryAttribute attributeInstance,
            string query = null,
            DynamicQueryBuilderSettings settings = null,
            bool queryOnHeader = false)
        {
            ActionExecutingContext context = CreateContext(query, settings, queryOnHeader);
            attributeInstance.OnActionExecuting(context);
            return context.ActionArguments.First().Value as DynamicQueryOptions;
        }

        private ActionExecutingContext CreateContext(
            string query = null,
            DynamicQueryBuilderSettings opts = null,
            bool queryOnHeader = false)
        {
            var actionDescriptor = new ActionDescriptor
            {
                Parameters = new List<ParameterDescriptor>()
                {
                    new ParameterDescriptor
                    {
                        Name = nameof(DynamicQueryOptions),
                        ParameterType = typeof(DynamicQueryOptions),
                        BindingInfo = new BindingInfo()
                    }
                }
            };

            HttpContext httpContext = new DefaultHttpContext();
            if (queryOnHeader)
            {
                var hhResolver = opts.QueryOptionsResolver as HttpHeaderResolver;
                httpContext.Request.Headers.Add(
                    DYNAMIC_QUERY_STRING_PARAM,
                    query ?? DYNAMIC_QUERY_STRING);
            }
            else
            {
                httpContext.Request.QueryString = new QueryString(query ?? DYNAMIC_QUERY_STRING);
            }

            if (opts != null)
            {
                var serviceProvider = new ServiceCollection();
                serviceProvider.AddSingleton(opts);
                httpContext.RequestServices = serviceProvider.BuildServiceProvider(true);
            }

            var actionContext = new ActionExecutingContext(
                new ActionContext(httpContext, new RouteData(), actionDescriptor),
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                null);

            return actionContext;
        }
    }
}
