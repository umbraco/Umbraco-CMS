using Umbraco.Cms.Api.Management.OpenApi;

namespace Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;

public interface IPermissionPresentationModel : IOpenApiDiscriminator
{
    ISet<string> Verbs { get; set; }
}
