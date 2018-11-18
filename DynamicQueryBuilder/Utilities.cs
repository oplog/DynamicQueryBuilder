using System;
using System.Linq;
using System.Reflection;

namespace DynamicQueryBuilder
{
    public static class StringUtils
    {
        public static string ClearSpaces(this string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                return input.Replace(" ", string.Empty);
            }

            return input;
        }
    }

    public static class LINQUtils
    {
        public static MethodInfo BuildLINQExtensionMethod(
            string functionName,
            int numberOfParameters = 2,
            int overloadNumber = 0,
            Type[] genericElementTypes = null,
            Type enumerableType = null)
        {
            return (enumerableType ?? typeof(Queryable))
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(x => x.Name == functionName && x.GetParameters().Count() == numberOfParameters)
            .ElementAt(overloadNumber)
            .MakeGenericMethod(genericElementTypes ?? new[] { typeof(object) });
        }
    }
}
