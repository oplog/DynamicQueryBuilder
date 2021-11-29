namespace DynamicQueryBuilder.IntegrationTests.EntityFramework.SampleModels
{
    public class Order : BaseModel
    {
        public string OrderRef { get; set; }

        public string ProductName { get; set; }

        public int Quantity { get; set; }

        public int UserId { get; set; }

        public Months Month { get; set; }
    }

    public enum Months : long
    {
        January,
        February,
        March,
        April,
        May,
        June,
        July,
        August,
        September,
        October,
        November,
        December
    }
}
