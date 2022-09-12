using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Patch;
using Umbraco.Cms.ManagementApi.ViewModels.JsonPatch;

namespace Umbraco.Cms.ManagementApi.Services;

public class JsonPatchService : IJsonPatchService
{

    public PatchResult? Patch(JsonPatchViewModel[] patchViewModel, object objectToPatch)
    {
        var patchString = JsonSerializer.Serialize(patchViewModel);
        var docString = JsonSerializer.Serialize(objectToPatch);
        JsonPatch? patch = JsonSerializer.Deserialize<JsonPatch>(patchString);
        var doc = JsonNode.Parse(docString);
        return patch?.Apply(doc);
    }
}
