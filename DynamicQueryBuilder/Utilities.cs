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
}
