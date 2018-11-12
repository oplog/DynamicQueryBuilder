using System.Collections.Generic;

namespace DynamicQueryBuilder.IntegrationTests.EntityFramework.SampleModels
{
    public class User : BaseModel
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public ICollection<Order> Orders { get; set; }
    }
}
