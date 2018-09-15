// <copyright file="ApplyFiltersTests.cs" company="Oplog">
// Copyright (c) Oplog. All rights reserved.
// </copyright>

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

            public string Name { get; set; }
        }

        [Fact]
        public void ApplyFiltersShouldReturnGivenSetWhenOptionsAreNull()
        {
            IQueryable<TestModel> currentSet = CreateSampleSet();
            IQueryable<TestModel> returnedSet = currentSet.ApplyFilters(null).AsQueryable();
            Assert.Equal(returnedSet.Expression, currentSet.Expression);
        }

        [Fact]
        public void ApplyFiltersShouldReturnGivenSetWhenOptionsAreExistingButFiltersAreNull()
        {
            IQueryable<TestModel> currentSet = CreateSampleSet();
            IQueryable<TestModel> returnedSet = currentSet.ApplyFilters(new DynamicQueryOptions
            {
                Filters = null,
                SortOption = null
            }).AsQueryable();

            Assert.Equal(returnedSet.Expression, currentSet.Expression);
        }

        [Fact]
        public void ApplyFiltersShouldReturnGivenSetWhenOptionsAreExistingButFiltersAreEmpty()
        {
            IQueryable<TestModel> currentSet = CreateSampleSet();
            IQueryable<TestModel> returnedSet = currentSet.ApplyFilters(new DynamicQueryOptions
            {
                SortOption = null,
                Filters = new List<Filter>()
            }).AsQueryable();

            Assert.Equal(returnedSet.Expression, currentSet.Expression);
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
                       Operator = FilterOperation.StartsWith
                   }
                }
            };

            IQueryable<TestModel> returnedSet = currentSet.ApplyFilters(filters).AsQueryable();
            const string expectedQuery = "x => ((x.Age == 10) AndAlso x.Name.StartsWith(\"testOne\"))";
            var expressionMethodCall = returnedSet.Expression as MethodCallExpression;
            Assert.NotNull(expressionMethodCall);

            /* First member of the MethodCallExpression.Arguments is always the type that the expression was written for
             * and the second parameter is the actual expression string.
             */
            Assert.Equal(expectedQuery, expressionMethodCall.Arguments[1].ToString());
        }

        [Fact]
        public void Apply_filters_should_append_sorting_options_to_the_given_query()
        {
            IQueryable<TestModel> currentSet = CreateSampleSet();
            var ascendingSortingOptions = new DynamicQueryOptions
            {
                SortOption = new SortOption
                {
                    PropertyName = "Age",
                    SortingDirection = SortingDirection.Asc
                }
            };

            var descendingSortingOptions = new DynamicQueryOptions
            {
                SortOption = new SortOption
                {
                    PropertyName = "Age",
                    SortingDirection = SortingDirection.Desc
                }
            };

            var directionNotSpecifiedSortingOptions = new DynamicQueryOptions
            {
                SortOption = new SortOption { PropertyName = "Age" }
            };

            // Unfortunately, there is no better way to check the sorting method here that i could find.
            IQueryable<TestModel> resultOfAscendingOption = currentSet.ApplyFilters(ascendingSortingOptions).AsQueryable();
            Assert.Equal(resultOfAscendingOption.ElementAt(0).Age, currentSet.ElementAt(0).Age);
            Assert.Equal(resultOfAscendingOption.ElementAt(1).Age, currentSet.ElementAt(2).Age);
            Assert.Equal(resultOfAscendingOption.ElementAt(2).Age, currentSet.ElementAt(1).Age);

            IQueryable<TestModel> resultOfDescendingOption = currentSet.ApplyFilters(descendingSortingOptions).AsQueryable();
            Assert.Equal(resultOfDescendingOption.ElementAt(0).Age, currentSet.ElementAt(1).Age);
            Assert.Equal(resultOfDescendingOption.ElementAt(1).Age, currentSet.ElementAt(2).Age);
            Assert.Equal(resultOfDescendingOption.ElementAt(2).Age, currentSet.ElementAt(0).Age);

            IQueryable<TestModel> resultOfDirectionNotSpecified = currentSet.ApplyFilters(directionNotSpecifiedSortingOptions).AsQueryable();
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

            IEnumerable<TestModel> result = currentSet.ApplyFilters(optionsWithPagination);
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
                    SortingDirection = SortingDirection.Asc
                }
            });

            IQueryable<TestModel> result = currentSet.ApplyFilters(allQueryTypes);
            string expressionString = result.Expression.ToString();
            Assert.Contains($".OrderByDescending(x => Convert(x.{allQueryTypes.SortOptions[0].PropertyName}, Object))", expressionString);
            Assert.Contains($".ThenBy(x => Convert(x.{allQueryTypes.SortOptions[1].PropertyName}, Object))", expressionString);
            Assert.Contains($".Where(x => (x.{allQueryTypes.Filters[0].PropertyName} == {allQueryTypes.Filters[0].Value})", expressionString);
            Assert.Contains($".Skip({allQueryTypes.PaginationOption.Offset}).Take({allQueryTypes.PaginationOption.Offset})", expressionString);
        }

        [Fact]
        public void ApplyFiltersShouldAssignDataSetCountWhenAssignDataSetCountIsTrue()
        {
            IQueryable<TestModel> currentSet = CreateSampleSet();
            var optionsWithAssignDataCount = new DynamicQueryOptions
            {
                PaginationOption = new PaginationOption
                {
                    Count = 1,
                    Offset = 1,
                    AssignDataSetCount = true
                },
            };

            var optionsWithoutAssignDataCount = new DynamicQueryOptions
            {
                PaginationOption = new PaginationOption
                {
                    Count = 1,
                    Offset = 1,
                    AssignDataSetCount = false
                },
            };

            currentSet.ApplyFilters(optionsWithAssignDataCount);
            currentSet.ApplyFilters(optionsWithoutAssignDataCount);

            Assert.Equal(currentSet.Count(), optionsWithAssignDataCount.PaginationOption.DataSetCount);
            Assert.NotEqual(currentSet.Count(), optionsWithoutAssignDataCount.PaginationOption.DataSetCount);
        }

        private IQueryable<TestModel> CreateSampleSet()
        {
            return new List<TestModel>
            {
                new TestModel { Age = 10, Name = "testOne" },
                new TestModel { Age = 12, Name = "testThree" },
                new TestModel { Age = 11, Name = "testTwo" }
            }.AsQueryable();
        }
    }
}
