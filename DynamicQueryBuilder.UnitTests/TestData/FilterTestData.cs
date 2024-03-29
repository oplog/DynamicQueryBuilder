﻿using System.Collections.Generic;

using DynamicQueryBuilder.Models.Enums;

namespace DynamicQueryBuilder.UnitTests.TestData
{
    public class FilterTestData
    {
        public static IEnumerable<object[]> FilterQueryData
        {
            get
            {
                // Equals
                // For checking strings
                yield return new object[] { "(x.Name == \"Test\")", FilterOperation.Equals, true, "Test", "Name" };
                yield return new object[] { "(x.Name.ToLower() == \"test\".ToLower())", FilterOperation.Equals, false, "Test", "Name" };

                // For other types
                yield return new object[] { "(x.InnerMember.Age == 3)", FilterOperation.Equals, true, "3", "InnerMember.Age" };
                yield return new object[] { "(x.InnerMember.Age == 3)", FilterOperation.Equals, false, "3", "InnerMember.Age" };

                // For checking enums
                yield return new object[] { "(x.WeekDay == Monday)", FilterOperation.Equals, true, "Monday", "WeekDay" };
                yield return new object[] { "(x.WeekDay == Wednesday)", FilterOperation.Equals, false, "3", "WeekDay" };

                // Contains
                yield return new object[] { "x.Name.Contains(\"Test\")", FilterOperation.Contains, true, "Test", "Name" };
                yield return new object[] { "x.Name.ToLower().Contains(\"test\".ToLower())", FilterOperation.Contains, false, "Test", "Name" };

                // NotEquals
                // For checking strings
                yield return new object[] { "(x.Name != \"Test\")", FilterOperation.NotEqual, true, "Test", "Name" };
                yield return new object[] { "(x.Name.ToLower() != \"test\".ToLower())", FilterOperation.NotEqual, false, "Test", "Name" };

                // For other types
                yield return new object[] { "(x.InnerMember.Age != 3)", FilterOperation.NotEqual, true, "3", "InnerMember.Age" };
                yield return new object[] { "(x.InnerMember.Age != 3)", FilterOperation.NotEqual, false, "3", "InnerMember.Age" };

                // For checking enums
                yield return new object[] { "(x.WeekDay != Monday)", FilterOperation.NotEqual, true, "Monday", "WeekDay" };
                yield return new object[] { "(x.WeekDay != Wednesday)", FilterOperation.NotEqual, false, "3", "WeekDay" };

                // EndsWith
                yield return new object[] { "x.Name.EndsWith(\"Test\")", FilterOperation.EndsWith, true, "Test", "Name" };
                yield return new object[] { "x.Name.ToLower().EndsWith(\"test\".ToLower())", FilterOperation.EndsWith, false, "Test", "Name" };

                // StartsWith
                yield return new object[] { "x.Name.StartsWith(\"Test\")", FilterOperation.StartsWith, true, "Test", "Name" };
                yield return new object[] { "x.Name.ToLower().StartsWith(\"test\".ToLower())", FilterOperation.StartsWith, false, "Test", "Name" };

                // GreaterThan
                // For Non-string types
                yield return new object[] { "(x.InnerMember.Age > 3)", FilterOperation.GreaterThan, true, "3", "InnerMember.Age" };
                yield return new object[] { "(x.InnerMember.Age > 3)", FilterOperation.GreaterThan, false, "3", "InnerMember.Age" };
                //For strings
                yield return new object[] { "(x.Name.CompareTo(\"TestSix\") > 0)", FilterOperation.GreaterThan, true, "TestSix", "Name" };
                yield return new object[] { "(x.Name.ToLower().CompareTo(\"testsix\".ToLower()) > 0)", FilterOperation.GreaterThan, false, "TestSix", "Name" };
                // For checking enums
                yield return new object[] { "(Convert(x.WeekDay, UInt64).CompareTo(Convert(Monday, UInt64)) > 0)", FilterOperation.GreaterThan, true, "Monday", "WeekDay" };
                yield return new object[] { "(Convert(x.WeekDay, UInt64).CompareTo(Convert(Wednesday, UInt64)) > 0)", FilterOperation.GreaterThan, false, "3", "WeekDay" };

                // GreaterThanOrEqual
                // For Non-string types
                yield return new object[] { "(x.InnerMember.Age >= 3)", FilterOperation.GreaterThanOrEqual, true, "3", "InnerMember.Age" };
                yield return new object[] { "(x.InnerMember.Age >= 3)", FilterOperation.GreaterThanOrEqual, false, "3", "InnerMember.Age" };
                //For strings
                yield return new object[] { "(x.Name.CompareTo(\"TestSix\") >= 0)", FilterOperation.GreaterThanOrEqual, true, "TestSix", "Name" };
                yield return new object[] { "(x.Name.ToLower().CompareTo(\"testsix\".ToLower()) >= 0)", FilterOperation.GreaterThanOrEqual, false, "TestSix", "Name" };
                // For checking enums
                yield return new object[] { "(Convert(x.WeekDay, UInt64).CompareTo(Convert(Monday, UInt64)) >= 0)", FilterOperation.GreaterThanOrEqual, true, "Monday", "WeekDay" };
                yield return new object[] { "(Convert(x.WeekDay, UInt64).CompareTo(Convert(Wednesday, UInt64)) >= 0)", FilterOperation.GreaterThanOrEqual, false, "3", "WeekDay" };
                // For checking enums
                yield return new object[] { "(Convert(x.WeekDay, UInt64).CompareTo(Convert(Monday, UInt64)) >= 0)", FilterOperation.GreaterThanOrEqual, true, "Monday", "WeekDay" };
                yield return new object[] { "(Convert(x.WeekDay, UInt64).CompareTo(Convert(Wednesday, UInt64)) >= 0)", FilterOperation.GreaterThanOrEqual, false, "3", "WeekDay" };

                // LessThan
                // For Non-string types
                yield return new object[] { "(x.InnerMember.Age < 3)", FilterOperation.LessThan, true, "3", "InnerMember.Age" };
                yield return new object[] { "(x.InnerMember.Age < 3)", FilterOperation.LessThan, false, "3", "InnerMember.Age" };
                //For strings
                yield return new object[] { "(x.Name.CompareTo(\"TestSix\") < 0)", FilterOperation.LessThan, true, "TestSix", "Name" };
                yield return new object[] { "(x.Name.ToLower().CompareTo(\"testsix\".ToLower()) < 0)", FilterOperation.LessThan, false, "TestSix", "Name" };
                // For checking enums
                yield return new object[] { "(Convert(x.WeekDay, UInt64).CompareTo(Convert(Monday, UInt64)) < 0)", FilterOperation.LessThan, true, "Monday", "WeekDay" };
                yield return new object[] { "(Convert(x.WeekDay, UInt64).CompareTo(Convert(Wednesday, UInt64)) < 0)", FilterOperation.LessThan, false, "3", "WeekDay" };

                // LessThanOrEqual
                // For Non-string types
                yield return new object[] { "(x.InnerMember.Age <= 3)", FilterOperation.LessThanOrEqual, true, "3", "InnerMember.Age" };
                yield return new object[] { "(x.InnerMember.Age <= 3)", FilterOperation.LessThanOrEqual, false, "3", "InnerMember.Age" };
                //For strings
                yield return new object[] { "(x.Name.CompareTo(\"TestSix\") <= 0)", FilterOperation.LessThanOrEqual, true, "TestSix", "Name" };
                yield return new object[] { "(x.Name.ToLower().CompareTo(\"testsix\".ToLower()) <= 0)", FilterOperation.LessThanOrEqual, false, "TestSix", "Name" };
                // For checking enums
                yield return new object[] { "(Convert(x.WeekDay, UInt64).CompareTo(Convert(Monday, UInt64)) <= 0)", FilterOperation.LessThanOrEqual, true, "Monday", "WeekDay" };
                yield return new object[] { "(Convert(x.WeekDay, UInt64).CompareTo(Convert(Wednesday, UInt64)) <= 0)", FilterOperation.LessThanOrEqual, false, "3", "WeekDay" };
            }
        }
    }
}
