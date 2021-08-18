using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

namespace DynamicQueryBuilder.Utils
{
    internal static class QueryStringParser
    {
        public static IEnumerable<QueryStringParserResult> GetAllParameterWithValue(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return new QueryStringParserResult[0];
            }

            var len = query.Length;
            var parameter = string.Empty;

            var nameValues = new List<QueryStringParserResult>();
            var order = 0;

            for (var i = 0; i < len; i++)
            {
                if (query[i] == 'p' && query[i + 1] == '=')
                {
                    var pEndIndex = query.IndexOf("&", i + 2, StringComparison.Ordinal);
                    parameter = query[(i + 2)..pEndIndex];

                    i = pEndIndex;
                }

                if (query[i] == 'v' && query[i + 1] == '=')
                {
                    var pEndIndex = query.IndexOf("&", i + 2, StringComparison.Ordinal);

                    if (pEndIndex.Equals(-1))
                    {
                        nameValues.Add(new QueryStringParserResult(parameter, query[(i + 2)..], order));
                        break;
                    }

                    nameValues.Add(new QueryStringParserResult(parameter, query[(i + 2)..pEndIndex], order++));

                    i = pEndIndex;
                }
            }

            return nameValues;
        }
        internal static bool IsQueryStringEncoded(string query)
        {
            var decodedText = HttpUtility.HtmlDecode(query);
            var encodedText = HttpUtility.HtmlEncode(decodedText);

            return encodedText.Equals(query, StringComparison.OrdinalIgnoreCase);
        }

        internal static void ReplaceNameValueCollection(IEnumerable<QueryStringParserResult> queryStringParserResults, NameValueCollection queryCollection, string parameterKey)
        {
            foreach (var parserResult in queryStringParserResults)
            {
                var len = queryCollection.GetValues(parameterKey)?.Length;

                if (!len.HasValue) break;

                var values = queryCollection.GetValues(parameterKey);
                values?.SetValue(parserResult.Value, parserResult.Order);

                if (values == null)
                {
                    continue;
                }

                queryCollection.Remove(parameterKey);

                foreach (var value in values)
                {
                    queryCollection.Add(parameterKey, value);
                }
            }
        }
    }

    internal class QueryStringParserResult
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public int Order { get; set; }

        public QueryStringParserResult()
        {
            
        }
        public QueryStringParserResult(string key, string value, int order)
        {
            Key = key;
            Value = value;
            Order = order;
        }
    }
}