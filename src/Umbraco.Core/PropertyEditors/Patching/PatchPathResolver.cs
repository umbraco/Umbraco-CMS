using System.Text.Json.Nodes;

namespace Umbraco.Cms.Core.PropertyEditors.Patching;

/// <summary>
/// The result of resolving a patch path against a JSON document.
/// </summary>
public sealed class ResolvedTarget
{
    /// <summary>
    /// The parent node of the target location.
    /// </summary>
    public required JsonNode Parent { get; init; }

    /// <summary>
    /// The key identifying the target within the parent:
    /// a <see cref="string"/> for object properties, an <see cref="int"/> for array indices,
    /// or <c>null</c> for append operations.
    /// </summary>
    public required object? Key { get; init; }

    /// <summary>
    /// The current value at the target location, or null if the location doesn't exist yet (e.g., for Add).
    /// </summary>
    public JsonNode? Current { get; init; }

    /// <summary>
    /// Whether this target represents an append to the end of an array.
    /// </summary>
    public bool IsAppend { get; init; }
}

/// <summary>
/// Resolves parsed patch path segments against a JSON document to find the target node.
/// </summary>
public static class PatchPathResolver
{
    /// <summary>
    /// Resolves a parsed path against a JSON node tree and returns the target location.
    /// </summary>
    /// <param name="root">The root JSON node.</param>
    /// <param name="segments">The parsed path segments.</param>
    /// <returns>The resolved target containing the parent node and key.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the path cannot be resolved.</exception>
    public static ResolvedTarget Resolve(JsonNode root, PatchPathSegment[] segments)
    {
        if (segments.Length == 0)
        {
            throw new InvalidOperationException("Path must contain at least one segment.");
        }

        JsonNode? current = root;
        JsonNode? parent = null;
        object? lastKey = null;

        for (var i = 0; i < segments.Length; i++)
        {
            PatchPathSegment segment = segments[i];

            switch (segment)
            {
                case PropertySegment property:
                    if (current is not JsonObject obj)
                    {
                        throw new InvalidOperationException(
                            $"Expected object at path segment '{property.Name}', but found {current?.GetType().Name ?? "null"}.");
                    }

                    parent = current;
                    lastKey = property.Name;
                    current = obj[property.Name];
                    break;

                case FilterSegment filter:
                    if (current is not JsonArray filterArray)
                    {
                        throw new InvalidOperationException(
                            $"Expected array for filter operation, but found {current?.GetType().Name ?? "null"}.");
                    }

                    var (matchedNode, matchedIndex) = FindMatchingElement(filterArray, filter.Conditions);
                    if (matchedNode is null)
                    {
                        throw new InvalidOperationException(
                            $"No array element matched filter conditions: [{FormatConditions(filter.Conditions)}].");
                    }

                    parent = filterArray;
                    lastKey = matchedIndex;
                    current = matchedNode;
                    break;

                case IndexSegment index:
                    if (current is not JsonArray indexArray)
                    {
                        throw new InvalidOperationException(
                            $"Expected array for index access, but found {current?.GetType().Name ?? "null"}.");
                    }

                    if (index.Index < 0 || index.Index >= indexArray.Count)
                    {
                        throw new InvalidOperationException(
                            $"Array index {index.Index} is out of bounds (array length: {indexArray.Count}).");
                    }

                    parent = indexArray;
                    lastKey = index.Index;
                    current = indexArray[index.Index];
                    break;

                case AppendSegment:
                    if (i != segments.Length - 1)
                    {
                        throw new InvalidOperationException("Append segment '-' must be the last segment in the path.");
                    }

                    if (current is not JsonArray)
                    {
                        throw new InvalidOperationException(
                            $"Expected array for append operation, but found {current?.GetType().Name ?? "null"}.");
                    }

                    return new ResolvedTarget
                    {
                        Parent = current,
                        Key = null,
                        Current = null,
                        IsAppend = true,
                    };
            }
        }

        if (parent is null || lastKey is null)
        {
            throw new InvalidOperationException("Could not resolve path to a valid target.");
        }

        return new ResolvedTarget
        {
            Parent = parent,
            Key = lastKey,
            Current = current,
        };
    }

    private static (JsonNode? Node, int Index) FindMatchingElement(JsonArray array, FilterCondition[] conditions)
    {
        for (var i = 0; i < array.Count; i++)
        {
            JsonNode? element = array[i];
            if (element is JsonObject elementObj && MatchesAllConditions(elementObj, conditions))
            {
                return (element, i);
            }
        }

        return (null, -1);
    }

    private static bool MatchesAllConditions(JsonObject element, FilterCondition[] conditions)
    {
        foreach (FilterCondition condition in conditions)
        {
            JsonNode? propertyNode = element[condition.Key];

            if (condition.Value is null)
            {
                // Condition expects null: property must be missing or explicitly null
                if (propertyNode is not null)
                {
                    return false;
                }
            }
            else
            {
                // Condition expects a specific value: compare as string
                if (propertyNode is null)
                {
                    return false;
                }

                var nodeValue = propertyNode.GetValue<string>();
                if (!string.Equals(nodeValue, condition.Value, StringComparison.Ordinal))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static string FormatConditions(FilterCondition[] conditions) =>
        string.Join(", ", conditions.Select(c => $"{c.Key}={c.Value ?? "null"}"));
}
