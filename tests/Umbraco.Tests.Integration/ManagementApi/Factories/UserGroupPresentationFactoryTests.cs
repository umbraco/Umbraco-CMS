using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Mapping.Permissions;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Factories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class UserGroupPresentationFactoryTests : UmbracoIntegrationTest
{
    public IUserGroupPresentationFactory UserGroupPresentationFactory => GetRequiredService<IUserGroupPresentationFactory>();

    public IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    public ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    public IContentTypeEditingService ContentTypeEditingService => GetRequiredService<IContentTypeEditingService>();

    public IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    public IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        services.AddTransient<IUserGroupPresentationFactory, UserGroupPresentationFactory>();
        services.AddSingleton<IPermissionPresentationFactory, PermissionPresentationFactory>();
        services.AddSingleton<IPermissionMapper, DocumentPermissionMapper>();
        services.AddSingleton<IPermissionPresentationMapper, DocumentPermissionMapper>();
        services.AddSingleton<IPermissionMapper, DocumentPropertyValuePermissionMapper>();
        services.AddSingleton<IPermissionPresentationMapper, DocumentPropertyValuePermissionMapper>();
        services.AddSingleton<IPermissionMapper, ElementContainerPermissionMapper>();
        services.AddSingleton<IPermissionPresentationMapper, ElementContainerPermissionMapper>();
    }

    [Test]
    public async Task Can_Map_Create_Model_And_Create()
    {
        var createModel = new CreateUserGroupRequestModel()
        {
            Alias = "testAlias",
            FallbackPermissions = new HashSet<string>(),
            HasAccessToAllLanguages = true,
            Languages = new List<string>(),
            Name = "Test Name",
            Sections = new[] { "Umb.Section.Content" },
            Permissions = new HashSet<IPermissionPresentationModel>()
        };

        var attempt = await UserGroupPresentationFactory.CreateAsync(createModel);
        Assert.IsTrue(attempt.Success);

        var userGroupCreateAttempt = await UserGroupService.CreateAsync(attempt.Result, Constants.Security.SuperUserKey);

        var userGroup = userGroupCreateAttempt.Result;

        Assert.Multiple(() =>
        {
            Assert.IsTrue(userGroupCreateAttempt.Success);
            Assert.IsNotNull(userGroup);
            Assert.IsEmpty(userGroup.GranularPermissions);
        });
    }

    [Test]
    public async Task Cannot_Create_UserGroup_With_Unexisting_Document_Reference()
    {
        var createModel = new CreateUserGroupRequestModel()
        {
            Alias = "testAlias",
            FallbackPermissions = new HashSet<string>(),
            HasAccessToAllLanguages = true,
            Languages = new List<string>(),
            Name = "Test Name",
            Sections = new[] { "Umb.Section.Content" },
            Permissions = new HashSet<IPermissionPresentationModel>()
            {
                new DocumentPermissionPresentationModel()
                {
                    Document = new ReferenceByIdModel(Guid.NewGuid()),
                    Verbs = new HashSet<string>()
                }
            }
        };

        var attempt = await UserGroupPresentationFactory.CreateAsync(createModel);
        Assert.IsTrue(attempt.Success);

        var userGroupCreateAttempt = await UserGroupService.CreateAsync(attempt.Result, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(userGroupCreateAttempt.Success);
            Assert.AreEqual(UserGroupOperationStatus.DocumentPermissionKeyNotFound, userGroupCreateAttempt.Status);
        });
    }

    [Test]
    public async Task Cannot_Create_UserGroup_With_Unexisting_DocumentType_Reference()
    {
        var updateModel = new CreateUserGroupRequestModel()
        {
            Alias = "testAlias",
            FallbackPermissions = new HashSet<string>(),
            HasAccessToAllLanguages = true,
            Languages = new List<string>(),
            Name = "Test Name",
            Sections = new[] { "Umb.Section.Content" },
            Permissions = new HashSet<IPermissionPresentationModel>()
            {
                new DocumentPropertyValuePermissionPresentationModel()
                {
                    DocumentType = new ReferenceByIdModel(Guid.NewGuid()),
                    PropertyType = new ReferenceByIdModel(Guid.NewGuid()),
                    Verbs = new HashSet<string>()
                }
            }
        };

        var attempt = await UserGroupPresentationFactory.CreateAsync(updateModel);
        Assert.IsTrue(attempt.Success);

        var userGroupCreateAttempt = await UserGroupService.CreateAsync(attempt.Result, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(userGroupCreateAttempt.Success);
            Assert.AreEqual(UserGroupOperationStatus.DocumentTypePermissionKeyNotFound, userGroupCreateAttempt.Status);
        });
    }

    [Test]
    public async Task Can_Create_Usergroup_With_Empty_Granular_Permissions_For_Document()
    {
        var contentKey = await CreateContent();

        var createModel = new CreateUserGroupRequestModel()
        {
            Alias = "testAlias",
            FallbackPermissions = new HashSet<string>(),
            HasAccessToAllLanguages = true,
            Languages = new List<string>(),
            Name = "Test Name",
            Sections = new[] { "Umb.Section.Content" },
            Permissions = new HashSet<IPermissionPresentationModel>
            {
                new DocumentPermissionPresentationModel()
                {
                    Document = new ReferenceByIdModel(contentKey),
                    Verbs = new HashSet<string>()
                }
            }
        };

        var attempt = await UserGroupPresentationFactory.CreateAsync(createModel);
        Assert.IsTrue(attempt.Success);

        var userGroupCreateAttempt = await UserGroupService.CreateAsync(attempt.Result, Constants.Security.SuperUserKey);
        var userGroup = userGroupCreateAttempt.Result;

        Assert.Multiple(() =>
        {
            Assert.IsTrue(userGroupCreateAttempt.Success);
            Assert.IsNotNull(userGroup);
            Assert.IsNotEmpty(userGroup.GranularPermissions);
            Assert.AreEqual(contentKey, userGroup.GranularPermissions.First().Key);
            Assert.AreEqual(string.Empty, userGroup.GranularPermissions.First().Permission);
        });
    }

    [Test]
    public async Task Can_Create_Usergroup_With_Granular_Permissions_For_Document_PropertyValue()
    {
        var template = TemplateBuilder.CreateTextPageTemplate("defaultTemplate");
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType = (await ContentTypeEditingService.CreateAsync(
            ContentTypeEditingBuilder.CreateSimpleContentType(defaultTemplateKey: template.Key),
            Constants.Security.SuperUserKey)).Result!;

        var propertyType = contentType.PropertyTypes.First();

        var updateModel = new CreateUserGroupRequestModel()
        {
            Alias = "testAlias",
            FallbackPermissions = new HashSet<string>(),
            HasAccessToAllLanguages = true,
            Languages = new List<string>(),
            Name = "Test Name",
            Sections = new[] { "Umb.Section.Content" },
            Permissions = new HashSet<IPermissionPresentationModel>
            {
                new DocumentPropertyValuePermissionPresentationModel
                {
                    DocumentType = new ReferenceByIdModel(contentType.Key),
                    PropertyType = new ReferenceByIdModel(propertyType.Key),
                    Verbs = new HashSet<string>(["Some", "Another"])
                }
            }
        };

        var attempt = await UserGroupPresentationFactory.CreateAsync(updateModel);
        Assert.IsTrue(attempt.Success);

        var userGroupCreateAttempt = await UserGroupService.CreateAsync(attempt.Result, Constants.Security.SuperUserKey);
        var userGroup = userGroupCreateAttempt.Result;

        Assert.Multiple(() =>
        {
            Assert.IsTrue(userGroupCreateAttempt.Success);
            Assert.IsNotNull(userGroup);
        });

        Assert.AreEqual(2, userGroup.GranularPermissions.Count);
        var documentTypeGranularPermissions = userGroup.GranularPermissions.OfType<DocumentPropertyValueGranularPermission>().ToArray();
        Assert.AreEqual(2, documentTypeGranularPermissions.Length);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(documentTypeGranularPermissions.All(x => x.Key == contentType.Key));
            Assert.AreEqual($"{propertyType.Key}|Some", documentTypeGranularPermissions.First().Permission);
            Assert.AreEqual($"{propertyType.Key}|Another", documentTypeGranularPermissions.Last().Permission);
        });
    }

    [Test]
    public async Task Can_Create_Usergroup_With_Granular_Permissions_For_Document_PropertyValue_Without_Verbs()
    {
        var template = TemplateBuilder.CreateTextPageTemplate("defaultTemplate");
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType = (await ContentTypeEditingService.CreateAsync(
            ContentTypeEditingBuilder.CreateSimpleContentType(defaultTemplateKey: template.Key),
            Constants.Security.SuperUserKey)).Result!;

        var propertyType = contentType.PropertyTypes.First();

        var updateModel = new CreateUserGroupRequestModel()
        {
            Alias = "testAlias",
            FallbackPermissions = new HashSet<string>(),
            HasAccessToAllLanguages = true,
            Languages = new List<string>(),
            Name = "Test Name",
            Sections = new[] { "Umb.Section.Content" },
            Permissions = new HashSet<IPermissionPresentationModel>
            {
                new DocumentPropertyValuePermissionPresentationModel
                {
                    DocumentType = new ReferenceByIdModel(contentType.Key),
                    PropertyType = new ReferenceByIdModel(propertyType.Key),
                    Verbs = new HashSet<string>()
                }
            }
        };

        var attempt = await UserGroupPresentationFactory.CreateAsync(updateModel);
        Assert.IsTrue(attempt.Success);

        var userGroupCreateAttempt = await UserGroupService.CreateAsync(attempt.Result, Constants.Security.SuperUserKey);
        var userGroup = userGroupCreateAttempt.Result;

        Assert.Multiple(() =>
        {
            Assert.IsTrue(userGroupCreateAttempt.Success);
            Assert.IsNotNull(userGroup);
        });

        Assert.AreEqual(1, userGroup.GranularPermissions.Count);
        var documentTypeGranularPermissions = userGroup.GranularPermissions.OfType<DocumentPropertyValueGranularPermission>().ToArray();
        Assert.AreEqual(1, documentTypeGranularPermissions.Length);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(documentTypeGranularPermissions.All(x => x.Key == contentType.Key));
            Assert.AreEqual($"{propertyType.Key}|", documentTypeGranularPermissions.First().Permission);
        });
    }

    [Test]
    public async Task Usergroup_Granular_Permissions_For_Document_PropertyValue_Are_Cleaned_Up_When_DocumentType_Is_Deleted()
    {
        var template = TemplateBuilder.CreateTextPageTemplate("defaultTemplate");
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType1 = (await ContentTypeEditingService.CreateAsync(
            ContentTypeEditingBuilder.CreateSimpleContentType(defaultTemplateKey: template.Key),
            Constants.Security.SuperUserKey)).Result!;

        var contentType2 = (await ContentTypeEditingService.CreateAsync(
            ContentTypeEditingBuilder.CreateSimpleContentType(alias: "anotherAlias", defaultTemplateKey: template.Key),
            Constants.Security.SuperUserKey)).Result!;

        var propertyType1 = contentType1.PropertyTypes.First();
        var propertyType2 = contentType2.PropertyTypes.First();

        var updateModel = new CreateUserGroupRequestModel()
        {
            Alias = "testAlias",
            FallbackPermissions = new HashSet<string>(),
            HasAccessToAllLanguages = true,
            Languages = new List<string>(),
            Name = "Test Name",
            Sections = new[] { "Umb.Section.Content" },
            Permissions = new HashSet<IPermissionPresentationModel>
            {
                new DocumentPropertyValuePermissionPresentationModel
                {
                    DocumentType = new ReferenceByIdModel(contentType1.Key),
                    PropertyType = new ReferenceByIdModel(propertyType1.Key),
                    Verbs = new HashSet<string>(["Some", "Another"])
                },
                new DocumentPropertyValuePermissionPresentationModel
                {
                    DocumentType = new ReferenceByIdModel(contentType2.Key),
                    PropertyType = new ReferenceByIdModel(propertyType2.Key),
                    Verbs = new HashSet<string>(["Even", "More"])
                }
            }
        };

        var attempt = await UserGroupPresentationFactory.CreateAsync(updateModel);

        var userGroupCreateAttempt = await UserGroupService.CreateAsync(attempt.Result, Constants.Security.SuperUserKey);
        Assert.IsTrue(userGroupCreateAttempt.Success);
        Assert.AreEqual(4, userGroupCreateAttempt.Result!.GranularPermissions.Count);

        var deleteResult = await GetRequiredService<IContentTypeService>().DeleteAsync(contentType1.Key, Constants.Security.SuperUserKey);
        Assert.AreEqual(ContentTypeOperationStatus.Success, deleteResult);

        var userGroup = await UserGroupService.GetAsync(userGroupCreateAttempt.Result!.Key);
        Assert.IsNotNull(userGroup);

        Assert.AreEqual(2, userGroup.GranularPermissions.Count);
    }

    [Test]
    public async Task Can_Create_Usergroup_With_Combined_Granular_Permissions_For_ElementContainer()
    {
        var containerResult = await ElementContainerService.CreateAsync(
            null,
            "Test Container",
            null,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(containerResult.Success);
        var folderKey = containerResult.Result!.Key;

        var createModel = new CreateUserGroupRequestModel
        {
            Alias = "testAlias",
            FallbackPermissions = new HashSet<string>(),
            HasAccessToAllLanguages = true,
            Languages = new List<string>(),
            Name = "Test Name",
            Sections = ["Umb.Section.Content"],
            Permissions = new HashSet<IPermissionPresentationModel>
            {
                new ElementContainerPermissionPresentationModel
                {
                    ElementContainer = new ReferenceByIdModel(folderKey),
                    Verbs = new HashSet<string>(["Umb.ElementContainer.Create", "Umb.Element.Create"]),
                },
            },
        };

        var attempt = await UserGroupPresentationFactory.CreateAsync(createModel);
        Assert.IsTrue(attempt.Success);

        var userGroupCreateAttempt = await UserGroupService.CreateAsync(attempt.Result, Constants.Security.SuperUserKey);
        var userGroup = userGroupCreateAttempt.Result;

        Assert.Multiple(() =>
        {
            Assert.IsTrue(userGroupCreateAttempt.Success);
            Assert.IsNotNull(userGroup);
        });

        var containerGranularPermissions = userGroup!.GranularPermissions.OfType<ElementContainerGranularPermission>().ToArray();
        Assert.AreEqual(2, containerGranularPermissions.Length);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(containerGranularPermissions.All(x => x.Key == folderKey));
            Assert.That(
                containerGranularPermissions.Select(x => x.Permission),
                Is.EquivalentTo(new[] { "Umb.ElementContainer.Create", "Umb.Element.Create" }));
        });

        // Reload to prove the rows persist and deserialize back via MapFromDto, not just the in-memory save object.
        var reloadedGroup = await UserGroupService.GetAsync(userGroup.Key);
        Assert.IsNotNull(reloadedGroup);

        var reloadedContainerGranularPermissions = reloadedGroup!.GranularPermissions.OfType<ElementContainerGranularPermission>().ToArray();
        Assert.AreEqual(2, reloadedContainerGranularPermissions.Length);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(reloadedContainerGranularPermissions.All(x => x.Key == folderKey));
            Assert.That(
                reloadedContainerGranularPermissions.Select(x => x.Permission),
                Is.EquivalentTo(new[] { "Umb.ElementContainer.Create", "Umb.Element.Create" }));
        });
    }

    [Test]
    public async Task Usergroup_Granular_Permissions_For_ElementContainer_Merge_Back_Into_One_Presentation_Model()
    {
        var containerResult = await ElementContainerService.CreateAsync(
            null,
            "Test Container",
            null,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(containerResult.Success);
        var folderKey = containerResult.Result!.Key;

        var createModel = new CreateUserGroupRequestModel
        {
            Alias = "testAlias",
            FallbackPermissions = new HashSet<string>(),
            HasAccessToAllLanguages = true,
            Languages = new List<string>(),
            Name = "Test Name",
            Sections = ["Umb.Section.Content"],
            Permissions = new HashSet<IPermissionPresentationModel>
            {
                new ElementContainerPermissionPresentationModel
                {
                    ElementContainer = new ReferenceByIdModel(folderKey),
                    Verbs = new HashSet<string>(["Umb.ElementContainer.Create", "Umb.Element.Create"]),
                },
            },
        };

        var attempt = await UserGroupPresentationFactory.CreateAsync(createModel);
        var userGroupCreateAttempt = await UserGroupService.CreateAsync(attempt.Result, Constants.Security.SuperUserKey);
        var userGroup = userGroupCreateAttempt.Result;
        Assert.IsTrue(userGroupCreateAttempt.Success);

        var responseModel = await UserGroupPresentationFactory.CreateAsync(userGroup);
        var elementContainerPresentationModels = responseModel.Permissions.OfType<ElementContainerPermissionPresentationModel>().ToArray();
        Assert.AreEqual(1, elementContainerPresentationModels.Length);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(folderKey, elementContainerPresentationModels[0].ElementContainer.Id);
            Assert.That(
                elementContainerPresentationModels[0].Verbs,
                Is.EquivalentTo(new[] { "Umb.ElementContainer.Create", "Umb.Element.Create" }));
        });
    }

    [Test]
    public async Task Can_Create_Usergroup_With_Empty_Granular_Permissions_For_ElementContainer()
    {
        var containerResult = await ElementContainerService.CreateAsync(
            null,
            "Test Container",
            null,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(containerResult.Success);
        var folderKey = containerResult.Result!.Key;

        var createModel = new CreateUserGroupRequestModel
        {
            Alias = "testAlias",
            FallbackPermissions = new HashSet<string>(),
            HasAccessToAllLanguages = true,
            Languages = new List<string>(),
            Name = "Test Name",
            Sections = ["Umb.Section.Content"],
            Permissions = new HashSet<IPermissionPresentationModel>
            {
                new ElementContainerPermissionPresentationModel
                {
                    ElementContainer = new ReferenceByIdModel(folderKey),
                    Verbs = new HashSet<string>(),
                },
            },
        };

        var attempt = await UserGroupPresentationFactory.CreateAsync(createModel);
        Assert.IsTrue(attempt.Success);

        var userGroupCreateAttempt = await UserGroupService.CreateAsync(attempt.Result, Constants.Security.SuperUserKey);
        var userGroup = userGroupCreateAttempt.Result;

        Assert.Multiple(() =>
        {
            Assert.IsTrue(userGroupCreateAttempt.Success);
            Assert.IsNotNull(userGroup);
            Assert.IsNotEmpty(userGroup.GranularPermissions);
            Assert.AreEqual(folderKey, userGroup.GranularPermissions.First().Key);
            Assert.AreEqual(string.Empty, userGroup.GranularPermissions.First().Permission);
        });

        var reloadedGroup = await UserGroupService.GetAsync(userGroup.Key);
        Assert.IsNotNull(reloadedGroup);

        var reloadedPermission = reloadedGroup!.GranularPermissions.OfType<ElementContainerGranularPermission>().SingleOrDefault();
        Assert.IsNotNull(reloadedPermission);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(folderKey, reloadedPermission!.Key);
            Assert.AreEqual(string.Empty, reloadedPermission!.Permission);
        });
    }

    [Test]
    public async Task Usergroup_Granular_Permissions_For_ElementContainer_Are_Cleaned_Up_When_ElementContainer_Is_Deleted()
    {
        var container1 = (await ElementContainerService.CreateAsync(
            null,
            "Test Container 1",
            null,
            Constants.Security.SuperUserKey)).Result!;

        var container2 = (await ElementContainerService.CreateAsync(
            null,
            "Test Container 2",
            null,
            Constants.Security.SuperUserKey)).Result!;

        var createModel = new CreateUserGroupRequestModel
        {
            Alias = "testAlias",
            FallbackPermissions = new HashSet<string>(),
            HasAccessToAllLanguages = true,
            Languages = new List<string>(),
            Name = "Test Name",
            Sections = ["Umb.Section.Content"],
            Permissions = new HashSet<IPermissionPresentationModel>
            {
                new ElementContainerPermissionPresentationModel
                {
                    ElementContainer = new ReferenceByIdModel(container1.Key),
                    Verbs = new HashSet<string>(["Umb.ElementContainer.Create", "Umb.Element.Create"]),
                },
                new ElementContainerPermissionPresentationModel
                {
                    ElementContainer = new ReferenceByIdModel(container2.Key),
                    Verbs = new HashSet<string>(["Umb.ElementContainer.Create", "Umb.Element.Create"]),
                },
            },
        };

        var attempt = await UserGroupPresentationFactory.CreateAsync(createModel);
        Assert.IsTrue(attempt.Success);

        var userGroupCreateAttempt = await UserGroupService.CreateAsync(attempt.Result, Constants.Security.SuperUserKey);
        Assert.IsTrue(userGroupCreateAttempt.Success);
        Assert.AreEqual(4, userGroupCreateAttempt.Result!.GranularPermissions.Count);

        var deleteResult = await ElementContainerService.DeleteAsync(container1.Key, Constants.Security.SuperUserKey);
        Assert.IsTrue(deleteResult.Success);

        var userGroup = await UserGroupService.GetAsync(userGroupCreateAttempt.Result!.Key);
        Assert.IsNotNull(userGroup);

        Assert.AreEqual(2, userGroup!.GranularPermissions.Count);
        Assert.IsTrue(userGroup.GranularPermissions.OfType<ElementContainerGranularPermission>().All(p => p.Key == container2.Key));
    }

    private async Task<Guid> CreateContent()
    {
        // NOTE Maybe not the best way to create/save test data as we are using the services, which are being tested.
        var template = TemplateBuilder.CreateTextPageTemplate("defaultTemplate");
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);
        // Create and Save ContentType "umbTextpage" -> 1051 (template), 1052 (content type)
        var contentTypeCreateModel = ContentTypeEditingBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateKey: template.Key);
        var contentTypeAttempt = await ContentTypeEditingService.CreateAsync(contentTypeCreateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(contentTypeAttempt.Success);

        var contentTypeResult = contentTypeAttempt.Result;
        var contentTypeUpdateModel = ContentTypeUpdateHelper.CreateContentTypeUpdateModel(contentTypeResult);
        contentTypeUpdateModel.AllowedContentTypes = new[]
        {
            new ContentTypeSort(contentTypeResult.Key, 0, contentTypeCreateModel.Alias),
        };
        var updatedContentTypeResult = await ContentTypeEditingService.UpdateAsync(contentTypeResult, contentTypeUpdateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(updatedContentTypeResult.Success);

        // Create and Save Content "Homepage" based on "umbTextpage" -> 1053
        var textPage = ContentEditingBuilder.CreateSimpleContent(updatedContentTypeResult.Result.Key);
        var createContentResultTextPage = await ContentEditingService.CreateAsync(textPage, Constants.Security.SuperUserKey);
        Assert.IsTrue(createContentResultTextPage.Success);

        return createContentResultTextPage.Result.Content.Key;
    }
}
