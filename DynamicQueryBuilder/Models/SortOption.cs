using DynamicQueryBuilder.Models.Enums;

namespace DynamicQueryBuilder.Models
{
    public sealed class SortOption
    {
        public string PropertyName { get; set; }

        public SortingDirection SortingDirection { get; set; }

        public bool CaseSensitive { get; set; } = false;
    }
}
