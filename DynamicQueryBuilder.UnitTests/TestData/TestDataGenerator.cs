﻿using DynamicQueryBuilder.UnitTests.TestModels;
using System.Collections.Generic;
using System.Linq;

namespace DynamicQueryBuilder.UnitTests.TestData
{
    internal static class TestDataGenerator
    {
        public static IQueryable<TestModel> CreateSampleSet()
        {
            return new List<TestModel>
            {
                new TestModel
                {
                    Age = 10,
                    Name = "testOne",
                    NameN = "testOne",
                    Month = Months.February,
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
                    NameN = "testThree",
                    Month = Months.April,
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
                    NameN = "testTwo",
                    Month = Months.November,
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
                },
                new TestModel
                {
	                Age = 123,
	                Name = "testFour",
	                NameN = null,
	                Month = Months.May,
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
                }
            }.AsQueryable();
        }
    }
}
