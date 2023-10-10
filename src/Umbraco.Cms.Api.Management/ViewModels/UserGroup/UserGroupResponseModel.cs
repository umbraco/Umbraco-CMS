using Umbraco.Cms.Core;
namespace Umbraco.Cms.Api.Management.ViewModels.UserGroup;

public class UserGroupResponseModel : UserGroupBase, INamedEntityPresentationModel, IResponseModel
{
    /// <summary>
    /// The key identifier for the user group.
    /// </summary>
    public required Guid Id { get; init; }

    public string Type => UdiEntityType.UserGroup;
}
