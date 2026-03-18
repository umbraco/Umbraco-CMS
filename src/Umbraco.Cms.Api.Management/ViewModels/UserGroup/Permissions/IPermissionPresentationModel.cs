using Umbraco.Cms.Api.Common.OpenApi;

namespace Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;

/// <summary>
/// Defines an interface for presenting user group permission information in the management API.
/// </summary>
public interface IPermissionPresentationModel : IOpenApiDiscriminator
{
    /// <summary>
    /// Gets or sets the set of verbs associated with the permission.
    /// </summary>
    ISet<string> Verbs { get; set; }
}
