using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Mapping.Permissions;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.AuthorizationStatus;
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

    public IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

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

        services.AddSingleton<IPermissionMapper, ElementPermissionMapper>();
        services.AddSingleton<IPermissionPresentationMapper, ElementPermissionMapper>();

        services.AddSingleton<IPermissionMapper, CustomPermissionMapper>();
        services.AddSingleton<IPermissionPresentationMapper, CustomPermissionMapper>();

        // Decorate the stock element permission service so we can prove the factory routes fallback permissions
        // through IElementPermissionService. Granular resolution still delegates to the real implementation,
        // so the granular aggregation test is unaffected.
        services.AddSingleton<IElementPermissionService>(sp =>
            new FallbackFilteringElementPermissionService(ActivatorUtilities.CreateInstance<ElementPermissionService>(sp)));
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

        Assert.AreEqual(user.Key, model.Id);
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
        Assert.IsTrue(model.HasElementRootAccess);
        Assert.AreEqual(0, model.ElementStartNodeIds.Count);
        Assert.IsFalse(model.HasAccessToSensitiveData);
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

        Assert.AreEqual(4, model.FallbackPermissions.Count);
        Assert.IsTrue(model.FallbackPermissions.ContainsAll(["A", "B", "C", "D"]));

        // When aggregated, we expect one permission per document (we have several granular permissions assigned, for three unique documents).
        Assert.AreEqual(3, model.Permissions.Count);

        // User has two user groups, one of which provides granular permissions for the root content item.
        // As such we expect the aggregated permissions to be the union of the specific permissions coming from the user group with them assigned to the document,
        // and the fallback permissions from the other.
        var rootContentPermissions = model.Permissions.Cast<DocumentPermissionPresentationModel>().Single(x => x.Document.Id == rootContentKey);
        Assert.AreEqual(4, rootContentPermissions.Verbs.Count);
        Assert.IsTrue(rootContentPermissions.Verbs.ContainsAll(["A", "B", "D", "E"]));

        // The sub-page and it's parent have specific granular permissions from one user group.
        // So we expect the aggregated permissions to include those from the sub-page and the other user's groups fallback permissions.
        var subPageContentPermissions = model.Permissions.Cast<DocumentPermissionPresentationModel>().Single(x => x.Document.Id == subPageContentKey);
        Assert.AreEqual(4, subPageContentPermissions.Verbs.Count);
        Assert.IsTrue(subPageContentPermissions.Verbs.ContainsAll(["A", "B", "D", "F"]));

        // Both user groups provide granular permissions for the second sub-page content item.
        // Here we expect the aggregated permissions to be the union of the granular permissions on the document from both user groups.
        var subPage2ContentPermissions = model.Permissions.Cast<DocumentPermissionPresentationModel>().Single(x => x.Document.Id == subPage2ContentKey);
        Assert.AreEqual(3, subPage2ContentPermissions.Verbs.Count);
        Assert.IsTrue(subPage2ContentPermissions.Verbs.ContainsAll(["F", "G", "H"]));
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
        Assert.AreEqual(3, model.Permissions.Count);

        var documentPermissions = model.Permissions
            .Where(x => x is DocumentPermissionPresentationModel)
            .Cast<DocumentPermissionPresentationModel>()
            .Single(x => x.Document.Id == Guid.Parse(TextpageKey));
        Assert.AreEqual(1, documentPermissions.Verbs.Count);
        Assert.IsTrue(documentPermissions.Verbs.ContainsAll(["A"]));

        var documentPropertyValuePermissions = model.Permissions
            .Where(x => x is DocumentPropertyValuePermissionPresentationModel)
            .Cast<DocumentPropertyValuePermissionPresentationModel>()
            .Where(x => x.DocumentType.Id == ContentType.Key);
        Assert.AreEqual(2, documentPropertyValuePermissions.Count());

        var propertyTypePermission1 = documentPropertyValuePermissions
            .Single(x => x.PropertyType.Id == propertyTypeKey);
        Assert.AreEqual(2, propertyTypePermission1.Verbs.Count);
        Assert.IsTrue(propertyTypePermission1.Verbs.ContainsAll(["B", "C"]));

        var propertyTypePermission2 = documentPropertyValuePermissions
            .Single(x => x.PropertyType.Id == propertyTypeKey2);
        Assert.AreEqual(1, propertyTypePermission2.Verbs.Count);
        Assert.IsTrue(propertyTypePermission2.Verbs.ContainsAll(["D"]));
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
        Assert.AreEqual(2, model.Permissions.Count);

        var customPermissions = model.Permissions
            .Where(x => x is CustomPermissionPresentationModel)
            .Cast<CustomPermissionPresentationModel>();
        Assert.AreEqual(2, customPermissions.Count());

        var customPermission1 = customPermissions
            .Single(x => x.Key == key1);
        Assert.AreEqual(2, customPermission1.Verbs.Count);
        Assert.IsTrue(customPermission1.Verbs.ContainsAll(["A", "B"]));

        var customPermission2 = customPermissions
            .Single(x => x.Key == key2);
        Assert.AreEqual(2, customPermission2.Verbs.Count);
        Assert.IsTrue(customPermission2.Verbs.ContainsAll(["B", "C"]));
    }

    [Test]
    public async Task Can_Create_Current_User_Response_Model_With_Aggregated_Element_Permissions()
    {
        // Create an element content type and an element instance.
        var contentType = ContentTypeBuilder.CreateBasicContentType(
            name: Guid.NewGuid().ToString(),
            alias: Guid.NewGuid().ToString());
        contentType.IsElement = true;
        contentType.AllowedInLibrary = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var createResult = await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = contentType.Key,
                ParentKey = null,
                Variants = [new VariantModel { Name = "Test Element" }],
            },
            Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success, $"Failed to create element with status {createResult.Status}.");
        var elementKey = createResult.Result.Content!.Key;

        // One group assigns granular permissions to the element; the other only provides default (fallback) permissions.
        var groupOne = await CreateUserGroup(
            "Group One",
            "groupOne",
            [],
            ["A", "B", "C"],
            [
                new ElementGranularPermission
                {
                    Key = elementKey,
                    Permission = "A",
                },
                new ElementGranularPermission
                {
                    Key = elementKey,
                    Permission = "E",
                }
            ],
            Constants.System.Root);
        var groupTwo = await CreateUserGroup(
            "Group Two",
            "groupTwo",
            [],
            ["A", "B", "D"],
            [],
            Constants.System.Root);

        var user = await CreateUser([groupOne.Key, groupTwo.Key]);

        var model = await UserPresentationFactory.CreateCurrentUserResponseModelAsync(user);

        // One aggregated permission for the single element that has granular permissions assigned.
        var elementPermissions = model.Permissions.OfType<ElementPermissionPresentationModel>().ToArray();
        Assert.That(elementPermissions, Has.Length.EqualTo(1));

        // The element has explicit granular permissions from group one (A, E) and inherits group two's
        // fallback permissions (A, B, D), so the aggregate is the union of both.
        var elementPermission = elementPermissions.Single(x => x.Element.Id == elementKey);
        Assert.That(elementPermission.Verbs, Is.EquivalentTo(new[] { "A", "B", "D", "E" }));
    }

    [Test]
    public async Task Can_Create_Current_User_Response_Model_Filtering_Element_Fallback_Permissions_Through_Service()
    {
        // A group whose default (fallback) permissions include the element verb that the decorator strips.
        var group = await CreateUserGroup(
            "Group One",
            "groupOne",
            [],
            [FallbackFilteringElementPermissionService.StrippedVerb, "Umb.Element.Read"],
            [],
            Constants.System.Root);
        var user = await CreateUser([group.Key]);

        var model = await UserPresentationFactory.CreateCurrentUserResponseModelAsync(user);

        // The factory routed the fallback set through IElementPermissionService, so the stripped verb is gone
        // while other fallback verbs remain. This is what hides default-permission actions (e.g. Trash) in the UI.
        Assert.That(model.FallbackPermissions, Does.Not.Contain(FallbackFilteringElementPermissionService.StrippedVerb));
        Assert.That(model.FallbackPermissions, Contains.Item("Umb.Element.Read"));
    }

    private sealed class FallbackFilteringElementPermissionService : IElementPermissionService
    {
        public const string StrippedVerb = "Umb.Element.Delete";

        private readonly IElementPermissionService _inner;

        public FallbackFilteringElementPermissionService(IElementPermissionService inner) => _inner = inner;

        public Task<IEnumerable<NodePermissions>> GetPermissionsAsync(IUser user, IEnumerable<Guid> elementKeys)
            => _inner.GetPermissionsAsync(user, elementKeys);

        public async Task<ISet<string>> FilterFallbackPermissionsAsync(IUser user, ISet<string> fallbackPermissions)
        {
            ISet<string> filtered = await _inner.FilterFallbackPermissionsAsync(user, fallbackPermissions);
            filtered.Remove(StrippedVerb);
            return filtered;
        }

        public Task<ElementAuthorizationStatus> AuthorizeAccessAsync(IUser user, IEnumerable<Guid> elementKeys, ISet<string> permissionsToCheck)
            => _inner.AuthorizeAccessAsync(user, elementKeys, permissionsToCheck);

        public Task<ElementAuthorizationStatus> AuthorizeDescendantsAccessAsync(IUser user, Guid parentKey, ISet<string> permissionsToCheck)
            => _inner.AuthorizeDescendantsAccessAsync(user, parentKey, permissionsToCheck);

        public Task<ElementAuthorizationStatus> AuthorizeRootAccessAsync(IUser user, ISet<string> permissionsToCheck)
            => _inner.AuthorizeRootAccessAsync(user, permissionsToCheck);

        public Task<ElementAuthorizationStatus> AuthorizeBinAccessAsync(IUser user, ISet<string> permissionsToCheck)
            => _inner.AuthorizeBinAccessAsync(user, permissionsToCheck);

        public Task<ElementAuthorizationStatus> AuthorizeCultureAccessAsync(IUser user, ISet<string> culturesToCheck)
            => _inner.AuthorizeCultureAccessAsync(user, culturesToCheck);
    }

    private class CustomGranularPermission : IGranularPermission
    {
        public const string ContextType = "Custom";

        public string Context => ContextType;

        public required string Permission { get; set; }

        protected bool Equals(CustomGranularPermission other) => Permission == other.Permission;

        public override bool Equals(object? obj)
        {
            if (obj is null)
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
        Assert.IsTrue(createUserGroupResult.Success);
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
        Assert.IsTrue(createUserAttempt.Success);

        return await UserService.GetAsync(createUserAttempt.Result.CreatedUser.Key);
    }
}
