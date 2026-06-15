using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Mapping.Permissions;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
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

        services.AddSingleton<IPermissionMapper, DocumentPermissionMapper>();
        services.AddSingleton<IPermissionPresentationMapper, DocumentPermissionMapper>();

        services.AddSingleton<IPermissionMapper, DocumentPropertyValuePermissionMapper>();
        services.AddSingleton<IPermissionPresentationMapper, DocumentPropertyValuePermissionMapper>();

        services.AddSingleton<IPermissionMapper, CustomPermissionMapper>();
        services.AddSingleton<IPermissionPresentationMapper, CustomPermissionMapper>();
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

        var groupOne = await CreateUserGroup(
            "Group One",
            "groupOne",
            [enUsLanguage.Id],
            [],
            [],
            rootMediaFolder.Id);
        var groupTwo = await CreateUserGroup(
            "Group Two",
            "groupTwo",
            [daDkLanguage.Id],
            [],
            [],
            rootMediaFolder.Id);

        var user = await CreateUser([groupOne.Key, groupTwo.Key]);

        var model = await UserPresentationFactory.CreateCurrentUserResponseModelAsync(user);

        Assert.That(model.Id, Is.EqualTo(user.Key));
        Assert.That(model.Email, Is.EqualTo("test@test.com"));
        Assert.That(model.Name, Is.EqualTo("Test User"));
        Assert.That(model.UserName, Is.EqualTo("test@test.com"));
        Assert.That(model.UserGroupIds, Has.Count.EqualTo(2));
        Assert.That(model.UserGroupIds.Select(x => x.Id).ContainsAll([groupOne.Key, groupTwo.Key]), Is.True);
        Assert.That(model.HasAccessToAllLanguages, Is.False);
        Assert.That(model.Languages.Count(), Is.EqualTo(2));
        Assert.That(model.Languages.ContainsAll(["en-US", "da-DK"]), Is.True);
        Assert.That(model.HasDocumentRootAccess, Is.True);
        Assert.That(model.DocumentStartNodeIds, Is.Empty);
        Assert.That(model.HasMediaRootAccess, Is.False);
        Assert.That(model.MediaStartNodeIds, Has.Count.EqualTo(1));
        Assert.That(model.MediaStartNodeIds.First().Id, Is.EqualTo(rootMediaFolder.Key));
        Assert.That(model.HasElementRootAccess, Is.True);
        Assert.That(model.ElementStartNodeIds, Is.Empty);
        Assert.That(model.HasAccessToSensitiveData, Is.False);
    }

    [Test]
    public async Task Can_Create_Current_User_Response_Model_With_Aggregated_Document_Permissions()
    {
        var rootContentKey = Guid.Parse(TextpageKey);
        var subPageContentKey = Guid.Parse(SubPageKey);
        var subPage2ContentKey = Guid.Parse(SubPage2Key);

        var rootMediaFolder = MediaService.CreateMedia("Pictures Folder", Constants.System.Root, "Folder");
        MediaService.Save(rootMediaFolder);

        var groupOne = await CreateUserGroup(
            "Group One",
            "groupOne",
            [],
            ["A", "B", "C"],
            [
                new DocumentGranularPermission
                {
                    Key = rootContentKey,
                    Permission = "A",
                },
                new DocumentGranularPermission
                {
                    Key = rootContentKey,
                    Permission = "E",
                },
                new DocumentGranularPermission
                {
                    Key = subPageContentKey,
                    Permission = "F",
                },
                new DocumentGranularPermission
                {
                    Key = subPage2ContentKey,
                    Permission = "F",
                }
            ],
            rootMediaFolder.Id);
        var groupTwo = await CreateUserGroup(
            "Group Two",
            "groupTwo",
            [],
            ["A", "B", "D"],
            [
                new DocumentGranularPermission
                {
                    Key = subPage2ContentKey,
                    Permission = "G",
                },
                new DocumentGranularPermission
                {
                    Key = subPage2ContentKey,
                    Permission = "H",
                }
            ],
            rootMediaFolder.Id);

        var user = await CreateUser([groupOne.Key, groupTwo.Key]);

        var model = await UserPresentationFactory.CreateCurrentUserResponseModelAsync(user);

        Assert.That(model.FallbackPermissions, Has.Count.EqualTo(4));
        Assert.That(model.FallbackPermissions.ContainsAll(["A", "B", "C", "D"]), Is.True);

        // When aggregated, we expect one permission per document (we have several granular permissions assigned, for three unique documents).
        Assert.That(model.Permissions, Has.Count.EqualTo(3));

        // User has two user groups, one of which provides granular permissions for the root content item.
        // As such we expect the aggregated permissions to be the union of the specific permissions coming from the user group with them assigned to the document,
        // and the fallback permissions from the other.
        var rootContentPermissions = model.Permissions.Cast<DocumentPermissionPresentationModel>().Single(x => x.Document.Id == rootContentKey);
        Assert.That(rootContentPermissions.Verbs, Has.Count.EqualTo(4));
        Assert.That(rootContentPermissions.Verbs.ContainsAll(["A", "B", "D", "E"]), Is.True);

        // The sub-page and it's parent have specific granular permissions from one user group.
        // So we expect the aggregated permissions to include those from the sub-page and the other user's groups fallback permissions.
        var subPageContentPermissions = model.Permissions.Cast<DocumentPermissionPresentationModel>().Single(x => x.Document.Id == subPageContentKey);
        Assert.That(subPageContentPermissions.Verbs, Has.Count.EqualTo(4));
        Assert.That(subPageContentPermissions.Verbs.ContainsAll(["A", "B", "D", "F"]), Is.True);

        // Both user groups provide granular permissions for the second sub-page content item.
        // Here we expect the aggregated permissions to be the union of the granular permissions on the document from both user groups.
        var subPage2ContentPermissions = model.Permissions.Cast<DocumentPermissionPresentationModel>().Single(x => x.Document.Id == subPage2ContentKey);
        Assert.That(subPage2ContentPermissions.Verbs, Has.Count.EqualTo(3));
        Assert.That(subPage2ContentPermissions.Verbs.ContainsAll(["F", "G", "H"]), Is.True);
    }

    [Test]
    public async Task Can_Create_Current_User_Response_Model_With_Aggregated_Document_Property_Value_Permissions()
    {
        var propertyTypeKey = Guid.NewGuid();
        var propertyTypeKey2 = Guid.NewGuid();
        var groupOne = await CreateUserGroup(
            "Group One",
            "groupOne",
            [],
            [],
            [
                new DocumentGranularPermission
                {
                    Key = Guid.Parse(TextpageKey),
                    Permission = "A",
                },
                new DocumentPropertyValueGranularPermission
                {
                    Key = ContentType.Key,
                    Permission = $"{propertyTypeKey}|C",
                },
                new DocumentPropertyValueGranularPermission
                {
                    Key = ContentType.Key,
                    Permission = $"{propertyTypeKey2}|D",
                },
            ],
            Constants.System.Root);
        var groupTwo = await CreateUserGroup(
            "Group Two",
            "groupTwo",
            [],
            [],
            [
                new DocumentPropertyValueGranularPermission
                {
                    Key = ContentType.Key,
                    Permission = $"{propertyTypeKey}|B",
                },
            ],
            Constants.System.Root);
        var user = await CreateUser([groupOne.Key, groupTwo.Key]);

        var model = await UserPresentationFactory.CreateCurrentUserResponseModelAsync(user);
        Assert.That(model.Permissions, Has.Count.EqualTo(3));

        var documentPermissions = model.Permissions
            .Where(x => x is DocumentPermissionPresentationModel)
            .Cast<DocumentPermissionPresentationModel>()
            .Single(x => x.Document.Id == Guid.Parse(TextpageKey));
        Assert.That(documentPermissions.Verbs, Has.Count.EqualTo(1));
        Assert.That(documentPermissions.Verbs.ContainsAll(["A"]), Is.True);

        var documentPropertyValuePermissions = model.Permissions
            .Where(x => x is DocumentPropertyValuePermissionPresentationModel)
            .Cast<DocumentPropertyValuePermissionPresentationModel>()
            .Where(x => x.DocumentType.Id == ContentType.Key);
        Assert.That(documentPropertyValuePermissions.Count(), Is.EqualTo(2));

        var propertyTypePermission1 = documentPropertyValuePermissions
            .Single(x => x.PropertyType.Id == propertyTypeKey);
        Assert.That(propertyTypePermission1.Verbs, Has.Count.EqualTo(2));
        Assert.That(propertyTypePermission1.Verbs.ContainsAll(["B", "C"]), Is.True);

        var propertyTypePermission2 = documentPropertyValuePermissions
            .Single(x => x.PropertyType.Id == propertyTypeKey2);
        Assert.That(propertyTypePermission2.Verbs, Has.Count.EqualTo(1));
        Assert.That(propertyTypePermission2.Verbs.ContainsAll(["D"]), Is.True);
    }

    [Test]
    public async Task Can_Create_Current_User_Response_Model_With_Aggregated_Custom_Permissions()
    {
        var key1 = Guid.NewGuid();
        var key2 = Guid.NewGuid();
        var groupOne = await CreateUserGroup(
            "Group One",
            "groupOne",
            [],
            [],
            [
                new CustomGranularPermission
                {
                    Permission = $"{key1}|A",
                },
                new CustomGranularPermission
                {
                    Permission = $"{key1}|B",
                },
                new CustomGranularPermission
                {
                    Permission = $"{key2}|C",
                }
            ],
            Constants.System.Root);
        var groupTwo = await CreateUserGroup(
            "Group Two",
            "groupTwo",
            [],
            [],
            [
                new CustomGranularPermission
                {
                    Permission = $"{key1}|A",
                },
                new CustomGranularPermission
                {
                    Permission = $"{key2}|B",
                },
            ],
            Constants.System.Root);
        var user = await CreateUser([groupOne.Key, groupTwo.Key]);

        var model = await UserPresentationFactory.CreateCurrentUserResponseModelAsync(user);
        Assert.That(model.Permissions, Has.Count.EqualTo(2));

        var customPermissions = model.Permissions
            .Where(x => x is CustomPermissionPresentationModel)
            .Cast<CustomPermissionPresentationModel>();
        Assert.That(customPermissions.Count(), Is.EqualTo(2));

        var customPermission1 = customPermissions
            .Single(x => x.Key == key1);
        Assert.That(customPermission1.Verbs, Has.Count.EqualTo(2));
        Assert.That(customPermission1.Verbs.ContainsAll(["A", "B"]), Is.True);

        var customPermission2 = customPermissions
            .Single(x => x.Key == key2);
        Assert.That(customPermission2.Verbs, Has.Count.EqualTo(2));
        Assert.That(customPermission2.Verbs.ContainsAll(["B", "C"]), Is.True);
    }

    private class CustomGranularPermission : IGranularPermission
    {
        public const string ContextType = "Custom";

        public string Context => ContextType;

        public required string Permission { get; set; }

        protected bool Equals(CustomGranularPermission other) => Permission == other.Permission;

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((CustomGranularPermission)obj);
        }

        public override int GetHashCode() => HashCode.Combine(Permission);
    }

    private class CustomPermissionPresentationModel : IPermissionPresentationModel
    {
        public required ISet<string> Verbs { get; set; }

        public required Guid Key { get; set; }
    }

    private class CustomPermissionMapper : IPermissionMapper, IPermissionPresentationMapper
    {
        public string Context => CustomGranularPermission.ContextType;

        public Type PresentationModelToHandle => typeof(CustomPermissionPresentationModel);

        public IGranularPermission MapFromDto(UserGroup2GranularPermissionDto dto)
        {
            return new CustomGranularPermission
            {
                Permission = dto.Permission,
            };
        }

        public IEnumerable<IPermissionPresentationModel> MapManyAsync(IEnumerable<IGranularPermission> granularPermissions)
            => granularPermissions
                .Where(x => x is CustomGranularPermission)
                .Cast<CustomGranularPermission>()
                .Select(x => new CustomPermissionPresentationModel
                {
                    Key = Guid.Parse(x.Permission.Split('|')[0]),
                    Verbs = new HashSet<string> { x.Permission.Split('|')[1] },
                });

        public IEnumerable<IGranularPermission> MapToGranularPermissions(IPermissionPresentationModel permissionViewModel)
        {
            if (permissionViewModel is not CustomPermissionPresentationModel customPermissionPresentationModel)
            {
                yield break;
            }

            foreach (var verb in customPermissionPresentationModel.Verbs.Distinct().DefaultIfEmpty(string.Empty))
            {
                yield return new CustomGranularPermission
                {
                    Permission = customPermissionPresentationModel.Key + "|" + verb,
                };
            }
        }

        public IEnumerable<IPermissionPresentationModel> AggregatePresentationModels(IUser user, IEnumerable<IPermissionPresentationModel> models)
        {
            IEnumerable<(Guid Key, ISet<string> Verbs)> groupedModels = models
                .Cast<CustomPermissionPresentationModel>()
                .GroupBy(x => x.Key)
                .Select(x => (x.Key, (ISet<string>)x.SelectMany(y => y.Verbs).Distinct().ToHashSet()));

            foreach ((Guid key, ISet<string> verbs) in groupedModels)
            {
                yield return new CustomPermissionPresentationModel
                {
                    Key = key,
                    Verbs = verbs,
                };
            }
        }
    }

    private async Task<IUserGroup> CreateUserGroup(
        string name,
        string alias,
        int[] allowedLanguages,
        string[] permissions,
        IGranularPermission[] granularPermissions,
        int startMediaId)
    {
        var userGroup = new UserGroupBuilder()
            .WithName(name)
            .WithAlias(alias)
            .WithAllowedLanguages(allowedLanguages)
            .WithStartMediaId(startMediaId)
            .WithPermissions(permissions.ToHashSet())
            .WithGranularPermissions(granularPermissions)
            .Build();
        var createUserGroupResult = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);
        Assert.That(createUserGroupResult.Success, Is.True);
        return userGroup;
    }

    private async Task<IUser> CreateUser(Guid[] userGroupKeys)
    {
        var createUserAttempt = await UserService.CreateAsync(Constants.Security.SuperUserKey, new UserCreateModel
        {
            Email = "test@test.com",
            Name = "Test User",
            UserName = "test@test.com",
            UserGroupKeys = userGroupKeys.ToHashSet(),
        });
        Assert.That(createUserAttempt.Success, Is.True);

        return await UserService.GetAsync(createUserAttempt.Result.CreatedUser.Key);
    }
}
