using System;
using DynamicQueryBuilder.Interfaces;

namespace DynamicQueryBuilder.Models
{
    public sealed class HttpHeaderResolver : IQueryResolver
    {
        /// <summary>
        /// Creates a setting for DQB to resolve options from given HttpHeader.
        /// </summary>
        /// <param name="httpHeaderName">Http header name to decode.</param>
        /// <param name="decodeFunction">Decode function to decode an encoded query value.</param>
        public HttpHeaderResolver(string httpHeaderName, Func<string, string> decodeFunction = null)
        {
            HttpHeaderName = httpHeaderName;
            DecodeFunction = decodeFunction;
        }

        public string HttpHeaderName { get; internal set; }

        public Func<string, string> DecodeFunction { get; internal set; }
    }
}
