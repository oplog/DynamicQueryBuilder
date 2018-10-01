// <copyright file="DynamicQueryBuilderExceptions.cs" company="Oplog">
// Copyright (c) Oplog. All rights reserved.
// </copyright>

using System;

namespace DynamicQueryBuilder
{
    public static class DynamicQueryBuilderExceptions
    {
        /// <summary>
        /// Base DynamicQueryException
        /// </summary>
        public class DynamicQueryException : Exception
        {
            public DynamicQueryException(string message, string requestedQuery = null, Exception innerException = null)
                : base(message, innerException)
            {
                RequestedQuery = requestedQuery;
            }

            public string RequestedQuery { get; private set; }
        }

        /// <summary>
        /// Thrown when a generic query parsing related exception occurs.
        /// </summary>
        public sealed class InvalidDynamicQueryException : DynamicQueryException
        {
            public InvalidDynamicQueryException(string message, Exception innerException = null)
                : base(message, null, innerException) { }
        }

        /// <summary>
        /// Thrown when query builder detects a querystring that does not formed in correct triplets.
        /// </summary>
        public sealed class QueryTripletsMismatchException : DynamicQueryException
        {
            public QueryTripletsMismatchException(string message, Exception innerException = null)
                : base(message, null, innerException) { }
        }

        /// <summary>
        /// Thrown when query builder detects an operation in querystring that is not supported.
        /// </summary>
        public sealed class OperationNotSupportedException : DynamicQueryException
        {
            public OperationNotSupportedException(string message, Exception innerException = null)
                : base(message, null, innerException) { }
        }

        /// <summary>
        /// Thrown when query builder detects a pagination request that exceeds the max result set count.
        /// </summary>
        public sealed class MaximumResultSetExceededException : DynamicQueryException
        {
            public MaximumResultSetExceededException(string message, Exception innerException = null)
                : base(message, null, innerException) { }
        }
    }
}
