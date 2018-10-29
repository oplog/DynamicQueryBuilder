// <copyright file="ParseQueryOptionsTests.cs" company="Oplog">
// Copyright (c) Oplog. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Xunit;
using static DynamicQueryBuilder.DynamicQueryBuilderExceptions;

namespace DynamicQueryBuilder.UnitTests.ExpressionBuilderTests
{
    public class ParseQueryOptionsTests : TestBase
    {
        [Fact]
        public void ParseQueryOptionsShouldReturnEmptyOptionsWhenQueryIsNullOrEmpty()
        {
            var emptyDynamicQueryOptions = new DynamicQueryOptions();
            DynamicQueryOptions resultForNull = ExpressionBuilder.ParseQueryOptions(null);
            DynamicQueryOptions resultForEmpty = ExpressionBuilder.ParseQueryOptions(string.Empty);

            Assert.True(AreObjectPropertiesMatching(emptyDynamicQueryOptions, resultForNull));
            Assert.True(AreObjectPropertiesMatching(emptyDynamicQueryOptions, resultForEmpty));
        }

        [Fact]
        public void ParseQueryOptionsShouldEncapsulateUnhandledExceptionsWithItsOwnType()
        {
            var thrownException = Assert.Throws<DynamicQueryException>(() => { ExpressionBuilder.ParseQueryOptions("offset=text&count=text"); });
        }

        [Fact]
        public void ParseQueryOptionsShouldReturnParsedQueriesWhenQueryIsValid()
        {
            var validOptions = new DynamicQueryOptions
            {
                Filters = new List<Filter>
                {
                    new Filter
                    {
                        Value = "Value",
                        PropertyName = "Name",
                        Operator = FilterOperation.Equals
                    }
                },
                PaginationOption = new PaginationOption
                {
                    Count = 10,
                    Offset = 0
                }
            };

            validOptions.SortOptions.AddRange(new SortOption[]
            {
                new SortOption
                {
                    PropertyName = "Name",
                    SortingDirection = SortingDirection.Desc
                },
                new SortOption
                {
                    PropertyName = "Age",
                    SortingDirection = SortingDirection.Asc
                }
            });

            DynamicQueryOptions result = ExpressionBuilder.ParseQueryOptions(DYNAMIC_QUERY_STRING);
            Assert.True(AreObjectPropertiesMatching(validOptions, result));
        }

        [Fact]
        public void ParseQueryOptionsShouldHandleOperationShortCodes()
        {
            var validOptions = new DynamicQueryOptions
            {
                Filters = new List<Filter>
                {
                    new Filter
                    {
                        Value = "123",
                        PropertyName = "name",
                        Operator = FilterOperation.Equals
                    },
                    new Filter
                    {
                        Value = "21",
                        PropertyName = "age",
                        Operator = FilterOperation.GreaterThanOrEqual
                    }
                }
            };

            DynamicQueryOptions result = ExpressionBuilder.ParseQueryOptions("o=eq&p=name&v=123&o=gtoe&p=age&v=21");
            Assert.True(AreObjectPropertiesMatching(validOptions, result));
        }

        [Fact]
        public void ParseQueryOptionsShouldHandleOperationCustomShortCodes()
        {
            var validOptions = new DynamicQueryOptions
            {
                Filters = new List<Filter>
                {
                    new Filter
                    {
                        Value = "123",
                        PropertyName = "name",
                        Operator = FilterOperation.Equals
                    },
                    new Filter
                    {
                        Value = "21",
                        PropertyName = "age",
                        Operator = FilterOperation.GreaterThanOrEqual
                    }
                }
            };

            var customOpShortCodes = new CustomOpCodes
            {
                { "fizz", FilterOperation.Equals },
                { "buzz", FilterOperation.GreaterThanOrEqual }
            };

            DynamicQueryOptions result = ExpressionBuilder.ParseQueryOptions("o=fizz&p=name&v=123&o=buzz&p=age&v=21", opShortCodes: customOpShortCodes);
            Assert.True(AreObjectPropertiesMatching(validOptions, result));
        }

        [Fact]
        public void ParseQueryOptionsShouldParseQueryFromGivenParameterWhenProvided()
        {
            var validOptions = new DynamicQueryOptions
            {
                Filters = new List<Filter>
                {
                    new Filter
                    {
                        Value = "Movies",
                        PropertyName = "category",
                        Operator = FilterOperation.Equals
                    }
                }
            };

            DynamicQueryOptions result = ExpressionBuilder.ParseQueryOptions(dynamicQueryWithParam, DYNAMIC_QUERY_STRING_PARAM);
            Assert.True(AreObjectPropertiesMatching(validOptions, result));
        }

        [Fact]
        public void ParseQueryOptionsShouldThrowExceptionWhenInvalidOperationProvided()
        {
            Assert.Throws<DynamicQueryException>(() =>
            {
                ExpressionBuilder.ParseQueryOptions("o=invalid&p=operation&v=provided");
            });
        }

        [Fact]
        public void ParseQueryOptionsShouldThrowExceptionWhenResolveParamProvidedButNotPresent()
        {
            Assert.Throws<DynamicQueryException>(() =>
            {
                ExpressionBuilder.ParseQueryOptions(DYNAMIC_QUERY_STRING, DYNAMIC_QUERY_STRING_PARAM);
            });
        }
    }
}
