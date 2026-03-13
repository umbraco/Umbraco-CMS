using Json.Patch;
using Umbraco.Cms.Api.Management.ViewModels.JsonPatch;

namespace Umbraco.Cms.Api.Management.Services;

/// <summary>
/// Represents a service that processes and applies JSON Patch operations to resources.
/// </summary>
public interface IJsonPatchService
{
    /// <summary>
    /// Applies a sequence of JSON patch operations to a specified object.
    /// </summary>
    /// <param name="patchViewModel">An array of <see cref="JsonPatchViewModel"/> representing the JSON patch operations to apply.</param>
    /// <param name="objectToPatch">The target object to which the patch operations will be applied.</param>
    /// <returns>A <see cref="PatchResult"/> indicating the outcome of the patch operation, or <c>null</c> if the patch could not be applied.</returns>
    PatchResult? Patch(JsonPatchViewModel[] patchViewModel, object objectToPatch);
}
