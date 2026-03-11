using System.Text.Json.Nodes;
using Json.Patch;
using Umbraco.Cms.Api.Management.ViewModels.JsonPatch;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Api.Management.Services;

/// <summary>
/// Service for applying JSON Patch operations using JsonPatch.Net.
/// </summary>
[Obsolete("Use the custom patch engine (DocumentPatcher) instead. JsonPatch.Net dependency is being removed. Scheduled for removal in Umbraco 19.")]
public class JsonPatchService : IJsonPatchService
{
    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonPatchService"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    public JsonPatchService(IJsonSerializer jsonSerializer) => _jsonSerializer = jsonSerializer;

    /// <inheritdoc/>
    public PatchResult? Patch(JsonPatchViewModel[] patchViewModel, object objectToPatch)
    {
        var patchString = _jsonSerializer.Serialize(patchViewModel);

        var docString = _jsonSerializer.Serialize(objectToPatch);
        JsonPatch? patch = _jsonSerializer.Deserialize<JsonPatch>(patchString);
        var doc = JsonNode.Parse(docString);
        return patch?.Apply(doc);
    }
}
