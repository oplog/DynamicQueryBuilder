using DynamicQueryBuilder.Utils;
using System;
using System.Linq;
using System.Reflection;

namespace DynamicQueryBuilder.Extensions
{
    internal static class ExtensionMethods
    {
        public static readonly MethodInfo CountFunction = LINQUtils.BuildLINQExtensionMethod(nameof(Enumerable.Count), numberOfParameters: 1);

        public static readonly MethodInfo SkipFunction = LINQUtils.BuildLINQExtensionMethod(nameof(Enumerable.Skip));

        public static readonly MethodInfo TakeFunction = LINQUtils.BuildLINQExtensionMethod(nameof(Enumerable.Take));

        public static readonly MethodInfo StringContainsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

        public static readonly MethodInfo StringEndsWithMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });

        public static readonly MethodInfo StringStartsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });

        public static readonly MethodInfo ToLowerMethod = typeof(string).GetMethod("ToLower", Array.Empty<Type>());

        public static readonly MethodInfo CompareTo = typeof(string).GetMethod("CompareTo", new[] { typeof(string) });
    }
}
