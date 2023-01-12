using Json.Patch;
using Umbraco.Cms.Api.Management.ViewModels.JsonPatch;

namespace Umbraco.Cms.Api.Management.Services;

public interface IJsonPatchService
{
    PatchResult? Patch(JsonPatchViewModel[] patchViewModel, object objectToPatch);
}
