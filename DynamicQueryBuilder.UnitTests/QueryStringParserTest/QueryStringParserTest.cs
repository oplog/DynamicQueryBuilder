using System.Linq;
using DynamicQueryBuilder.Utils;
using Xunit;

namespace DynamicQueryBuilder.UnitTests.QueryStringParserTest
{
    public class QueryStringParserTest
    {
        private const string QUERY = "o=In|AndAlso&p=referenceNumber&v=+90&o=Equals|AndAlso&p=integrationName&v=Shopify_303&s=orderCreatedAt,desc&offset=0&count=25";
        
        [Fact]
        public void IsParameterCountOk()
        {
            var parameterCount = QueryStringParser.GetAllParameterWithValue(QUERY).Count();

            Assert.Equal(2, parameterCount);
        }

        [Fact]
        public void IsParameterNamesOk()
        {
            var parameterWithValue = QueryStringParser.GetAllParameterWithValue(QUERY).ToList();

            Assert.Contains("referenceNumber", parameterWithValue.Select(e => e.Key));
            Assert.Contains("integrationName", parameterWithValue.Select(e => e.Key));
            Assert.DoesNotContain("IntegrationName", parameterWithValue.Select(e => e.Key));
        }

        [Fact]
        public void IsParameterValuesOk()
        {
            var parameterWithValue = QueryStringParser.GetAllParameterWithValue(QUERY).ToList();

            var referenceNumber = parameterWithValue.FirstOrDefault(e => e.Key.Equals("referenceNumber"));
            var integrationName = parameterWithValue.FirstOrDefault(e => e.Key.Equals("integrationName"));
            var nonExistKey = parameterWithValue.FirstOrDefault(e => e.Key.Equals("nonExistKey"));
            
            Assert.Equal("+90", referenceNumber?.Value);
            Assert.Equal("Shopify_303", integrationName?.Value);
            Assert.Null(nonExistKey?.Value);
        }
    }
}
