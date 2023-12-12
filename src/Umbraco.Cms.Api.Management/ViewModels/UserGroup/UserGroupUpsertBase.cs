namespace Umbraco.Cms.Api.Management.ViewModels.UserGroup;

public class UserGroupUpsertBase : UserGroupBase
{
    /// <summary>
    /// The list of users that should be part of this UserGroup after the operation has concluded
    /// </summary>
    public Guid[]? GroupUsers { get; init; }
}
