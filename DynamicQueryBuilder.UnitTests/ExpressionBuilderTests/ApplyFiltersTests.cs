// <copyright file="ApplyFiltersTests.cs" company="Oplog">
// Copyright (c) Oplog. All rights reserved.
// </copyright>

using DynamicQueryBuilder.Models;
using DynamicQueryBuilder.Models.Enums;

using Moq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Xunit;

using static DynamicQueryBuilder.DynamicQueryBuilderExceptions;

namespace DynamicQueryBuilder.UnitTests.ExpressionBuilderTests
{
    public class ApplyFiltersTests
    {
        internal class TestModel
        {
            public int Age { get; set; }

            public int? AgeN { get; set; }

            public string Name { get; set; }

            public ICollection<string> InnerPrimitiveList { get; set; }

            public ICollection<InnerTestModel> InnerTestModels { get; set; }
        }

        internal class InnerTestModel
        {
            public string Role { get; set; }
        }

        [Fact]
        public void ApplyFiltersShouldReturnGivenSetWhenOptionsAreNull()
        {
            IQueryable<TestModel> currentSet = CreateSampleSet();
            IQueryable<TestModel> returnedSet = currentSet.ApplyFilters(null).AsQueryable();

            currentSet = currentSet.Cast<TestModel>();
            Assert.Equal(returnedSet.Expression.ToString(), currentSet.Expression.ToString());
        }

        [Fact]
        public void ApplyFilterShouldBeAbleToHandleMemberQueries_Any()
        {
            IQueryable<TestModel> returnedSet = PrepareForMemberQuery(FilterOperation.Any);
            Assert.Equal(2, returnedSet.Count());
            Assert.Equal("testOne", returnedSet.ElementAt(0).Name);
            Assert.Equal("testTwo", returnedSet.ElementAt(1).Name);
        }

        [Fact]
        public void ApplyFilterShouldBeAbleToHandleMemberQueries_All()
        {
            IQueryable<TestModel> returnedSet = PrepareForMemberQuery(FilterOperation.All);
            Assert.Equal(1, returnedSet.Count());
            Assert.Equal("testTwo", returnedSet.ElementAt(0).Name);
        }

        [Fact]
        public void ApplyFiltersShouldReturnGivenSetWhenOptionsAndFiltersAreNull()
        {
            IQueryable<TestModel> currentSet = CreateSampleSet();
            IQueryable<TestModel> returnedSet = currentSet.ApplyFilters(new DynamicQueryOptions
            {
                Filters = null,
                SortOptions = null
            }).AsQueryable();

            currentSet = currentSet.Cast<TestModel>();
            Assert.Equal(returnedSet.Expression.ToString(), currentSet.Expression.ToString());
        }

        [Fact]
        public void ApplyFiltersShouldReturnGivenSetWhenOptionsAndFiltersAreEmpty()
        {
            IQueryable<TestModel> currentSet = CreateSampleSet();
            IQueryable<TestModel> returnedSet = currentSet.ApplyFilters(new DynamicQueryOptions
            {
                SortOptions = new List<SortOption>(),
                Filters = new List<Filter>()
            }).AsQueryable();

            currentSet = currentSet.Cast<TestModel>();
            Assert.Equal(returnedSet.Expression.ToString(), currentSet.Expression.ToString());
        }

        [Fact]
        public void ApplyFiltersShouldAppendGivenFiltersToTheGivenSet()
        {
            IQueryable<TestModel> currentSet = CreateSampleSet();
            var filters = new DynamicQueryOptions
            {
                Filters = new List<Filter>
                {
                   new Filter
                   {
                       Value = "10",
                       PropertyName = "Age",
                       Operator = FilterOperation.Equals
                   },
                   new Filter
                   {
                       Value = "testOne",
                       PropertyName = "Name",
                       Operator = FilterOperation.StartsWith,
                       CaseSensitive = true
                   }
                }
            };

            IQueryable<TestModel> returnedSet = currentSet.ApplyFilters(filters).AsQueryable();
            string paramName = nameof(TestModel).ToLower();
            string expectedQuery = $"{paramName} => (({paramName}.Age == 10) AndAlso {paramName}.Name.ToLowerInvariant().StartsWith(\"testone\"))";
            var expressionMethodCall = returnedSet.Expression as MethodCallExpression;
            Assert.NotNull(expressionMethodCall);

            /* First member of the MethodCallExpression.Arguments is always the type that the expression was written for
             * and the second parameter is the actual expression string.
             */
            Assert.Equal(expectedQuery, ((MethodCallExpression)expressionMethodCall.Arguments[0]).Arguments[1].ToString());
        }

        [Fact]
        public void Apply_filters_should_append_sorting_options_to_the_given_query()
        {
            IQueryable<TestModel> currentSet = CreateSampleSet();
            var ascendingSortingOptions = new DynamicQueryOptions();
            ascendingSortingOptions.SortOptions.Add(new SortOption
            {
                PropertyName = "Age",
                SortingDirection = SortingDirection.Asc
            });

            var descendingSortingOptions = new DynamicQueryOptions();
            descendingSortingOptions.SortOptions.Add(new SortOption
            {
                PropertyName = "Age",
                SortingDirection = SortingDirection.Desc
            });

            var directionNotSpecifiedSortingOptions = new DynamicQueryOptions();
            directionNotSpecifiedSortingOptions.SortOptions.Add(new SortOption { PropertyName = "Age" });

            // Unfortunately, there is no better way to check the sorting method here that i could find.
            IQueryable<TestModel> resultOfAscendingOption = currentSet.ApplyFilters(ascendingSortingOptions);
            Assert.Equal(resultOfAscendingOption.ElementAt(0).Age, currentSet.ElementAt(0).Age);
            Assert.Equal(resultOfAscendingOption.ElementAt(1).Age, currentSet.ElementAt(2).Age);
            Assert.Equal(resultOfAscendingOption.ElementAt(2).Age, currentSet.ElementAt(1).Age);

            IQueryable<TestModel> resultOfDescendingOption = currentSet.ApplyFilters(descendingSortingOptions);
            Assert.Equal(resultOfDescendingOption.ElementAt(0).Age, currentSet.ElementAt(1).Age);
            Assert.Equal(resultOfDescendingOption.ElementAt(1).Age, currentSet.ElementAt(2).Age);
            Assert.Equal(resultOfDescendingOption.ElementAt(2).Age, currentSet.ElementAt(0).Age);

            IQueryable<TestModel> resultOfDirectionNotSpecified = currentSet.ApplyFilters(directionNotSpecifiedSortingOptions);
            Assert.Equal(resultOfDirectionNotSpecified.ElementAt(0).Age, currentSet.ElementAt(0).Age);
            Assert.Equal(resultOfDirectionNotSpecified.ElementAt(1).Age, currentSet.ElementAt(2).Age);
            Assert.Equal(resultOfDirectionNotSpecified.ElementAt(2).Age, currentSet.ElementAt(1).Age);
        }

        [Fact]
        public void ApplyFiltersShouldEncapsulateUnhandledExceptionsWithItsOwnType()
        {
            var mockFilterList = new Mock<IList<Filter>>();
            mockFilterList.Setup(x => x.Count).Returns(1);

            var mockOptions = new DynamicQueryOptions
            {
                Filters = mockFilterList.Object.ToList()
            };

            IQueryable<TestModel> currentSet = CreateSampleSet();
            var thrownException = Assert.Throws<DynamicQueryException>(() => { currentSet.ApplyFilters(mockOptions); });
            Assert.Equal(typeof(NullReferenceException), thrownException.InnerException.GetType());
        }

        [Fact]
        public void ApplyFiltersShouldPaginateWhenPaginationOptionsIsPresent()
        {
            IQueryable<TestModel> currentSet = CreateSampleSet();
            var optionsWithPagination = new DynamicQueryOptions
            {
                PaginationOption = new PaginationOption
                {
                    Count = 1,
                    Offset = 1
                }
            };

            IQueryable<TestModel> result = currentSet.ApplyFilters(optionsWithPagination);
            Assert.Equal(result.Count(), optionsWithPagination.PaginationOption.Count);
            Assert.Equal(result.ElementAt(0), currentSet.ElementAt(1));
        }

        [Fact]
        public void ApplyFiltersShouldApplyAllGivenQueryTypes()
        {
            IQueryable<TestModel> currentSet = CreateSampleSet();
            var allQueryTypes = new DynamicQueryOptions
            {
                PaginationOption = new PaginationOption
                {
                    Count = 1,
                    Offset = 1
                },
                Filters = new List<Filter> { new Filter { Operator = FilterOperation.Equals, PropertyName = "Age", Value = "5" } }
            };

            allQueryTypes.SortOptions.AddRange(new SortOption[]
            {
                new SortOption
                {
                    PropertyName = "Age",
                    SortingDirection = SortingDirection.Desc
                }, new SortOption
                {
                    PropertyName = "Name",
                    SortingDirection = SortingDirection.Asc,
                    CaseSensitive = true
                }
            });

            IQueryable<TestModel> result = currentSet.ApplyFilters(allQueryTypes);
            string expressionString = result.Expression.ToString();
            string paramName = nameof(TestModel).ToLower();
            Assert.Contains($".OrderByDescending({paramName} => {paramName}.{allQueryTypes.SortOptions[0].PropertyName})", expressionString);
            Assert.Contains($".ThenBy({paramName} => {paramName}.{allQueryTypes.SortOptions[1].PropertyName}, value(System.CultureAwareComparer))", expressionString);
            Assert.Contains($".Where({paramName} => ({paramName}.{allQueryTypes.Filters[0].PropertyName} == {allQueryTypes.Filters[0].Value})", expressionString);
            Assert.Contains($".Skip({allQueryTypes.PaginationOption.Offset}).Take({allQueryTypes.PaginationOption.Offset})", expressionString);
        }

        [Fact]
        public void ApplyFiltersShouldHandleComparisonsDifferentlyForStrings()
        {
            IQueryable<TestModel> currentSet = CreateSampleSet();
            var caseSensitiveFilters = new DynamicQueryOptions
            {
                Filters = new List<Filter>
                {
                   new Filter
                   {
                       Value = "testSix",
                       PropertyName = nameof(TestModel.Name),
                       Operator = FilterOperation.GreaterThan,
                       CaseSensitive = true
                   }
                }
            };

            List<TestModel> result = currentSet.ApplyFilters(caseSensitiveFilters).ToList();
            Assert.NotEmpty(result);
            Assert.Equal(currentSet.ElementAtOrDefault(1), result[0]);
            Assert.Equal(currentSet.ElementAtOrDefault(2), result[1]);

            var caseInsensitiveFilters = new DynamicQueryOptions
            {
                Filters = new List<Filter>
                {
                   new Filter
                   {
                       Value = "testSix",
                       PropertyName = nameof(TestModel.Name),
                       Operator = FilterOperation.GreaterThan,
                       CaseSensitive = false
                   }
                }
            };

            result = currentSet.ApplyFilters(caseInsensitiveFilters).ToList();
            Assert.NotEmpty(result);
            Assert.Equal(currentSet.ElementAtOrDefault(1), result[0]);
            Assert.Equal(currentSet.ElementAtOrDefault(2), result[1]);
        }

        [Fact]
        public void ApplyFiltersShouldHandlePrimitiveCollectionTypes()
        {
            IQueryable<TestModel> currentSet = CreateSampleSet();
            var filters = new DynamicQueryOptions
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
                },
                SortOptions = new List<SortOption>
                {
                    new SortOption
                    {
                        PropertyName = nameof(TestModel.Age)
                    }
                }
            };

            List<TestModel> result = currentSet.ApplyFilters(filters).ToList();
            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(currentSet.ElementAtOrDefault(0), result[0]);
            Assert.Equal(currentSet.ElementAtOrDefault(1), result[1]);
        }

        private IQueryable<TestModel> PrepareForMemberQuery(FilterOperation operation)
        {
            IQueryable<TestModel> currentSet = CreateSampleSet();
            IQueryable<TestModel> returnedSet = currentSet.ApplyFilters(new DynamicQueryOptions
            {
                Filters = new List<Filter>
                {
                    new Filter
                    {
                        Operator = operation,
                        PropertyName = "InnerTestModels",
                        Value = new DynamicQueryOptions
                        {
                            Filters = new List<Filter>
                            {
                                new Filter
                                {
                                    Operator = FilterOperation.Equals,
                                    PropertyName = "Role",
                                    Value = "Admin"
                                }
                            }
                        }
                    }
                }
            }).AsQueryable();

            return returnedSet;
        }

        private IQueryable<TestModel> CreateSampleSet()
        {
            return new List<TestModel>
            {
                new TestModel
                {
                    Age = 10,
                    Name = "testOne",
                    InnerTestModels = new List<InnerTestModel>
                    {
                        new InnerTestModel
                        {
                            Role = "Admin"
                        },
                        new InnerTestModel
                        {
                            Role = "User"
                        }
                    },
                    InnerPrimitiveList = new List<string>
                    {
                        "1",
                        "2",
                        "3"
                    }
                },
                new TestModel
                {
                    Age = 12,
                    Name = "testThree",
                    InnerTestModels = new List<InnerTestModel>
                    {
                        new InnerTestModel
                        {
                            Role = "User"
                        },
                        new InnerTestModel
                        {
                            Role = "User"
                        }
                    },
                    InnerPrimitiveList = new List<string>
                    {
                        "3",
                        "4",
                        "5"
                    }
                },
                new TestModel
                {
                    Age = 11,
                    Name = "testTwo",
                    InnerTestModels = new List<InnerTestModel>
                    {
                        new InnerTestModel
                        {
                            Role = "Admin"
                        },
                        new InnerTestModel
                        {
                            Role = "Admin"
                        }
                    },
                    InnerPrimitiveList = new List<string>
                    {
                        "7",
                        "7",
                        "7"
                    }
                }
            }.AsQueryable();
        }
    }
}
