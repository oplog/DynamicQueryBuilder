using System;
using DynamicQueryBuilder.Interfaces;

namespace DynamicQueryBuilder.Models
{
    public sealed class QueryStringResolver : IQueryResolver
    {
        /// <summary>
        /// Creates a setting for DQB to resolve options from given QueryStringParameter.
        /// </summary>
        /// <param name="queryStringParameterName">QueryString parameter name to resolve dqb options from.</param>
        /// <param name="decodeFunction">Decode function to decode an encoded query value.</param>
        public QueryStringResolver(string queryStringParameterName = null, Func<string, string> decodeFunction = null)
        {
            ResolveFrom = queryStringParameterName;
            DecodeFunction = decodeFunction;
        }

        public string ResolveFrom { get; internal set; }

        public Func<string, string> DecodeFunction { get; internal set; }
    }
}
