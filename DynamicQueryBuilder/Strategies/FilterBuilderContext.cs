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

        public Expression Build(Expression parentMember, Expression constant)
        {
            return _strategy.Build(parentMember, constant);
        }
    }
}
