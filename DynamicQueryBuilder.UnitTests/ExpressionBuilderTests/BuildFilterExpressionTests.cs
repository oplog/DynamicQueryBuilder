// <copyright file="BuildFilterExpressionTests.cs" company="Oplog">
// Copyright (c) Oplog. All rights reserved.
// </copyright>

using DynamicQueryBuilder.Models;
using DynamicQueryBuilder.Models.Enums;
using DynamicQueryBuilder.UnitTests.TestData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;
using static DynamicQueryBuilder.DynamicQueryBuilderExceptions;

namespace DynamicQueryBuilder.UnitTests.ExpressionBuilderTests
{
    public class BuildFilterExpressionTests : TestBase
    {
        [Fact]
        public void ShouldThrowExceptionWhenInvalidFilterValueGiven()
        {
            Assert.Throws<DynamicQueryException>(() =>
            {
                ExpressionBuilder.BuildFilterExpression(
                    XParam, new Filter { Value = null, PropertyName = "Name", Operator = FilterOperation.In, CaseSensitive = true });
            });
        }

        [Fact]
        public void ShouldConvertNullAsStringToTypeAsString()
        {
            ExpressionBuilder.BuildFilterExpression(
                XParam, new Filter { Value = "null", PropertyName = "Name", Operator = FilterOperation.Equals, CaseSensitive = true });
        }

        [Fact]
        public void ShouldConvertInOperationToMultipleEquals()
        {
            // Check for case sensitive query.
            const string resultOfCaseSensitiveQuery = "(((x.Name == \"Te\") Or (x.Name == \" Test\")) Or (x.Name == \" Testx\"))";
            Expression caseSensitiveResult = ExpressionBuilder.BuildFilterExpression(
                XParam,
                new Filter { Value = "Te, Test, Testx", PropertyName = "Name", Operator = FilterOperation.In, CaseSensitive = true });
            
            Assert.Equal(caseSensitiveResult.ToString(), resultOfCaseSensitiveQuery);

            // Check for case Insensitive query.
            const string resultOfCaseInsensitiveQuery = "(((x.Name.ToLowerInvariant() == \"te\") Or (x.Name.ToLowerInvariant() == \" test\")) Or (x.Name.ToLowerInvariant() == \" testx\"))";
            Expression caseInsensitiveResult = ExpressionBuilder.BuildFilterExpression(
                    XParam,
                new Filter { Value = "Te, Test, Testx", PropertyName = "Name", Operator = FilterOperation.In },
                usesCaseInsensitiveSource: true);

            Assert.Equal(caseInsensitiveResult.ToString(), resultOfCaseInsensitiveQuery);
        }

        [Fact]
        public void ShouldHandleNullParameterValues()
        {
            const string resultOfQuery = "(x.Name == \"\")";
            Expression result = ExpressionBuilder.BuildFilterExpression(
                XParam,
                new Filter { Value = null, PropertyName = "Name", Operator = FilterOperation.Equals });

            Assert.Equal(result.ToString(), resultOfQuery);
        }

        [Theory]
        [MemberData(nameof(FilterTestData.FilterQueryData), MemberType = typeof(FilterTestData))]
        public void ShouldHandleEveryFilterOperationSupported(string resultQueryString, FilterOperation filterOperation, bool caseSensitive, string value, string propName)
        {
            Assert.Equal(resultQueryString, BuildQuery(filterOperation, value: value, propName: propName, caseSensitive: caseSensitive));
        }

        [Fact]
        public void ShouldReturnRullWhenNotSupportedOperationPassed() // don't know how this would happen tho.
        {
            Expression result = ExpressionBuilder.BuildFilterExpression(
            XParam, new Filter { Value = "test", PropertyName = "Name", Operator = (FilterOperation)999 });
            Assert.Null(result);
        }

        [Fact]
        public void ShouldHandleInnerPrimitiveCollectionMembers()
        {
            const string resultOfQuery = "(x == \"3\")";
            Expression result = ExpressionBuilder.BuildFilterExpression(
                Expression.Parameter(typeof(string), "x"),
                new Filter
                {
                    Value = "3",
                    PropertyName = "_",
                    Operator = FilterOperation.Equals
                });

            Assert.NotNull(result);
            Assert.Equal(result.ToString(), resultOfQuery);
        }

        private string BuildQuery(FilterOperation operation, string value = "Test", string propName = "Name", bool caseSensitive = true)
        {
            return ExpressionBuilder.BuildFilterExpression(
                        XParam,
                        new Filter { Value = value, PropertyName = propName, Operator = operation, CaseSensitive = caseSensitive }, usesCaseInsensitiveSource: !caseSensitive)?.ToString();
        }
    }
}
