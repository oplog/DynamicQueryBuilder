﻿using System.Collections.Generic;
using System.Linq;

using DynamicQueryBuilder.IntegrationTests.EntityFramework.SampleModels;
using DynamicQueryBuilder.Models;
using DynamicQueryBuilder.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DynamicQueryBuilder.IntegrationTests.EntityFramework.Tests
{
    public class EFIntegrationTests : IDqbIntegrationTestScenario
    {
        private readonly EFContext _ctx = new EFContext();

        public EFIntegrationTests()
        {
            if (_ctx.Database.EnsureCreated())
            {
                _ctx.SaveChanges();
                _ctx.Users.AddRange(new User[]
                {
                    new User
                    {
                        Age = 1,
                        Name = "Test_1",
                        Orders = new Order[]
                        {
                            new Order
                            {
                                OrderRef = "REF_1",
                                ProductName = "GoodProd",
                                Quantity = 3,
                                Month = Months.April
                            },
                            new Order
                            {
                                OrderRef = "REF_2",
                                ProductName = "BetterProd",
                                Quantity = 5,
                                Month = Months.February
                            },
                            new Order
                            {
                                OrderRef = "REF_1",
                                ProductName = "AREUNUTZ?",
                                Quantity = 7,
                                Month = Months.December
                            }
                        }
                    },
                    new User
                    {
                        Age = 2,
                        Name = "Test_2",
                        Orders = new Order[]
                        {
                            new Order
                            {
                                OrderRef = "REF_3",
                                ProductName = "GoodProd",
                                Quantity = 1,
                                Month = Months.January
                            },
                            new Order
                            {
                                OrderRef = "REF_4",
                                ProductName = "BetterProd",
                                Quantity = 2,
                                Month = Months.May
                            },
                            new Order
                            {
                                OrderRef = "REF_5",
                                ProductName = "AREUNUTZ?",
                                Quantity = 11,
                                Month = Months.September
                            }
                        }
                    },
                    new User
                    {
                        Age = 3,
                        Name = "Test_3",
                        Orders = new Order[]
                        {
                            new Order
                            {
                                OrderRef = "REF_6",
                                ProductName = "GoodProd",
                                Quantity = 10,
                                Month = Months.January
                            },
                            new Order
                            {
                                OrderRef = "REF_7",
                                ProductName = "BetterProd",
                                Quantity = 150,
                                Month = Months.April
                            },
                            new Order
                            {
                                OrderRef = "REF_8",
                                ProductName = "AREUNUTZ?",
                                Quantity = 8,
                                Month = Months.April
                            }
                        }
                    },
                    new User
                    {
                        Age = 4,
                        Name = "Test_4",
                        Orders = new Order[]
                        {
                            new Order
                            {
                                OrderRef = "REF_9",
                                ProductName = "GoodProd",
                                Quantity = 12,
                                Month = Months.June
                            },
                            new Order
                            {
                                OrderRef = "REF_10",
                                ProductName = "BetterProd",
                                Quantity = 15,
                                Month = Months.April
                            },
                            new Order
                            {
                                OrderRef = "REF_11",
                                ProductName = "UniqueName",
                                Quantity = 22,
                                Month = Months.April
                            }
                        }
                    }
                });

                _ctx.SaveChanges();
            }
        }

        [Fact(DisplayName = "EF_FiltersShouldWork")]
        public void FiltersShouldWork()
        {
            var usersQueryable = _ctx.Users.AsQueryable();
            var ordersQueryable = _ctx.Orders.AsQueryable();
            var resultOfAgeTwo = usersQueryable.ApplyFilters(new DynamicQueryOptions
            {
                Filters = new List<Filter>
                {
                    new Filter
                    {
                        Operator = FilterOperation.GreaterThanOrEqual,
                        PropertyName = "Age",
                        Value = "2"
                    }
                },
                UsesCaseInsensitiveSource = true,
                IgnorePredefinedOrders = true
            }).ToList();

            var resultOfNameTestOne = usersQueryable.ApplyFilters(new DynamicQueryOptions
            {
                Filters = new List<Filter>
                {
                    new Filter
                    {
                        Operator = FilterOperation.Equals,
                        PropertyName = "Name",
                        Value = "Test_1",
                        CaseSensitive = true
                    }
                },
                IgnorePredefinedOrders = true
            }).ToList();

            var resultOfStringsComparisonTest = usersQueryable.ApplyFilters(new DynamicQueryOptions
            {
                Filters = new List<Filter>
                {
                    new Filter
                    {
                        Operator = FilterOperation.Any,
                        PropertyName = "Orders",
                        Value = new DynamicQueryOptions
                        {
                            Filters = new List<Filter>
                            {
                                new Filter
                                {
                                    Operator = FilterOperation.GreaterThan,
                                    PropertyName = "ProductName",
                                    Value = "Prod"
                                }
                            }
                        }
                    }
                }
            }).ToList();

            var resultOfEnumsComparisonTest = ordersQueryable.ApplyFilters(new DynamicQueryOptions
            {
                Filters = new List<Filter>
                {
                    new Filter
                    {
                        Operator = FilterOperation.GreaterThanOrEqual,
                        PropertyName = "Month",
                        Value = Months.April.ToString()
                    }
                }
            }).ToList();

            Assert.True(resultOfAgeTwo.Count == 3);
            Assert.True(resultOfNameTestOne.Count == 1);
            Assert.True(resultOfStringsComparisonTest.Count == 1);
            Assert.Equal("Test_4", resultOfStringsComparisonTest[0].Name);
            Assert.Equal(9, resultOfEnumsComparisonTest.Count);
        }

        [Fact(DisplayName = "EF_PaginationShouldWork")]
        public void PaginationShouldWork()
        {
            var result = _ctx.Users.AsQueryable().ApplyFilters(new DynamicQueryOptions
            {
                PaginationOption = new PaginationOption
                {
                    Count = 1,
                    Offset = 2
                },
                UsesCaseInsensitiveSource = true,
                IgnorePredefinedOrders = true
            }).ToList();

            Assert.Equal("Test_3", result[0].Name);
        }

        [Fact(DisplayName = "EF_SortingShouldWork")]
        public void SortingShouldWork()
        {
            var result = _ctx.Users.AsQueryable().ApplyFilters(new DynamicQueryOptions
            {
                SortOptions = new List<SortOption>
                {
                    new SortOption
                    {
                        PropertyName = "Age",
                        SortingDirection = SortingDirection.Desc
                    }
                },
                IgnorePredefinedOrders = true
            }).ToList();

            Assert.Equal("Test_4", result[0].Name);
            Assert.Equal("Test_3", result[1].Name);
            Assert.Equal("Test_2", result[2].Name);
            Assert.Equal("Test_1", result[3].Name);
        }

        [Fact(DisplayName = "EF_CompleteQueryShouldWork")]
        public void CompleteQueryShouldWork()
        {
            var result = _ctx.Users.AsQueryable().ApplyFilters(new DynamicQueryOptions
            {
                Filters = new List<Filter> // should return Test_2, Test_3, Test_4
                {
                    new Filter
                    {
                        PropertyName = "Age",
                        Operator = FilterOperation.GreaterThanOrEqual,
                        Value = "2"
                    }
                },
                SortOptions = new List<SortOption> // should sort as 4,3,2
                {
                    new SortOption
                    {
                        PropertyName = "Age",
                        SortingDirection = SortingDirection.Desc
                    }
                },
                PaginationOption = new PaginationOption // should skip Test_4 and take the rest
                {
                    Count = 10,
                    Offset = 1
                },
                IgnorePredefinedOrders = true
            }).ToList();

            Assert.True(result.Count == 2);
            Assert.Equal("Test_3", result[0].Name);
            Assert.Equal("Test_2", result[1].Name);
        }

        [Fact(DisplayName = "EF_MemberQueriesShouldWork")]
        public void MemberQueriesShouldWork()
        {
            var queryable = _ctx.Users.AsQueryable();
            var resultOfMemberAllQuantity = queryable.ApplyFilters(new DynamicQueryOptions
            {
                Filters = new List<Filter>
                {
                    new Filter
                    {
                        Operator = FilterOperation.All,
                        PropertyName = "Orders",
                        Value = new DynamicQueryOptions
                        {
                            Filters = new List<Filter>
                            {
                                new Filter
                                {
                                    Operator = FilterOperation.LessThanOrEqual,
                                    PropertyName = "Quantity",
                                    Value = "100"
                                }
                            }
                        }
                    }
                },
                IgnorePredefinedOrders = true
            }).ToList();

            var resultOfMemberAnyName = queryable.ApplyFilters(new DynamicQueryOptions
            {
                Filters = new List<Filter>
                {
                    new Filter
                    {
                        Operator = FilterOperation.Any,
                        PropertyName = "Orders",
                        Value = new DynamicQueryOptions
                        {
                            Filters = new List<Filter>
                            {
                                new Filter
                                {
                                    Operator = FilterOperation.Equals,
                                    PropertyName = "ProductName",
                                    Value = "UniqueName",
                                    CaseSensitive = true
                                }
                            }
                        }
                    }
                },
                IgnorePredefinedOrders = true
            }).ToList();

            Assert.True(resultOfMemberAnyName.Count == 1);
            Assert.Equal("Test_4", resultOfMemberAnyName[0].Name);

            Assert.True(resultOfMemberAllQuantity.Count == 3);
            Assert.Equal("Test_1", resultOfMemberAllQuantity[0].Name);
            Assert.Equal("Test_2", resultOfMemberAllQuantity[1].Name);
            Assert.Equal("Test_4", resultOfMemberAllQuantity[2].Name);
        }

        [Fact(DisplayName = "EF_CaseSensitivityShouldWork")]
        public void CaseSensitivityShouldWork()
        {
            var queryable = _ctx.Users.AsQueryable();
            var withResultFilterWithCSEnabled = new DynamicQueryOptions
            {
                Filters = new List<Filter>
                {
                    new Filter
                    {
                        CaseSensitive = true,
                        Operator = FilterOperation.Equals,
                        PropertyName = "Name",
                        Value = "test_1"
                    }
                },
                IgnorePredefinedOrders = true,
                UsesCaseInsensitiveSource = false
            };

            var withResultFilterWithCSDisabled = new DynamicQueryOptions
            {
                Filters = new List<Filter>
                {
                    new Filter
                    {
                        Operator = FilterOperation.Equals,
                        PropertyName = "Name",
                        Value = "Test_1",
                        CaseSensitive = false
                    }
                },
                IgnorePredefinedOrders = true,
                UsesCaseInsensitiveSource = true
            };

            var noResultFilterWithCSDisabledAndInvalidCase = new DynamicQueryOptions
            {
                Filters = new List<Filter>
                {
                    new Filter
                    {   // Default value of CS is false
                        Operator = FilterOperation.Equals,
                        PropertyName = "Name",
                        Value = "test_1"
                    }
                },
                IgnorePredefinedOrders = true
            };

            var stringsComparisonWithCaseSensitivityFilters = new DynamicQueryOptions
            {
                Filters = new List<Filter>
                {
                    new Filter
                    {
                        Operator = FilterOperation.Any,
                        PropertyName = "Orders",
                        Value = new DynamicQueryOptions
                        {
                            Filters = new List<Filter>
                            {
                                new Filter
                                {
                                    Operator = FilterOperation.LessThan,
                                    PropertyName = "ProductName",
                                    Value = "AreUNUTZ?",
                                    CaseSensitive = true
                                }
                            }
                        }
                    }
                },
                IgnorePredefinedOrders = true,
                UsesCaseInsensitiveSource = false
            };

            var resultOfwithResultFilterWithCSEnabled = queryable.ApplyFilters(withResultFilterWithCSEnabled).ToList();
            var resultOfwithResultFilterWithCSDisabled = queryable.ApplyFilters(withResultFilterWithCSDisabled).ToList();
            var resultOfnoResultFilterWithCSDisabledAndInvalidCase = queryable.ApplyFilters(noResultFilterWithCSDisabledAndInvalidCase).ToList();
            var stringsComparisonWithCaseSensitivity = queryable.ApplyFilters(stringsComparisonWithCaseSensitivityFilters).ToList();

            Assert.Empty(resultOfwithResultFilterWithCSEnabled);
            Assert.NotEmpty(resultOfwithResultFilterWithCSDisabled);
            Assert.Empty(resultOfnoResultFilterWithCSDisabledAndInvalidCase);
            Assert.True(stringsComparisonWithCaseSensitivity.Count == 0);
        }

        [Fact(DisplayName = "EF_LogicalOperatorsShouldWork")]
        public void LogicalOperatorsShouldWork()
        {
            var result = _ctx.Users
                .AsQueryable()
                .ApplyFilters(new DynamicQueryOptions
                {
                    Filters = new List<Filter>
                {
                    new Filter
                    {
                        LogicalOperator = LogicalOperator.OrElse,
                        PropertyName = nameof(User.Name),
                        Operator = FilterOperation.Equals,
                        Value = "Test_1"
                    },
                    new Filter
                    {
                        PropertyName = nameof(User.Orders),
                        Operator = FilterOperation.Any,
                        Value = new DynamicQueryOptions
                        {
                            Filters = new List<Filter>
                            {
                                new Filter
                                {
                                    Operator = FilterOperation.Equals,
                                    PropertyName = nameof(Order.OrderRef),
                                    Value = "REF_8"
                                }
                            }
                        }
                    }
                },
                    UsesCaseInsensitiveSource = true
                }).ToList();

            Assert.True(result.Count == 2);
            Assert.Equal("Test_1", result[0].Name);
            Assert.Equal("Test_3", result[1].Name);
        }
    }
}
