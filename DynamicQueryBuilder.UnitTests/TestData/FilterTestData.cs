using System.Collections.Generic;

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
                yield return new object[] { "(x.Name.ToLowerInvariant() == \"test\".ToLowerInvariant())", FilterOperation.Equals, false, "Test", "Name" };

                // For other types
                yield return new object[] { "(x.InnerMember.Age == 3)", FilterOperation.Equals, true, "3", "InnerMember.Age" };
                yield return new object[] { "(x.InnerMember.Age == 3)", FilterOperation.Equals, false, "3", "InnerMember.Age" };

                // Contains
                yield return new object[] { "x.Name.Contains(\"Test\")", FilterOperation.Contains, true, "Test", "Name" };
                yield return new object[] { "x.Name.ToLowerInvariant().Contains(\"test\".ToLowerInvariant())", FilterOperation.Contains, false, "Test", "Name" };

                // NotIn
                yield return new object[] { "Not(x.Name.Contains(\"Test\"))", FilterOperation.NotIn, true, "Test", "Name" };
                yield return new object[] { "Not(x.Name.ToLowerInvariant().Contains(\"test\".ToLowerInvariant()))", FilterOperation.NotIn, false, "Test", "Name" };

                // NotEquals
                // For checking strings
                yield return new object[] { "(x.Name != \"Test\")", FilterOperation.NotEqual, true, "Test", "Name" };
                yield return new object[] { "(x.Name.ToLowerInvariant() != \"test\".ToLowerInvariant())", FilterOperation.NotEqual, false, "Test", "Name" };

                // For other types
                yield return new object[] { "(x.InnerMember.Age != 3)", FilterOperation.NotEqual, true, "3", "InnerMember.Age" };
                yield return new object[] { "(x.InnerMember.Age != 3)", FilterOperation.NotEqual, false, "3", "InnerMember.Age" };

                // EndsWith
                yield return new object[] { "x.Name.EndsWith(\"Test\")", FilterOperation.EndsWith, true, "Test", "Name" };
                yield return new object[] { "x.Name.ToLowerInvariant().EndsWith(\"test\".ToLowerInvariant())", FilterOperation.EndsWith, false, "Test", "Name" };

                // StartsWith
                yield return new object[] { "x.Name.StartsWith(\"Test\")", FilterOperation.StartsWith, true, "Test", "Name" };
                yield return new object[] { "x.Name.ToLowerInvariant().StartsWith(\"test\".ToLowerInvariant())", FilterOperation.StartsWith, false, "Test", "Name" };

                // GreaterThan
                // For Non-string types
                yield return new object[] { "(x.InnerMember.Age > 3)", FilterOperation.GreaterThan, true, "3", "InnerMember.Age" };
                yield return new object[] { "(x.InnerMember.Age > 3)", FilterOperation.GreaterThan, false, "3", "InnerMember.Age" };
                //For strings
                yield return new object[] { "(x.Name.CompareTo(\"TestSix\") > 0)", FilterOperation.GreaterThan, true, "TestSix", "Name" };
                yield return new object[] { "(x.Name.ToLowerInvariant().CompareTo(\"testsix\".ToLowerInvariant()) > 0)", FilterOperation.GreaterThan, false, "TestSix", "Name" };

                // GreaterThanOrEqual
                // For Non-string types
                yield return new object[] { "(x.InnerMember.Age >= 3)", FilterOperation.GreaterThanOrEqual, true, "3", "InnerMember.Age" };
                yield return new object[] { "(x.InnerMember.Age >= 3)", FilterOperation.GreaterThanOrEqual, false, "3", "InnerMember.Age" };
                //For strings
                yield return new object[] { "(x.Name.CompareTo(\"TestSix\") >= 0)", FilterOperation.GreaterThanOrEqual, true, "TestSix", "Name" };
                yield return new object[] { "(x.Name.ToLowerInvariant().CompareTo(\"testsix\".ToLowerInvariant()) >= 0)", FilterOperation.GreaterThanOrEqual, false, "TestSix", "Name" };

                // LessThan
                // For Non-string types
                yield return new object[] { "(x.InnerMember.Age < 3)", FilterOperation.LessThan, true, "3", "InnerMember.Age" };
                yield return new object[] { "(x.InnerMember.Age < 3)", FilterOperation.LessThan, false, "3", "InnerMember.Age" };
                //For strings
                yield return new object[] { "(x.Name.CompareTo(\"TestSix\") < 0)", FilterOperation.LessThan, true, "TestSix", "Name" };
                yield return new object[] { "(x.Name.ToLowerInvariant().CompareTo(\"testsix\".ToLowerInvariant()) < 0)", FilterOperation.LessThan, false, "TestSix", "Name" };

                // LessThanOrEqual
                // For Non-string types
                yield return new object[] { "(x.InnerMember.Age <= 3)", FilterOperation.LessThanOrEqual, true, "3", "InnerMember.Age" };
                yield return new object[] { "(x.InnerMember.Age <= 3)", FilterOperation.LessThanOrEqual, false, "3", "InnerMember.Age" };
                //For strings
                yield return new object[] { "(x.Name.CompareTo(\"TestSix\") <= 0)", FilterOperation.LessThanOrEqual, true, "TestSix", "Name" };
                yield return new object[] { "(x.Name.ToLowerInvariant().CompareTo(\"testsix\".ToLowerInvariant()) <= 0)", FilterOperation.LessThanOrEqual, false, "TestSix", "Name" };
            }
        }
    }
}
