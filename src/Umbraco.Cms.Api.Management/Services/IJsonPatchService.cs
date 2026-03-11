using Json.Patch;
using Umbraco.Cms.Api.Management.ViewModels.JsonPatch;

namespace Umbraco.Cms.Api.Management.Services;

/// <summary>
/// Service for applying JSON Patch operations using JsonPatch.Net.
/// </summary>
[Obsolete("Use the custom patch engine (DocumentPatcher) instead. JsonPatch.Net dependency is being removed. Scheduled for removal in Umbraco 19.")]
public interface IJsonPatchService
{
    /// <summary>
    /// Applies JSON Patch operations to an object.
    /// </summary>
    /// <param name="patchViewModel">The patch operations to apply.</param>
    /// <param name="objectToPatch">The object to patch.</param>
    /// <returns>The result of the patch operation.</returns>
    PatchResult? Patch(JsonPatchViewModel[] patchViewModel, object objectToPatch);
}
