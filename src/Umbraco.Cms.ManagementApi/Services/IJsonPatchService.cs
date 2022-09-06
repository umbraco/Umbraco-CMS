using Json.Patch;
using Umbraco.Cms.ManagementApi.ViewModels.JsonPatch;

namespace Umbraco.Cms.ManagementApi.Services;

public interface IJsonPatchService
{
    PatchResult? Patch(JsonPatchViewModel[] patchViewModel, object objectToPatch);
}
