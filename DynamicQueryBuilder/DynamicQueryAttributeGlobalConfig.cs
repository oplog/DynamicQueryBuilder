using DynamicQueryBuilder.Models.Enums;

namespace DynamicQueryBuilder
{
    public static class DynamicQueryAttributeGlobalConfig
    {
        internal static readonly int MaxCountSizeDefault = 100;
        internal static readonly bool IncludeDataSetCountToPaginationDefault = true;
        internal static readonly PaginationBehaviour ExceededPaginationCountBehaviourDefault = PaginationBehaviour.GetMax;

        public static int MaxCountSize { get; set; } = MaxCountSizeDefault;
        public static bool IncludeDataSetCountToPagination { get; set; } = IncludeDataSetCountToPaginationDefault;
        public static PaginationBehaviour ExceededPaginationCountBehaviour { get; set; } = ExceededPaginationCountBehaviourDefault;

        public static void LoadDefaultConfigs()
        {
            MaxCountSize = MaxCountSizeDefault;
            IncludeDataSetCountToPagination = IncludeDataSetCountToPaginationDefault;
            ExceededPaginationCountBehaviour = ExceededPaginationCountBehaviourDefault;
        }
    }
}