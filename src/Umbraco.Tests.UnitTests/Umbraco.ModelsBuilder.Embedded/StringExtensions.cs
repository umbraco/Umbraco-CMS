namespace Umbraco.Tests.UnitTests.Umbraco.ModelsBuilder.Embedded
{
    public static class StringExtensions
    {
        public static string ClearLf(this string s)
        {
            return s.Replace("\r", "");
        }
    }
}
