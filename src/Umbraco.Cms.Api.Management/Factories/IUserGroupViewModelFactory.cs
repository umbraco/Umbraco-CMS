using Umbraco.Cms.Api.Management.ViewModels.UserGroups;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// A factory for creating <see cref="UserGroupViewModel"/>
/// </summary>
public interface IUserGroupViewModelFactory
{
    /// <summary>
    /// Creates a <see cref="UserGroupViewModel"/> based on a <see cref="UserGroup"/>
    /// </summary>
    /// <param name="userGroup"></param>
    /// <returns></returns>
    UserGroupViewModel Create(IUserGroup userGroup);

    /// <summary>
    /// Creates multiple <see cref="UserGroupViewModel"/> base on multiple <see cref="UserGroup"/>
    /// </summary>
    /// <param name="userGroups"></param>
    /// <returns></returns>
    IEnumerable<UserGroupViewModel> CreateMultiple(IEnumerable<IUserGroup> userGroups);
}
