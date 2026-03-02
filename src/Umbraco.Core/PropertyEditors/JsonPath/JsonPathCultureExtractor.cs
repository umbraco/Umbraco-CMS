using Umbraco.Cms.Core.PropertyEditors.Patching;

namespace Umbraco.Cms.Core.PropertyEditors.JsonPath;

/// <summary>
/// Extracts culture and segment information from patch path expressions for authorization purposes.
/// Delegates to <see cref="PatchPathParser"/> for path parsing.
/// </summary>
public class JsonPathCultureExtractor
{
    /// <summary>
    /// Extracts all unique cultures referenced in a path expression.
    /// </summary>
    /// <param name="pathExpression">The patch path expression to parse.</param>
    /// <returns>A set of culture codes (e.g., "en-US", "da-DK"), or empty set if no cultures found.</returns>
    public ISet<string> ExtractCultures(string pathExpression)
        => PatchPathParser.ExtractCultures(pathExpression);

    /// <summary>
    /// Extracts all unique segments referenced in a path expression.
    /// </summary>
    /// <param name="pathExpression">The patch path expression to parse.</param>
    /// <returns>A set of segment names, or empty set if no segments found.</returns>
    public ISet<string> ExtractSegments(string pathExpression)
        => PatchPathParser.ExtractSegments(pathExpression);

    /// <summary>
    /// Checks if a path expression explicitly targets invariant content (culture=null).
    /// </summary>
    /// <param name="pathExpression">The path expression to parse.</param>
    /// <returns>True if the expression targets invariant culture, false otherwise.</returns>
    public bool ContainsInvariantCultureFilter(string pathExpression)
        => PatchPathParser.TargetsInvariantCulture(pathExpression);

    /// <summary>
    /// Checks if a path expression explicitly targets null segment.
    /// </summary>
    /// <param name="pathExpression">The path expression to parse.</param>
    /// <returns>True if the expression targets null segment, false otherwise.</returns>
    public bool ContainsNullSegmentFilter(string pathExpression)
    {
        if (!PatchPathParser.IsValid(pathExpression))
        {
            return false;
        }

        PatchPathSegment[] segments = PatchPathParser.Parse(pathExpression);
        foreach (PatchPathSegment segment in segments)
        {
            if (segment is FilterSegment filter)
            {
                foreach (FilterCondition condition in filter.Conditions)
                {
                    if (string.Equals(condition.Key, "segment", StringComparison.OrdinalIgnoreCase)
                        && condition.Value is null)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Extracts all cultures from a collection of path expressions.
    /// </summary>
    /// <param name="pathExpressions">Collection of path expressions.</param>
    /// <returns>A set of all unique cultures across all expressions.</returns>
    public ISet<string> ExtractCulturesFromOperations(IEnumerable<string> pathExpressions)
    {
        var allCultures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var path in pathExpressions)
        {
            ISet<string> cultures = ExtractCultures(path);
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
    /// <param name="pathExpressions">Collection of path expressions.</param>
    /// <returns>True if any expression contains invariant culture filter.</returns>
    public bool AnyOperationTargetsInvariantCulture(IEnumerable<string> pathExpressions)
        => pathExpressions.Any(ContainsInvariantCultureFilter);
}
