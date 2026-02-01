using System.Text.Json;
using System.Text.Json.Nodes;
using JsonCons.JsonPath;
using Umbraco.Cms.Core.Models.ContentEditing;

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

    /// <summary>
    /// Applies a PATCH operation (replace/add/remove) to a JSON string at the specified JSONPath.
    /// Returns a new JSON string with the modification applied.
    /// </summary>
    /// <param name="jsonString">The JSON string to modify.</param>
    /// <param name="op">The operation type (Replace, Add, Remove).</param>
    /// <param name="path">The JSONPath expression identifying the target location.</param>
    /// <param name="value">The value to set (required for Replace and Add operations).</param>
    /// <returns>The modified JSON string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the path matches no elements for Replace/Remove operations.</exception>
    public string ApplyOperation(string jsonString, PatchOperationType op, string path, object? value)
    {
        if (string.IsNullOrWhiteSpace(jsonString))
        {
            throw new ArgumentNullException(nameof(jsonString));
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path expression cannot be null or empty.", nameof(path));
        }

        // Parse to mutable JsonNode
        JsonNode? rootNode = JsonNode.Parse(jsonString);
        if (rootNode == null)
        {
            throw new InvalidOperationException("Failed to parse JSON string.");
        }

        // Use JsonCons to find matching nodes (path-value pairs)
        using var doc = JsonDocument.Parse(jsonString);
        var selector = JsonSelector.Parse(path);
        var nodes = selector.SelectNodes(doc.RootElement);
        var nodeList = nodes.ToList();

        if (nodeList.Count == 0 && op != PatchOperationType.Add)
        {
            throw new InvalidOperationException($"JSONPath expression '{path}' matched no elements.");
        }

        // Convert value to JsonNode
        JsonNode? valueNode = value != null
            ? JsonSerializer.SerializeToNode(value)
            : null;

        // Apply operation to each matched path
        foreach (var node in nodeList)
        {
            var jsonPointer = node.Path.ToJsonPointer();
            ApplyOperationAtJsonPointer(rootNode, jsonPointer, op, valueNode);
        }

        return rootNode.ToJsonString();
    }

    private void ApplyOperationAtJsonPointer(JsonNode rootNode, string jsonPointer, PatchOperationType op, JsonNode? valueNode)
    {
        // Parse JSON Pointer into segments
        var segments = ParseJsonPointer(jsonPointer);

        if (segments.Length == 0)
        {
            throw new InvalidOperationException("Cannot modify root node directly.");
        }

        // Navigate to the parent of the target node
        JsonNode? current = rootNode;
        JsonNode? parent = null;
        object? lastKey = null; // string for object property, int for array index

        for (int i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];
            parent = current;

            // Try to parse as array index
            if (int.TryParse(segment, out int arrayIndex) && current is JsonArray)
            {
                lastKey = arrayIndex;
                current = current?[arrayIndex];
            }
            else
            {
                // Treat as object property name
                lastKey = segment;
                current = current?[segment];
            }
        }

        if (parent == null || lastKey == null)
        {
            throw new InvalidOperationException("Cannot modify root node directly.");
        }

        // Apply the operation
        switch (op)
        {
            case PatchOperationType.Replace:
                if (lastKey is string propertyName)
                {
                    if (parent is JsonObject parentObj)
                    {
                        parentObj[propertyName] = valueNode?.DeepClone();
                    }
                }
                else if (lastKey is int index)
                {
                    if (parent is JsonArray parentArr)
                    {
                        parentArr[index] = valueNode?.DeepClone();
                    }
                }
                break;

            case PatchOperationType.Add:
                if (lastKey is string addPropertyName)
                {
                    if (parent is JsonObject addParentObj)
                    {
                        addParentObj[addPropertyName] = valueNode?.DeepClone();
                    }
                }
                else if (lastKey is int addIndex)
                {
                    if (parent is JsonArray addParentArr)
                    {
                        addParentArr.Insert(addIndex, valueNode?.DeepClone());
                    }
                }
                break;

            case PatchOperationType.Remove:
                if (lastKey is string removePropertyName)
                {
                    if (parent is JsonObject removeParentObj)
                    {
                        removeParentObj.Remove(removePropertyName);
                    }
                }
                else if (lastKey is int removeIndex)
                {
                    if (parent is JsonArray removeParentArr)
                    {
                        removeParentArr.RemoveAt(removeIndex);
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Parses a JSON Pointer string (RFC 6901) into path segments.
    /// </summary>
    private static string[] ParseJsonPointer(string jsonPointer)
    {
        if (string.IsNullOrEmpty(jsonPointer) || jsonPointer == "/")
        {
            return Array.Empty<string>();
        }

        // Remove leading slash and split
        var pointer = jsonPointer.StartsWith("/") ? jsonPointer.Substring(1) : jsonPointer;
        var segments = pointer.Split('/');

        // Unescape JSON Pointer special characters (~1 -> /, ~0 -> ~)
        for (int i = 0; i < segments.Length; i++)
        {
            segments[i] = segments[i].Replace("~1", "/").Replace("~0", "~");
        }

        return segments;
    }
}
