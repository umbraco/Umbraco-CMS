namespace Umbraco.Cms.Tests.Common.Extensions;

public static class StringExtensions
{
    public static string StripNewLines(this string input) =>
        input.Replace("\r\n", string.Empty)
            .Replace("\n", string.Empty);

    public static string NormalizeNewLines(this string input) =>
        input.Replace("\r\n", "\n");
}
