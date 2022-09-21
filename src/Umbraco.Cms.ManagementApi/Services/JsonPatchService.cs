using System.Text.Json.Nodes;
using Json.Patch;
using Umbraco.Cms.ManagementApi.Serialization;
using Umbraco.Cms.ManagementApi.ViewModels.JsonPatch;

namespace Umbraco.Cms.ManagementApi.Services;

public class JsonPatchService : IJsonPatchService
{
    private readonly ISystemTextJsonSerializer _systemTextJsonSerializer;

    public JsonPatchService(ISystemTextJsonSerializer systemTextJsonSerializer) => _systemTextJsonSerializer = systemTextJsonSerializer;

    public PatchResult? Patch(JsonPatchViewModel[] patchViewModel, object objectToPatch)
    {
        var patchString = _systemTextJsonSerializer.Serialize(patchViewModel);

        var docString = _systemTextJsonSerializer.Serialize(objectToPatch);
        JsonPatch? patch = _systemTextJsonSerializer.Deserialize<JsonPatch>(patchString);
        var doc = JsonNode.Parse(docString);
        return patch?.Apply(doc);
    }
}
