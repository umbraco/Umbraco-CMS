using System.Text.RegularExpressions;
using Umbraco.Extensions;

namespace Umbraco.Search.Extensions;

public static class UmbracoSearchExtensions
{
    /// <summary>
    ///     Matches a culture iso name suffix
    /// </summary>
    /// <remarks>
    ///     myFieldName_en-us will match the "en-us"
    /// </remarks>
    public static readonly Regex _cultureIsoCodeFieldNameMatchExpression = new(
        "^(?<FieldName>[_\\w]+)_(?<CultureName>[a-z]{2,3}(-[a-z0-9]{2,4})?)$",
        RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    // TODO: We need a public method here to just match a field name against CultureIsoCodeFieldNameMatchExpression

    /// <summary>
    ///     Returns all index fields that are culture specific (suffixed)
    /// </summary>
    /// <param name="index"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public static IEnumerable<string> GetCultureFields(this IUmbracoIndex index, string culture)
    {
        IEnumerable<string> allFields = index.GetFieldNames();

        var results = new List<string>();
        foreach (var field in allFields)
        {
            Match match = _cultureIsoCodeFieldNameMatchExpression.Match(field);
            if (match.Success && culture.InvariantEquals(match.Groups["CultureName"].Value))
            {
                results.Add(field);
            }
        }

        return results;
    }

}
