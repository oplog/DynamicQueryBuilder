using Newtonsoft.Json;

namespace DynamicQueryBuilder.Models
{
    public sealed class PaginationOption
    {
        public int Count { get; set; }

        public int Offset { get; set; }

        public int DataSetCount { get; internal set; }

        [JsonIgnore]
        public bool AssignDataSetCount { get; set; }
    }
}
