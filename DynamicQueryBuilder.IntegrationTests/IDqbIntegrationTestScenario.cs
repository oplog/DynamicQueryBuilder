using Xunit;

namespace DynamicQueryBuilder.IntegrationTests
{
    public interface IDqbIntegrationTestScenario
    {
        [Fact]
        void FiltersShouldWork();


        [Fact]
        void PaginationShouldWork();


        [Fact]
        void SortingShouldWork();

        [Fact]
        void CompleteQueryShouldWork();

        [Fact]
        void MemberQueriesShouldWork();

        [Fact]
        void CaseSensitivityShouldWork();
    }
}
