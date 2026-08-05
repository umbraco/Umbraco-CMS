using System.Text.RegularExpressions;

namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
/// These are the regexes used to generate the operation IDs.
/// </summary>
/// <remarks>
/// The benefit of this being partial with GeneratedRegex source generators is that they will be pre-compiled at startup
/// See: https://devblogs.microsoft.com/dotnet/regular-expression-improvements-in-dotnet-7/#source-generation for more info.
/// </remarks>
internal static partial class OperationIdRegexes
{
    [GeneratedRegex(".*?\\/v[0-9]+(?:\\.[0-9]+)?/")]
    public static partial Regex VersionPrefixRegex();

    [GeneratedRegex("[^A-Za-z0-9]")]
    public static partial Regex NonAlphanumericRegex();

    [GeneratedRegex("\\{(.*?)\\:?\\}")]
    public static partial Regex TemplatePlaceholdersRegex();

    [GeneratedRegex("[\\/\\-](\\w{1})")]
    public static partial Regex ToCamelCaseRegex();
}
