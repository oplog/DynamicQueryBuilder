// <copyright file="Filter.cs" company="Oplog">
// Copyright (c) Oplog. All rights reserved.
// </copyright>

using Newtonsoft.Json;
using System.Collections.Generic;

namespace DynamicQueryBuilder
{
    public sealed class Filter
    {
        public string PropertyName { get; set; }

        public string Value { get; set; }

        public FilterOperation Operator { get; set; } = FilterOperation.Contains;
    }

    public sealed class SortOption
    {
        public string PropertyName { get; set; }

        public SortingDirection SortingDirection { get; set; }
    }

    public sealed class DynamicQueryOptions
    {
        public List<Filter> Filters { get; set; } = new List<Filter>();

        public SortOption SortOption { get; set; }

        public List<SortOption> SortOptions { get; set; } = new List<SortOption>();

        public PaginationOption PaginationOption { get; set; }
    }

    public sealed class PaginationOption
    {
        public int Count { get; set; }

        public int Offset { get; set; }

        public int DataSetCount { get; internal set; }

        [JsonIgnore]
        public bool AssignDataSetCount { get; set; }
    }

    public enum PaginationBehaviour
    {
        GetMax,
        Throw
    }

    public enum SortingDirection
    {
        Asc,
        Desc
    }

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
        GreaterThanOrEqual
    }
}
