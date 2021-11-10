using System.Linq.Expressions;

namespace DynamicQueryBuilder.Strategies
{
    public class FilterBuilderContext
    {
        private IFilterBuilderStrategy _strategy;

        public FilterBuilderContext() { }

        public FilterBuilderContext(IFilterBuilderStrategy strategy)
        {
            this._strategy = strategy;
        }

        public void SetStrategy(IFilterBuilderStrategy strategy)
        {
            this._strategy = strategy;
        }

        public Expression Build(Expression parentMember, Expression constant, bool useCaseInsensitiveComparison)
        {
            if (parentMember.Type == typeof(string) && useCaseInsensitiveComparison)
            {
                parentMember = StrategyUtils.ToLowerIfCaseInsensitive(parentMember, useCaseInsensitiveComparison);
                constant = StrategyUtils.ToLowerIfCaseInsensitive(constant, useCaseInsensitiveComparison);
            }

            return _strategy.Build(parentMember, constant);
        }
    }
}
