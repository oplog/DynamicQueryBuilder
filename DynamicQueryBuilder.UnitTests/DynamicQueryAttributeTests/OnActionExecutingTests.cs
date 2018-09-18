// <copyright file="OnActionExecutingTests.cs" company="Oplog">
// Copyright (c) Oplog. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
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

        private DynamicQueryOptions ExecuteAction(DynamicQueryAttribute parameterlessInstance, string query = null)
        {
            ActionExecutingContext context = CreateContext(query);
            parameterlessInstance.OnActionExecuting(context);
            return context.ActionArguments.First().Value as DynamicQueryOptions;
        }

        private ActionExecutingContext CreateContext(string query = null, CustomOpCodes customOpCodes = null)
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
            httpContext.Request.QueryString = new QueryString(query ?? DYNAMIC_QUERY_STRING);
            if (customOpCodes != null)
            {
                var serviceProvider = new ServiceCollection();
                serviceProvider.AddSingleton(customOpCodes);
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
