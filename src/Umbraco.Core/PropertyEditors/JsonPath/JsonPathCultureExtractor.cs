using System.Text.RegularExpressions;

namespace Umbraco.Cms.Core.PropertyEditors.JsonPath;

/// <summary>
/// Extracts culture and segment information from JSONPath expressions for authorization purposes.
/// Parses filter expressions like "@.culture == 'en-US'" to extract the culture value.
/// </summary>
public class JsonPathCultureExtractor
{
    // Regex patterns for extracting culture and segment from JSONPath filter expressions
    // Matches: @.culture == 'value' or @.culture == "value"
    private static readonly Regex CulturePattern = new(@"@\.culture\s*==\s*['""]([^'""]+)['""]", RegexOptions.Compiled);

    // Matches: @.segment == 'value' or @.segment == "value"
    private static readonly Regex SegmentPattern = new(@"@\.segment\s*==\s*['""]([^'""]+)['""]", RegexOptions.Compiled);

    // Matches: @.culture == null
    private static readonly Regex CultureNullPattern = new(@"@\.culture\s*==\s*null\b", RegexOptions.Compiled);

    // Matches: @.segment == null
    private static readonly Regex SegmentNullPattern = new(@"@\.segment\s*==\s*null\b", RegexOptions.Compiled);

    /// <summary>
    /// Extracts all unique cultures referenced in a JSONPath expression.
    /// </summary>
    /// <param name="pathExpression">The JSONPath expression to parse.</param>
    /// <returns>A set of culture codes (e.g., "en-US", "da-DK"), or empty set if no cultures found.</returns>
    public ISet<string> ExtractCultures(string pathExpression)
    {
        if (string.IsNullOrWhiteSpace(pathExpression))
        {
            return new HashSet<string>();
        }

        var cultures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var matches = CulturePattern.Matches(pathExpression);
        foreach (Match match in matches)
        {
            if (match.Success && match.Groups.Count > 1)
            {
                cultures.Add(match.Groups[1].Value);
            }
        }

        return cultures;
    }

    /// <summary>
    /// Extracts all unique segments referenced in a JSONPath expression.
    /// </summary>
    /// <param name="pathExpression">The JSONPath expression to parse.</param>
    /// <returns>A set of segment names, or empty set if no segments found.</returns>
    public ISet<string> ExtractSegments(string pathExpression)
    {
        if (string.IsNullOrWhiteSpace(pathExpression))
        {
            return new HashSet<string>();
        }

        var segments = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var matches = SegmentPattern.Matches(pathExpression);
        foreach (Match match in matches)
        {
            if (match.Success && match.Groups.Count > 1)
            {
                segments.Add(match.Groups[1].Value);
            }
        }

        return segments;
    }

    /// <summary>
    /// Checks if a JSONPath expression explicitly filters for null culture (invariant).
    /// </summary>
    /// <param name="pathExpression">The JSONPath expression to parse.</param>
    /// <returns>True if the expression contains "@.culture == null", false otherwise.</returns>
    public bool ContainsInvariantCultureFilter(string pathExpression)
    {
        if (string.IsNullOrWhiteSpace(pathExpression))
        {
            return false;
        }

        return CultureNullPattern.IsMatch(pathExpression);
    }

    /// <summary>
    /// Checks if a JSONPath expression explicitly filters for null segment.
    /// </summary>
    /// <param name="pathExpression">The JSONPath expression to parse.</param>
    /// <returns>True if the expression contains "@.segment == null", false otherwise.</returns>
    public bool ContainsNullSegmentFilter(string pathExpression)
    {
        if (string.IsNullOrWhiteSpace(pathExpression))
        {
            return false;
        }

        return SegmentNullPattern.IsMatch(pathExpression);
    }

    /// <summary>
    /// Extracts all cultures from a collection of JSONPath expressions.
    /// </summary>
    /// <param name="pathExpressions">Collection of JSONPath expressions.</param>
    /// <returns>A set of all unique cultures across all expressions.</returns>
    public ISet<string> ExtractCulturesFromOperations(IEnumerable<string> pathExpressions)
    {
        if (pathExpressions == null)
        {
            return new HashSet<string>();
        }

        var allCultures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var path in pathExpressions)
        {
            var cultures = ExtractCultures(path);
            foreach (var culture in cultures)
            {
                allCultures.Add(culture);
            }
        }

        return allCultures;
    }

    /// <summary>
    /// Checks if any of the path expressions target invariant content (null culture).
    /// </summary>
    /// <param name="pathExpressions">Collection of JSONPath expressions.</param>
    /// <returns>True if any expression contains invariant culture filter.</returns>
    public bool AnyOperationTargetsInvariantCulture(IEnumerable<string> pathExpressions)
    {
        if (pathExpressions == null)
        {
            return false;
        }

        return pathExpressions.Any(ContainsInvariantCultureFilter);
    }
}
