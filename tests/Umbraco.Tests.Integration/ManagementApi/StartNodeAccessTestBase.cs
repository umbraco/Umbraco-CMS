using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi;

/// <summary>
/// Base for tests asserting that an endpoint honours the start nodes of the requesting user.
/// </summary>
/// <remarks>
/// <see cref="ManagementApiUserGroupTestBase{T}"/> only varies the built-in user groups, all of which have
/// root level access, so it cannot express a start node restriction.
/// </remarks>
public abstract class StartNodeAccessTestBase<T> : ManagementApiTest<T>
    where T : ManagementApiControllerBase
{
    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    // The URL helper names query parameters after the action parameters, but batch endpoints bind their ids from "id".
    protected static string WithIds(string url, IEnumerable<Guid> ids)
        => $"{url}{(url.Contains('?') ? '&' : '?')}{string.Join('&', ids.Select(id => $"id={id}"))}";

    protected async Task AuthenticateRestrictedUserAsync(string[] sections, int? startContentId = null, int? startMediaId = null)
    {
        var alias = $"restricted{Guid.NewGuid():N}";

        var builder = new UserGroupBuilder()
            .WithAlias(alias)
            .WithName(alias)
            .WithAllowedSections(sections)
            .WithPermissions(new HashSet<string> { ActionBrowse.ActionLetter });
        if (startContentId is not null)
        {
            builder.WithStartContentId(startContentId.Value);
        }

        if (startMediaId is not null)
        {
            builder.WithStartMediaId(startMediaId.Value);
        }

        IUserGroup userGroup = builder.Build();
        userGroup.HasAccessToAllLanguages = true;

        Attempt<IUserGroup, UserGroupOperationStatus> userGroupAttempt =
            await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);
        Assert.IsTrue(userGroupAttempt.Success, $"Could not create the user group: {userGroupAttempt.Status}");

        var email = $"{alias}@umbraco.com";

        await AuthenticateClientAsync(
            Client,
            async userService =>
            {
                IUser user = (await userService.CreateAsync(
                    Constants.Security.SuperUserKey,
                    new UserCreateModel
                    {
                        Email = email,
                        Name = alias,
                        UserName = email,
                        UserGroupKeys = new HashSet<Guid> { userGroupAttempt.Result.Key },
                    },
                    true)).Result.CreatedUser;

                return (user, UserPassword);
            },
            $"{email}:{alias}");
    }
}
