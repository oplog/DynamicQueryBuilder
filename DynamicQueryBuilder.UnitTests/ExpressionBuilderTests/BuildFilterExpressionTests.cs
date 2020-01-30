// <copyright file="BuildFilterExpressionTests.cs" company="Oplog">
// Copyright (c) Oplog. All rights reserved.
// </copyright>

using DynamicQueryBuilder.Models;
using DynamicQueryBuilder.Models.Enums;
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

        [Fact]
        public void ShouldHandleEveryFilterOperationSupported()
        {
            const string resultOfEquals = "(x.Name.ToLowerInvariant() == \"test\")";
            const string resultOfContains = "x.Name.ToLowerInvariant().Contains(\"test\")";
            const string resultOfNotEquals = "(x.Name.ToLowerInvariant() != \"test\")";
            const string resultOfEndsWith = "x.Name.ToLowerInvariant().EndsWith(\"test\")";
            const string resultOfEStartsWith = "x.Name.ToLowerInvariant().StartsWith(\"test\")";
            const string resultOfGreaterThan = "(x.InnerMember.Age > 3)";
            const string resultOfLessThan = "(x.InnerMember.Age < 3)";
            const string resultOfLessThanOrEquals = "(x.InnerMember.Age <= 3)";
            const string resultOfGreaterThanOrEquals = "(x.InnerMember.Age >= 3)";

            const string resultOfStringLessThanCaseSensitive = "(x.Name.CompareTo(\"testSix\") < 0)";
            const string resultOfStringLessThanOrEqualsCaseSensitive = "(x.Name.CompareTo(\"testSix\") <= 0)";
            const string resultOfStringGreaterThanCaseSensitive = "(x.Name.CompareTo(\"testSix\") > 0)";
            const string resultOfStringGreaterThanOrEqualsCaseSensitive = "(x.Name.CompareTo(\"testSix\") >= 0)";

            const string resultOfStringLessThanCaseInsensitive = "(x.Name.ToLowerInvariant().CompareTo(\"testsix\".ToLowerInvariant()) < 0)";
            const string resultOfStringLessThanOrEqualsCaseInsensitive = "(x.Name.ToLowerInvariant().CompareTo(\"testsix\".ToLowerInvariant()) <= 0)";
            const string resultOfStringGreaterThanCaseInsensitive = "(x.Name.ToLowerInvariant().CompareTo(\"testsix\".ToLowerInvariant()) > 0)";
            const string resultOfStringGreaterThanOrEqualsCaseInsensitive = "(x.Name.ToLowerInvariant().CompareTo(\"testsix\".ToLowerInvariant()) >= 0)";


            Expression result = Expression.Empty();
            List<string> operations = Enum.GetNames(typeof(FilterOperation)).ToList();
            operations.Remove(nameof(FilterOperation.In)); // We handle this differently

            foreach (string item in operations)
            {
                var innerOperation = (FilterOperation)Enum.Parse(typeof(FilterOperation), item);
                switch (innerOperation)
                {
                    case FilterOperation.Equals:
                        Assert.Equal(resultOfEquals, BuildQuery(FilterOperation.Equals, caseSensitive: false));
                        break;
                    case FilterOperation.Contains:
                        Assert.Equal(resultOfContains, BuildQuery(FilterOperation.Contains, caseSensitive: false));
                        break;
                    case FilterOperation.NotEqual:
                        Assert.Equal(resultOfNotEquals, BuildQuery(FilterOperation.NotEqual, caseSensitive: false));
                        break;
                    case FilterOperation.EndsWith:
                        Assert.Equal(resultOfEndsWith, BuildQuery(FilterOperation.EndsWith, caseSensitive: false));
                        break;
                    case FilterOperation.StartsWith:
                        Assert.Equal(resultOfEStartsWith, BuildQuery(FilterOperation.StartsWith, caseSensitive: false));
                        break;
                    case FilterOperation.LessThan:
                        Assert.Equal(resultOfLessThan, BuildQuery(FilterOperation.LessThan, "3", "InnerMember.Age"));
                        Assert.Equal(resultOfStringLessThanCaseSensitive, BuildQuery(FilterOperation.LessThan, "testSix", "Name"));
                        Assert.Equal(resultOfStringLessThanCaseInsensitive, BuildQuery(FilterOperation.LessThan, "testSix", "Name", caseSensitive: false));
                        break;
                    case FilterOperation.GreaterThan:
                        Assert.Equal(resultOfGreaterThan, BuildQuery(FilterOperation.GreaterThan, "3", "InnerMember.Age"));
                        Assert.Equal(resultOfStringGreaterThanCaseSensitive, BuildQuery(FilterOperation.GreaterThan, "testSix", "Name"));
                        Assert.Equal(resultOfStringGreaterThanCaseInsensitive, BuildQuery(FilterOperation.GreaterThan, "testSix", "Name", caseSensitive: false));
                        break;
                    case FilterOperation.LessThanOrEqual:
                        Assert.Equal(resultOfLessThanOrEquals, BuildQuery(FilterOperation.LessThanOrEqual, "3", "InnerMember.Age"));
                        Assert.Equal(resultOfStringLessThanOrEqualsCaseSensitive, BuildQuery(FilterOperation.LessThanOrEqual, "testSix", "Name"));
                        Assert.Equal(resultOfStringLessThanOrEqualsCaseInsensitive, BuildQuery(FilterOperation.LessThanOrEqual, "testSix", "Name", caseSensitive: false));
                        break;
                    case FilterOperation.GreaterThanOrEqual:
                        Assert.Equal(resultOfGreaterThanOrEquals, BuildQuery(FilterOperation.GreaterThanOrEqual, "3", "InnerMember.Age"));
                        Assert.Equal(resultOfStringGreaterThanOrEqualsCaseSensitive, BuildQuery(FilterOperation.GreaterThanOrEqual, "testSix", "Name"));
                        Assert.Equal(resultOfStringGreaterThanOrEqualsCaseInsensitive, BuildQuery(FilterOperation.GreaterThanOrEqual, "testSix", "Name", caseSensitive: false));
                        break;
                    default:
                        Assert.Null(BuildQuery((FilterOperation)999));
                        break;
                }
            }
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

        private string BuildQuery(FilterOperation operation, string value = "test", string propName = "Name", bool caseSensitive = true)
        {
            return ExpressionBuilder.BuildFilterExpression(
                        XParam,
                        new Filter { Value = value, PropertyName = propName, Operator = operation, CaseSensitive = caseSensitive }, caseSensitive)?.ToString();
        }
    }
}
