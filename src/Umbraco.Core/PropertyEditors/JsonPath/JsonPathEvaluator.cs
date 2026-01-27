using System.Text.Json;
using JsonCons.JsonPath;

namespace Umbraco.Cms.Core.PropertyEditors.JsonPath;

/// <summary>
/// Evaluates JSONPath expressions against JSON documents.
/// Wraps JsonCons.JsonPath library to provide RFC 9535 compliant JSONPath evaluation.
/// </summary>
public class JsonPathEvaluator
{
    /// <summary>
    /// Evaluates a JSONPath expression against a JSON document and returns matching nodes.
    /// </summary>
    /// <param name="jsonDocument">The JSON document to query.</param>
    /// <param name="pathExpression">The JSONPath expression (e.g., "$.values[?(@.alias == 'title')]").</param>
    /// <returns>A list of matching JSON elements, or an empty list if no matches found.</returns>
    /// <exception cref="ArgumentException">Thrown when the path expression is invalid.</exception>
    public IReadOnlyList<JsonElement> Select(JsonDocument jsonDocument, string pathExpression)
    {
        if (jsonDocument == null)
        {
            throw new ArgumentNullException(nameof(jsonDocument));
        }

        if (string.IsNullOrWhiteSpace(pathExpression))
        {
            throw new ArgumentException("Path expression cannot be null or empty.", nameof(pathExpression));
        }

        try
        {
            var selector = JsonSelector.Parse(pathExpression);
            var results = selector.Select(jsonDocument.RootElement);
            return results.ToList();
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid JSONPath expression: {pathExpression}", nameof(pathExpression), ex);
        }
    }

    /// <summary>
    /// Evaluates a JSONPath expression and returns the first matching node, or null if no match found.
    /// </summary>
    /// <param name="jsonDocument">The JSON document to query.</param>
    /// <param name="pathExpression">The JSONPath expression.</param>
    /// <returns>The first matching JSON element, or null if no matches found.</returns>
    public JsonElement? SelectSingle(JsonDocument jsonDocument, string pathExpression)
    {
        var results = Select(jsonDocument, pathExpression);
        return results.Count > 0 ? results[0] : null;
    }

    /// <summary>
    /// Checks if a JSONPath expression matches any nodes in the document.
    /// </summary>
    /// <param name="jsonDocument">The JSON document to query.</param>
    /// <param name="pathExpression">The JSONPath expression.</param>
    /// <returns>True if at least one node matches, false otherwise.</returns>
    public bool Exists(JsonDocument jsonDocument, string pathExpression)
    {
        var results = Select(jsonDocument, pathExpression);
        return results.Count > 0;
    }

    /// <summary>
    /// Validates that a JSONPath expression is syntactically correct.
    /// </summary>
    /// <param name="pathExpression">The JSONPath expression to validate.</param>
    /// <returns>True if the expression is valid, false otherwise.</returns>
    public bool IsValidExpression(string pathExpression)
    {
        if (string.IsNullOrWhiteSpace(pathExpression))
        {
            return false;
        }

        try
        {
            JsonSelector.Parse(pathExpression);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
