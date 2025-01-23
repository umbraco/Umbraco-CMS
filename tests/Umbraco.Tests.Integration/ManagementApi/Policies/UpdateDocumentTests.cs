using System.Linq.Expressions;
using System.Net.Http.Json;
using System.Text;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Policies;

public class UpdateDocumentTests : ManagementApiTest<UpdateDocumentController>
{
    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private IShortStringHelper ShortStringHelper => GetRequiredService<IShortStringHelper>();

    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    protected override Expression<Func<UpdateDocumentController, object>> MethodSelector =>
        x => x.Update(CancellationToken.None, Guid.Empty, null!);

    [Test]
    public async Task UserWithoutPermissionCannotUpdate()
    {
        var userGroup = new UserGroup(ShortStringHelper);
        userGroup.Name = "Test";
        userGroup.Alias = "test";
        userGroup.Permissions = new HashSet<string> { "Umb.Document.Read" };
        userGroup.HasAccessToAllLanguages = true;
        userGroup.StartContentId = -1;
        userGroup.StartMediaId = -1;
        userGroup.AddAllowedSection("content");
        userGroup.AddAllowedSection("media");

        var groupCreationResult = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);
        Assert.IsTrue(groupCreationResult.Success);

        await AuthenticateClientAsync(Client, async userService =>
        {
            var email = "test@test.com";
            var testUserCreateModel = new UserCreateModel
            {
                Email = email,
                Name = "Test Mc.Gee",
                UserName = email,
                UserGroupKeys = new HashSet<Guid> { groupCreationResult.Result.Key },
            };

            var userCreationResult =
                await userService.CreateAsync(Constants.Security.SuperUserKey, testUserCreateModel, true);

            Assert.IsTrue(userCreationResult.Success);

            return (userCreationResult.Result.CreatedUser, "1234567890");
        });

        var updateModel = new UpdateDocumentRequestModel();
        var content = new StringContent(JsonSerializer.Serialize(updateModel), Encoding.UTF8, "application/json");
        var response = await Client.PutAsync(Url, content);
    }
}
