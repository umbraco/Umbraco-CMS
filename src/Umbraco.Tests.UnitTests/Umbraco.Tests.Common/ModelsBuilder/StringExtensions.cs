namespace Umbraco.Tests.ModelsBuilder
{
    public static class StringExtensions
    {
        public static string ClearLf(this string s)
        {
            return s.Replace("\r", "");
        }
    }
}
