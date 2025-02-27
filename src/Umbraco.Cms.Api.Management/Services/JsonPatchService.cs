using System.Text.Json.Nodes;
using Json.Patch;
using Umbraco.Cms.Api.Management.Serialization;
using Umbraco.Cms.Api.Management.ViewModels.JsonPatch;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Api.Management.Services;

public class JsonPatchService : IJsonPatchService
{
    private readonly IJsonSerializer _jsonSerializer;

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
