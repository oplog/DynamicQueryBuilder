﻿using DynamicQueryBuilder.IntegrationTests.EntityFramework.SampleModels;
using System.Collections.Generic;
using System.Linq;
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
                                Quantity = 3
                            },
                            new Order
                            {
                                OrderRef = "REF_2",
                                ProductName = "BetterProd",
                                Quantity = 5
                            },
                            new Order
                            {
                                OrderRef = "REF_1",
                                ProductName = "AREUNUTZ?",
                                Quantity = 7
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
                                Quantity = 1
                            },
                            new Order
                            {
                                OrderRef = "REF_4",
                                ProductName = "BetterProd",
                                Quantity = 2
                            },
                            new Order
                            {
                                OrderRef = "REF_5",
                                ProductName = "AREUNUTZ?",
                                Quantity = 11
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
                                Quantity = 10
                            },
                            new Order
                            {
                                OrderRef = "REF_7",
                                ProductName = "BetterProd",
                                Quantity = 150
                            },
                            new Order
                            {
                                OrderRef = "REF_8",
                                ProductName = "AREUNUTZ?",
                                Quantity = 8
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
                                Quantity = 12
                            },
                            new Order
                            {
                                OrderRef = "REF_10",
                                ProductName = "BetterProd",
                                Quantity = 15
                            },
                            new Order
                            {
                                OrderRef = "REF_11",
                                ProductName = "UniqueName",
                                Quantity = 22
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
            var queryable = _ctx.Users.AsQueryable();
            var resultOfAgeTwo = queryable.ApplyFilters(new DynamicQueryOptions
            {
                Filters = new List<Filter>
                {
                    new Filter
                    {
                        Operator = FilterOperation.GreaterThanOrEqual,
                         PropertyName = "Age",
                         Value = "2"
                    }
                }
            }).ToList();

            var resultOfNameTestOne = queryable.ApplyFilters(new DynamicQueryOptions
            {
                Filters = new List<Filter>
                {
                    new Filter
                    {
                        Operator = FilterOperation.Equals,
                        PropertyName = "Name",
                        Value = "Test_1"
                    }
                }
            }).ToList();

            Assert.True(resultOfAgeTwo.Count == 3);
            Assert.True(resultOfNameTestOne.Count == 1);
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
                }
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
                }
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
                }
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
                }
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
                                    Value = "UniqueName"
                                }
                            }
                        }
                    }
                }
            }).ToList();

            Assert.True(resultOfMemberAnyName.Count == 1);
            Assert.Equal("Test_4", resultOfMemberAnyName[0].Name);

            Assert.True(resultOfMemberAllQuantity.Count == 3);
            Assert.Equal("Test_1", resultOfMemberAllQuantity[0].Name);
            Assert.Equal("Test_2", resultOfMemberAllQuantity[1].Name);
            Assert.Equal("Test_4", resultOfMemberAllQuantity[2].Name);
        }

        [Fact]
        public void CaseSensitivityShouldWork()
        {
            var queryable = _ctx.Users.AsQueryable();
            var noResultFilterWithCSEnabled = new DynamicQueryOptions
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
                }
            };
            var withResultFilterWithCSEnabled = new DynamicQueryOptions
            {
                Filters = new List<Filter>
                {
                    new Filter 
                    {
                        CaseSensitive = true,
                        Operator = FilterOperation.Equals,
                        PropertyName = "Name",
                        Value = "Test_1"
                    }
                }
            };

            var filterWithCSDisabledOne = new DynamicQueryOptions
            {
                Filters = new List<Filter>
                {
                    new Filter
                    {   // Default value of CS is false
                        Operator = FilterOperation.Equals,
                        PropertyName = "Name",
                        Value = "test_1"
                    }
                }
            };
            var filterWithCSDisabledTwo = new DynamicQueryOptions
            {
                Filters = new List<Filter>
                {
                    new Filter
                    {   // Default value of CS is false
                        Operator = FilterOperation.Equals,
                        PropertyName = "Name",
                        Value = "Test_1"
                    }
                }
            };

            var resultOfNoResultFilterWithCSEnabled = queryable.ApplyFilters(noResultFilterWithCSEnabled).ToList();
            var resultOfWithResultFilterWithCSEnabled = queryable.ApplyFilters(withResultFilterWithCSEnabled).ToList();

            var resultOfWithCSDisabledOne = queryable.ApplyFilters(filterWithCSDisabledOne).ToList();
            var resultOfWithCSDisabledTwo = queryable.ApplyFilters(filterWithCSDisabledTwo).ToList();

            Assert.Empty(resultOfNoResultFilterWithCSEnabled);
            Assert.NotEmpty(resultOfWithResultFilterWithCSEnabled);
            Assert.Equal("Test_1", resultOfWithResultFilterWithCSEnabled[0].Name);

            Assert.NotEmpty(resultOfWithCSDisabledOne);
            Assert.NotEmpty(resultOfWithCSDisabledTwo);
            Assert.Equal("Test_1", resultOfWithCSDisabledOne[0].Name);
            Assert.Equal("Test_1", resultOfWithCSDisabledTwo[0].Name);
        }
    }
}
