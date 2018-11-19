namespace DynamicQueryBuilder.Models.Enums
{
    public enum FilterOperation
    {
        In,
        Equals,
        LessThan,
        Contains,
        NotEqual,
        EndsWith,
        StartsWith,
        GreaterThan,
        LessThanOrEqual,
        GreaterThanOrEqual,
        Any = 100, // Above 100 reserved for collection member operations
        All
    }
}
