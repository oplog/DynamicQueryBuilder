// <copyright file="PopulateDynamicQueryOptionsTests.cs" company="Oplog">
// Copyright (c) Oplog. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

using DynamicQueryBuilder.Models;
using DynamicQueryBuilder.Models.Enums;

using Xunit;

using static DynamicQueryBuilder.DynamicQueryBuilderExceptions;

namespace DynamicQueryBuilder.UnitTests.ExpressionBuilderTests
{
    public class PopulateDynamicQueryOptionsTests : TestBase
    {
        private readonly string[] _validOperations;
        private readonly string[] _validParameterNames;
        private readonly string[] _validParameterValues;
        private readonly string[] _validSortOptions;
        private readonly string[] _emptyArray = new List<string>().ToArray();
        private readonly string[] _arrayOfOne = new string[1];
        private readonly string[] _arrayOfTwo = new string[2];

        private readonly DynamicQueryOptions _reflectedObjectWithValidParameters;
        private readonly DynamicQueryOptions _reflectedObjectWithValidParametersAndSorted;

        public PopulateDynamicQueryOptionsTests()
        {
            Array.Fill(_arrayOfOne, string.Empty);
            Array.Fill(_arrayOfTwo, string.Empty);
            _validOperations = new string[] { "Equals", "Contains" };
            _validParameterNames = new string[] { "Name", "Name" };
            _validParameterValues = new string[] { "Test", "Te" };
            _validSortOptions = new string[] { "Name,desc" };

            string[] validSortOptionsSplitted = _validSortOptions[0].Split(',');

            var validListOfOperationsParsed = new List<Filter>
            {
                new Filter
                {
                    Value = _validParameterValues[0],
                    PropertyName = _validParameterNames[0],
                    Operator = (FilterOperation)Enum.Parse(typeof(FilterOperation), _validOperations[0])
                },
                new Filter
                {
                    Value = _validParameterValues[1],
                    PropertyName = _validParameterNames[1],
                    Operator = (FilterOperation)Enum.Parse(typeof(FilterOperation), _validOperations[1], true)
                }
            };

            _reflectedObjectWithValidParameters = new DynamicQueryOptions
            {
                Filters = validListOfOperationsParsed
            };

            _reflectedObjectWithValidParametersAndSorted = new DynamicQueryOptions
            {
                Filters = validListOfOperationsParsed
            };

            _reflectedObjectWithValidParametersAndSorted.SortOptions.Add(new SortOption
            {
                PropertyName = validSortOptionsSplitted[0],
                SortingDirection = (SortingDirection)Enum.Parse(typeof(SortingDirection), validSortOptionsSplitted[1], true)
            });
        }

        [Fact]
        public void NullDynamicQueryOptionsAndPropertyCountsMismatchShouldThrowDynamicQueryException()
        {
            var opts = new DynamicQueryOptions();
            Assert.Throws<DynamicQueryException>(() => ExpressionBuilder.PopulateDynamicQueryOptions(
                null, _arrayOfOne, _arrayOfOne, _arrayOfOne, _arrayOfOne, _emptyArray, _emptyArray));

            Assert.Throws<QueryTripletsMismatchException>(() => ExpressionBuilder.PopulateDynamicQueryOptions(
                opts, _arrayOfTwo, _arrayOfOne, _arrayOfOne, _arrayOfOne, _emptyArray, _emptyArray));

            Assert.Throws<QueryTripletsMismatchException>(() => ExpressionBuilder.PopulateDynamicQueryOptions(
                opts, _arrayOfOne, _arrayOfTwo, _arrayOfOne, _arrayOfOne, _emptyArray, _emptyArray));

            Assert.Throws<QueryTripletsMismatchException>(() => ExpressionBuilder.PopulateDynamicQueryOptions(
               opts, _arrayOfOne, _arrayOfOne, _arrayOfTwo, _arrayOfOne, _emptyArray, _emptyArray));
        }

        [Fact]
        public void InvalidOperationRequestedShouldThrowOperationNotSupportedException()
        {
            var opts = new DynamicQueryOptions();

            Assert.Throws<OperationNotSupportedException>(() => ExpressionBuilder.PopulateDynamicQueryOptions(
                opts, _arrayOfOne, _arrayOfOne, _arrayOfOne, _arrayOfOne, _arrayOfOne, _arrayOfOne));
        }

        [Fact]
        public void NullOrEmptySortOptions()
        {
            var opts = new DynamicQueryOptions();
            List<SortOption> currentSortOptsValue = opts.SortOptions;

            ExpressionBuilder.PopulateDynamicQueryOptions(
                opts, _validOperations, _validParameterNames, _validParameterValues, null, _emptyArray, _emptyArray);
            Assert.Equal(currentSortOptsValue, opts.SortOptions);

            ExpressionBuilder.PopulateDynamicQueryOptions(
                opts, _validOperations, _validParameterNames, _validParameterValues, _arrayOfOne, _emptyArray, _emptyArray);
            Assert.Equal(currentSortOptsValue, opts.SortOptions);
        }

        [Fact]
        public void InvalidSortOptionQueryDelimeterCountShouldThrowInvalidQueryException()
        {
            var opts = new DynamicQueryOptions();
            Assert.Throws<InvalidDynamicQueryException>(() =>
            {
                ExpressionBuilder.PopulateDynamicQueryOptions(
                    opts, _validOperations, _validParameterNames, _validParameterValues, new string[] { "Names,Desc,Invalid" }, _arrayOfOne, _arrayOfOne);
            });
        }

        [Fact]
        public void InvalidSortOptionDirectionShouldThrowInvalidQueryException()
        {
            var opts = new DynamicQueryOptions();
            Assert.Throws<InvalidDynamicQueryException>(() =>
            {
                ExpressionBuilder.PopulateDynamicQueryOptions(
                    opts, _validOperations, _validParameterNames, _validParameterValues, new string[] { "Names,Invalid" }, _arrayOfOne, _arrayOfOne);
            });
        }

        [Fact]
        public void ValidSortingOptionsShouldParseAllMembers()
        {
            var opts = new DynamicQueryOptions();
            ExpressionBuilder.PopulateDynamicQueryOptions(
                    opts, _validOperations, _validParameterNames, _validParameterValues, new string[] { "Name,Desc", "Age,Asc" }, _emptyArray, _emptyArray);

            Assert.Equal("Name", opts.SortOptions[0].PropertyName);
            Assert.Equal(SortingDirection.Desc, opts.SortOptions[0].SortingDirection);
            Assert.Equal("Age", opts.SortOptions[1].PropertyName);
            Assert.Equal(SortingDirection.Asc, opts.SortOptions[1].SortingDirection);
        }

        [Fact]
        public void ValidQueryShouldFillFilterAndSortingOptions()
        {
            var optsWithSorting = new DynamicQueryOptions();
            var optsWithoutSorting = new DynamicQueryOptions();

            ExpressionBuilder.PopulateDynamicQueryOptions(
                    optsWithoutSorting, _validOperations, _validParameterNames, _validParameterValues, null, _emptyArray, _emptyArray);

            ExpressionBuilder.PopulateDynamicQueryOptions(
                    optsWithSorting, _validOperations, _validParameterNames, _validParameterValues, _validSortOptions, _emptyArray, _emptyArray);

            Assert.True(AreObjectPropertiesMatching(optsWithoutSorting, _reflectedObjectWithValidParameters));
            Assert.True(AreObjectPropertiesMatching(optsWithSorting, _reflectedObjectWithValidParametersAndSorted));
        }

        [Fact]
        public void ShouldPopulateWithLogicalOperators()
        {
            var opts = new DynamicQueryOptions();
            ExpressionBuilder.PopulateDynamicQueryOptions(opts,
                                                          operations: new string[]
                                                          {
                                                              $"{FilterOperation.Equals}|{LogicalOperator.OrElse}",
                                                              $"{FilterOperation.Equals}"
                                                          },
                                                          parameterNames: _validParameterNames,
                                                          parameterValues: _validParameterValues,
                                                          sortOptions: _emptyArray,
                                                          offsetOptions: _emptyArray,
                                                          countOptions: _emptyArray);
            Assert.NotNull(opts.Filters);
            Assert.NotEmpty(opts.Filters);
            Assert.Equal(FilterOperation.Equals, opts.Filters[0].Operator);
            Assert.Equal(LogicalOperator.OrElse, opts.Filters[0].LogicalOperator);
            Assert.Equal(_validParameterNames[0], opts.Filters[0].PropertyName);
            Assert.Equal(_validParameterValues[0], opts.Filters[0].Value);

            Assert.Equal(FilterOperation.Equals, opts.Filters[1].Operator);
            Assert.Equal(LogicalOperator.AndAlso, opts.Filters[1].LogicalOperator);
            Assert.Equal(_validParameterNames[1], opts.Filters[1].PropertyName);
            Assert.Equal(_validParameterValues[1], opts.Filters[1].Value);
        }
    }
}
