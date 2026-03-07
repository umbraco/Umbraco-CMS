using System.Text.Json.Nodes;
using Json.Patch;
using Umbraco.Cms.Api.Management.Serialization;
using Umbraco.Cms.Api.Management.ViewModels.JsonPatch;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Api.Management.Services;

    /// <summary>
    /// Provides functionality to apply and manage JSON Patch operations on resources.
    /// </summary>
public class JsonPatchService : IJsonPatchService
{
    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Services.JsonPatchService"/> class with the specified JSON serializer.
    /// </summary>
    /// <param name="jsonSerializer">The <see cref="IJsonSerializer"/> instance used for JSON serialization and deserialization.</param>
    public JsonPatchService(IJsonSerializer jsonSerializer) => _jsonSerializer = jsonSerializer;

    public PatchResult? Patch(JsonPatchViewModel[] patchViewModel, object objectToPatch)
    {
        var patchString = _jsonSerializer.Serialize(patchViewModel);

        var docString = _jsonSerializer.Serialize(objectToPatch);
        JsonPatch? patch = _jsonSerializer.Deserialize<JsonPatch>(patchString);
        var doc = JsonNode.Parse(docString);
        return patch?.Apply(doc);
    }
}
