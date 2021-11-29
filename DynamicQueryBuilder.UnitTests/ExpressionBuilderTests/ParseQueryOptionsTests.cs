// <copyright file="ParseQueryOptionsTests.cs" company="Oplog">
// Copyright (c) Oplog. All rights reserved.
// </copyright>

using System.Collections.Generic;

using DynamicQueryBuilder.Models;
using DynamicQueryBuilder.Models.Enums;
using DynamicQueryBuilder.UnitTests.TestModels;
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
        public void ParseQueryOptionsShouldParseMultipleLevelMemberQueries()
        {
            const string firstLevelInnerCollectionMemberKey = "FirstLevelInnerCollectionMember";
            const string secondLevelInnerCollectionMemberKey = "SecondLevelInnerCollectionMember";
            const string secondLevelInnerMemberProperty = "SecondLevelInnerMemberProperty";
            const string secondLevelInnerMemberQueryValue = "3";

            string innerMemberQuery = $"o=Any&p={firstLevelInnerCollectionMemberKey}&v=(o=All&p={secondLevelInnerCollectionMemberKey}&v=(o=Equals&p={secondLevelInnerMemberProperty}&v={secondLevelInnerMemberQueryValue}))";

            var validOptions = new DynamicQueryOptions
            {
                Filters = new List<Filter>
                {
                    new Filter
                    {
                        Value = new DynamicQueryOptions
                        {
                            Filters = new List<Filter>
                            {
                                new Filter
                                {
                                    Operator = FilterOperation.All,
                                    PropertyName = secondLevelInnerCollectionMemberKey,
                                    Value = new DynamicQueryOptions
                                    {
                                        Filters = new List<Filter>
                                        {
                                            new Filter
                                            {
                                                Operator = FilterOperation.Equals,
                                                PropertyName = secondLevelInnerMemberProperty,
                                                Value = secondLevelInnerMemberQueryValue
                                            }
                                        }
                                     }
                                }
                            }
                        },
                        PropertyName = firstLevelInnerCollectionMemberKey,
                        Operator = FilterOperation.Any
                    }
                }
            };

            DynamicQueryOptions result = ExpressionBuilder.ParseQueryOptions(innerMemberQuery);
            Assert.True(AreObjectPropertiesMatching(validOptions, result));
        }

        [Fact]
        public void ParseQueryOptionsShouldParseMultipleLevelMemberQueriesWithPrimitiveTypes()
        {
            string innerMemberQuery = $"o=Any&p=InnerPrimitiveList&v=(o=eq&p=_&v=3)";
            var validOptions = new DynamicQueryOptions
            {
                Filters = new List<Filter>
                {
                    new Filter
                    {
                        Value = new DynamicQueryOptions
                        {
                            Filters = new List<Filter>
                            {
                                new Filter
                                {
                                    Operator = FilterOperation.Equals,
                                    PropertyName = "_",
                                    Value = "3"
                                }
                            }
                        },
                        PropertyName = nameof(TestModel.InnerPrimitiveList),
                        Operator = FilterOperation.Any
                    }
                }
            };

            DynamicQueryOptions result = ExpressionBuilder.ParseQueryOptions(innerMemberQuery);
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
        public void ParseQueryOptionsShouldThrowExceptionAndIncludeTheRequestedQueryInTheExceptionWhenQueryIsInvalid()
        {
            const string veryFaultyQueryString = "extremely=wrong&query=string";
            try
            {
                ExpressionBuilder.ParseQueryOptions(veryFaultyQueryString);
            }
            catch (DynamicQueryException ex)
            {
                Assert.Equal(ex.RequestedQuery, veryFaultyQueryString);
            }
        }

        [Fact]
        public void ParseQueryOptionsShouldNotClearSpacesInValueParameterValue()
        {
            const string finalParameterValue = "Movies and Music";
            string valueWithSpace = string.Concat(dynamicQueryWithParam.Replace("dqb=", string.Empty), "%20and%20Music");

            DynamicQueryOptions result = ExpressionBuilder.ParseQueryOptions(valueWithSpace);
            Assert.NotNull(result.Filters);
            Assert.NotEmpty(result.Filters);
            Assert.Equal(result.Filters[0].Value, finalParameterValue);
        }

        [Fact]
        public void ParseQueryOptionsShouldHandleLogicalOperators()
        {
            DynamicQueryOptions expectedResult = new DynamicQueryOptions
            {
                Filters = new List<Filter>()
                {
                    new Filter
                    {
                        LogicalOperator = LogicalOperator.OrElse,
                        Operator = FilterOperation.Equals,
                        PropertyName = "Fizz",
                        Value = "Buzz"
                    },
                    new Filter
                    {
                        LogicalOperator = LogicalOperator.AndAlso,
                        Operator = FilterOperation.Equals,
                        PropertyName = "Fizz",
                        Value = "NotBuzz"
                    }
                }
            };

            DynamicQueryOptions result =
                ExpressionBuilder.ParseQueryOptions("o=eq|orelse&p=Fizz&v=Buzz&o=eq&p=Fizz&v=NotBuzz");

            Assert.NotNull(result.Filters);
            Assert.NotEmpty(result.Filters);
            Assert.Equal(result.Filters[0].ToHTTPQueryString(), expectedResult.Filters[0].ToHTTPQueryString());
            Assert.Equal(result.Filters[1].ToHTTPQueryString(), expectedResult.Filters[1].ToHTTPQueryString());
        }
    }
}
