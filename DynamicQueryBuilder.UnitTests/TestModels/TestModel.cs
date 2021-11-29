using System.Collections.Generic;

namespace DynamicQueryBuilder.UnitTests.TestModels
{
    internal class TestModel
    {
        public int Age { get; set; }

        public int? AgeN { get; set; }

        public string Name { get; set; }

        public Months Month { get; set; }

        public ICollection<string> InnerPrimitiveList { get; set; }

        public ICollection<InnerTestModel> InnerTestModels { get; set; }
    }
}
