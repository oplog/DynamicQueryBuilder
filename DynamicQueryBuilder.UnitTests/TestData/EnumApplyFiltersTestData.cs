using DynamicQueryBuilder.Models.Enums;
using DynamicQueryBuilder.UnitTests.TestModels;
using System.Collections.Generic;

namespace DynamicQueryBuilder.UnitTests.TestData
{
    public class EnumApplyFiltersTestData
    {
        public static IEnumerable<object[]> Data
        {
            get
            {
                // Equals
                yield return new object[] { Months.November, FilterOperation.Equals, new List<Months> { Months.November } };

                // NotEqual
                yield return new object[] { Months.November, FilterOperation.NotEqual, new List<Months> { Months.February, Months.April } };

                // GreaterThan
                yield return new object[] { Months.February, FilterOperation.GreaterThan, new List<Months> { Months.April, Months.November } };

                // GreaterThanOrEqual
                yield return new object[] { Months.February, FilterOperation.GreaterThanOrEqual, new List<Months> { Months.February, Months.April, Months.November } };

                // LessThan
                yield return new object[] { Months.April, FilterOperation.LessThan, new List<Months> { Months.February } };

                // LessThanOrEqual
                yield return new object[] { Months.April, FilterOperation.LessThanOrEqual, new List<Months> { Months.February, Months.April } };
            }
        }
    }
}
