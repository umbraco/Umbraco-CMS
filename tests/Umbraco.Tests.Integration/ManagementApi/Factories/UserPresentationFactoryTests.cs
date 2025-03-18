using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Mapping.Permissions;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Factories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class UserPresentationFactoryTests : UmbracoIntegrationTestWithContent
{
    public IUserPresentationFactory UserPresentationFactory => GetRequiredService<IUserPresentationFactory>();

    public IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    public IUserService UserService => GetRequiredService<IUserService>();

    public ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    public IMediaService MediaService => GetRequiredService<IMediaService>();

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        services.AddTransient<IUserPresentationFactory, UserPresentationFactory>();
        services.AddTransient<IUserGroupPresentationFactory, UserGroupPresentationFactory>();
        services.AddSingleton<IAbsoluteUrlBuilder, DefaultAbsoluteUrlBuilder>();
        services.AddSingleton<IUrlAssembler, DefaultUrlAssembler>();
        services.AddSingleton<IPasswordConfigurationPresentationFactory, PasswordConfigurationPresentationFactory>();
        services.AddSingleton<IPermissionPresentationFactory, PermissionPresentationFactory>();
        services.AddSingleton<DocumentPermissionMapper>();
        services.AddSingleton<IPermissionMapper>(x => x.GetRequiredService<DocumentPermissionMapper>());
        services.AddSingleton<IPermissionPresentationMapper>(x => x.GetRequiredService<DocumentPermissionMapper>());
    }

    [Test]
    public async Task Can_Create_Current_User_Response_Model()
    {
        var daLanguage = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .Build();
        await LanguageService.CreateAsync(daLanguage, Constants.Security.SuperUserKey);
        var enUsLanguage = await LanguageService.GetAsync("en-US");
        var daDkLanguage = await LanguageService.GetAsync("da-DK");

        var rootMediaFolder = MediaService.CreateMedia("Pictures Folder", Constants.System.Root, "Folder");
        MediaService.Save(rootMediaFolder);

        var groupOne = new UserGroupBuilder()
            .WithName("Group One")
            .WithAlias("groupOne")
            .WithAllowedLanguages([enUsLanguage.Id])
            .WithStartMediaId(rootMediaFolder.Id)
            .WithPermissions(new[] { ActionBrowse.ActionLetter, ActionDelete.ActionLetter, ActionPublish.ActionLetter }.ToHashSet())
            .WithGranularPermissions([new DocumentGranularPermission
            {
                Key = Guid.Parse(TextpageKey),
                Permission = ActionBrowse.ActionLetter
            }])
            .Build();
        var createUserGroupResult = await UserGroupService.CreateAsync(groupOne, Constants.Security.SuperUserKey);
        Assert.IsTrue(createUserGroupResult.Success);

        var groupTwo = new UserGroupBuilder()
            .WithName("Group Two")
            .WithAlias("groupTwo")
            .WithAllowedLanguages([daDkLanguage.Id])
            .WithStartMediaId(rootMediaFolder.Id)
            .WithPermissions(new[] { ActionBrowse.ActionLetter, ActionDelete.ActionLetter, ActionUnpublish.ActionLetter }.ToHashSet())
            .Build();
        createUserGroupResult = await UserGroupService.CreateAsync(groupTwo, Constants.Security.SuperUserKey);
        Assert.IsTrue(createUserGroupResult.Success);

        var createUserAttempt = await UserService.CreateAsync(Constants.Security.SuperUserKey, new UserCreateModel
        {
            Email = "test@test.com",
            Name = "Test User",
            UserName = "test@test.com",
            UserGroupKeys = new[] { groupOne.Key, groupTwo.Key }.ToHashSet(),
        });
        Assert.IsTrue(createUserAttempt.Success);

        var userId = createUserAttempt.Result.CreatedUser.Key;

        var user = await UserService.GetAsync(userId);

        var model = await UserPresentationFactory.CreateCurrentUserResponseModelAsync(user);

        Assert.AreEqual(userId, model.Id);
        Assert.AreEqual("test@test.com", model.Email);
        Assert.AreEqual("Test User", model.Name);
        Assert.AreEqual("test@test.com", model.UserName);
        Assert.AreEqual(2, model.UserGroupIds.Count);
        Assert.IsTrue(model.UserGroupIds.Select(x => x.Id).ContainsAll([groupOne.Key, groupTwo.Key]));
        Assert.IsFalse(model.HasAccessToAllLanguages);
        Assert.AreEqual(2, model.Languages.Count());
        Assert.IsTrue(model.Languages.ContainsAll(["en-US", "da-DK"]));
        Assert.IsTrue(model.HasDocumentRootAccess);
        Assert.AreEqual(0, model.DocumentStartNodeIds.Count);
        Assert.IsFalse(model.HasMediaRootAccess);
        Assert.AreEqual(1, model.MediaStartNodeIds.Count);
        Assert.AreEqual(rootMediaFolder.Key, model.MediaStartNodeIds.First().Id);
        Assert.IsFalse(model.HasAccessToSensitiveData);
        Assert.AreEqual(4, model.FallbackPermissions.Count);
        Assert.IsTrue(model.FallbackPermissions.ContainsAll([ActionBrowse.ActionLetter, ActionDelete.ActionLetter, ActionPublish.ActionLetter, ActionUnpublish.ActionLetter]));
        Assert.AreEqual(1, model.Permissions.Count);
    }
}
