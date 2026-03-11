using System.Text.Json;
using System.Text.Json.Nodes;
using Umbraco.Cms.Api.Management.ViewModels.Patching;

namespace Umbraco.Cms.Api.Management.Patching;

/// <summary>
/// Applies patch operations to JSON documents using Umbraco's custom path syntax based on Json pointer
/// augmented with array filtering to support culture/segment and keyed items.
/// </summary>
public static class PatchEngine
{
    /// <summary>
    /// Applies a single patch operation to a JSON string and returns the modified JSON.
    /// </summary>
    /// <param name="json">The JSON string to modify.</param>
    /// <param name="op">The operation type (Replace, Add, Remove).</param>
    /// <param name="path">The patch path expression.</param>
    /// <param name="value">The value to set (required for Replace and Add operations).</param>
    /// <returns>The modified JSON string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the operation cannot be applied.</exception>
    /// <exception cref="FormatException">Thrown when the path syntax is invalid.</exception>
    public static string ApplyOperation(string json, PatchOperationType op, string path, object? value)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentNullException(nameof(json));
        }

        PatchPathSegment[] segments = PatchPathParser.Parse(path);

        JsonNode? rootNode = JsonNode.Parse(json);
        if (rootNode is null)
        {
            throw new InvalidOperationException("Failed to parse JSON string.");
        }

        JsonNode? valueNode = value is not null
            ? JsonSerializer.SerializeToNode(value)
            : null;

        ResolvedTarget target = PatchPathResolver.Resolve(rootNode, segments);
        ApplyMutation(target, op, valueNode);

        return rootNode.ToJsonString();
    }

    private static void ApplyMutation(ResolvedTarget target, PatchOperationType op, JsonNode? valueNode)
    {
        switch (op)
        {
            case PatchOperationType.Replace:
                ApplyReplace(target, valueNode);
                break;

            case PatchOperationType.Add:
                ApplyAdd(target, valueNode);
                break;

            case PatchOperationType.Remove:
                ApplyRemove(target);
                break;

            default:
                throw new InvalidOperationException($"Unsupported operation type: {op}");
        }
    }

    private static void ApplyReplace(ResolvedTarget target, JsonNode? valueNode)
    {
        if (target.IsAppend)
        {
            throw new InvalidOperationException("Cannot use 'replace' with append target '/-'.");
        }

        switch (target.Key)
        {
            case string propertyName when target.Parent is JsonObject parentObj:
                parentObj[propertyName] = valueNode?.DeepClone();
                break;

            case int index when target.Parent is JsonArray parentArr:
                parentArr[index] = valueNode?.DeepClone();
                break;

            default:
                throw new InvalidOperationException("Cannot replace at the resolved target location.");
        }
    }

    private static void ApplyAdd(ResolvedTarget target, JsonNode? valueNode)
    {
        if (target.IsAppend)
        {
            if (target.Parent is JsonArray appendArray)
            {
                appendArray.Add(valueNode?.DeepClone());
                return;
            }

            throw new InvalidOperationException("Append target '/-' requires parent to be an array.");
        }

        switch (target.Key)
        {
            case string propertyName when target.Parent is JsonObject parentObj:
                parentObj[propertyName] = valueNode?.DeepClone();
                break;

            case int index when target.Parent is JsonArray parentArr:
                parentArr.Insert(index, valueNode?.DeepClone());
                break;

            default:
                throw new InvalidOperationException("Cannot add at the resolved target location.");
        }
    }

    private static void ApplyRemove(ResolvedTarget target)
    {
        if (target.IsAppend)
        {
            throw new InvalidOperationException("Cannot use 'remove' with append target '/-'.");
        }

        switch (target.Key)
        {
            case string propertyName when target.Parent is JsonObject parentObj:
                parentObj.Remove(propertyName);
                break;

            case int index when target.Parent is JsonArray parentArr:
                parentArr.RemoveAt(index);
                break;

            default:
                throw new InvalidOperationException("Cannot remove at the resolved target location.");
        }
    }
}
