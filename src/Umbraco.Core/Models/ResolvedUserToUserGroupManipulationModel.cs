using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Models;

public class ResolvedUserToUserGroupManipulationModel
{
    public required IUser[] Users { get; init; }

    public required IUserGroup UserGroup { get; init; }
}
