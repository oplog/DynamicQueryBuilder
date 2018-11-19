namespace DynamicQueryBuilder.Utils.Extensions
{
    public static class StringUtilsExtension
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
}
