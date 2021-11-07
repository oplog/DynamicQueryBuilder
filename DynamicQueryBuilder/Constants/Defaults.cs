using DynamicQueryBuilder.Models;
using DynamicQueryBuilder.Models.Enums;

namespace DynamicQueryBuilder.Constants
{
    public class Defaults
    {
        public static readonly CustomOpCodes DefaultOpShortCodes = new CustomOpCodes
        {
            { "eq", FilterOperation.Equals },
            { "lt", FilterOperation.LessThan },
            { "cts", FilterOperation.Contains },
            { "ne", FilterOperation.NotEqual },
            { "ew", FilterOperation.EndsWith },
            { "sw", FilterOperation.StartsWith },
            { "gt", FilterOperation.GreaterThan },
            { "ltoe", FilterOperation.LessThanOrEqual },
            { "gtoe", FilterOperation.GreaterThanOrEqual },
            { "any", FilterOperation.Any },
            { "all", FilterOperation.All }
        };
    }
}
