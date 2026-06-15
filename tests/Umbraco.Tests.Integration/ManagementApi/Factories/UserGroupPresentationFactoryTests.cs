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

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        services.AddTransient<IUserGroupPresentationFactory, UserGroupPresentationFactory>();
        services.AddSingleton<IPermissionPresentationFactory, PermissionPresentationFactory>();
        services.AddSingleton<IPermissionMapper, DocumentPermissionMapper>();
        services.AddSingleton<IPermissionPresentationMapper, DocumentPermissionMapper>();
        services.AddSingleton<IPermissionMapper, DocumentPropertyValuePermissionMapper>();
        services.AddSingleton<IPermissionPresentationMapper, DocumentPropertyValuePermissionMapper>();
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
        Assert.That(attempt.Success, Is.True);

        var userGroupCreateAttempt = await UserGroupService.CreateAsync(attempt.Result, Constants.Security.SuperUserKey);

        var userGroup = userGroupCreateAttempt.Result;

        Assert.Multiple(() =>
        {
            Assert.That(userGroupCreateAttempt.Success, Is.True);
            Assert.That(userGroup, Is.Not.Null);
            Assert.That(userGroup.GranularPermissions, Is.Empty);
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
        Assert.That(attempt.Success, Is.True);

        var userGroupCreateAttempt = await UserGroupService.CreateAsync(attempt.Result, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(userGroupCreateAttempt.Success, Is.False);
            Assert.That(userGroupCreateAttempt.Status, Is.EqualTo(UserGroupOperationStatus.DocumentPermissionKeyNotFound));
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
        Assert.That(attempt.Success, Is.True);

        var userGroupCreateAttempt = await UserGroupService.CreateAsync(attempt.Result, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(userGroupCreateAttempt.Success, Is.False);
            Assert.That(userGroupCreateAttempt.Status, Is.EqualTo(UserGroupOperationStatus.DocumentTypePermissionKeyNotFound));
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
        Assert.That(attempt.Success, Is.True);

        var userGroupCreateAttempt = await UserGroupService.CreateAsync(attempt.Result, Constants.Security.SuperUserKey);
        var userGroup = userGroupCreateAttempt.Result;

        Assert.Multiple(() =>
        {
            Assert.That(userGroupCreateAttempt.Success, Is.True);
            Assert.That(userGroup, Is.Not.Null);
            Assert.That(userGroup.GranularPermissions, Is.Not.Empty);
            Assert.That(userGroup.GranularPermissions.First().Key, Is.EqualTo(contentKey));
            Assert.That(userGroup.GranularPermissions.First().Permission, Is.EqualTo(string.Empty));
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
        Assert.That(attempt.Success, Is.True);

        var userGroupCreateAttempt = await UserGroupService.CreateAsync(attempt.Result, Constants.Security.SuperUserKey);
        var userGroup = userGroupCreateAttempt.Result;

        Assert.Multiple(() =>
        {
            Assert.That(userGroupCreateAttempt.Success, Is.True);
            Assert.That(userGroup, Is.Not.Null);
        });

        Assert.That(userGroup.GranularPermissions, Has.Count.EqualTo(2));
        var documentTypeGranularPermissions = userGroup.GranularPermissions.OfType<DocumentPropertyValueGranularPermission>().ToArray();
        Assert.That(documentTypeGranularPermissions, Has.Length.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(documentTypeGranularPermissions.All(x => x.Key == contentType.Key), Is.True);
            Assert.That(documentTypeGranularPermissions.First().Permission, Is.EqualTo($"{propertyType.Key}|Some"));
            Assert.That(documentTypeGranularPermissions.Last().Permission, Is.EqualTo($"{propertyType.Key}|Another"));
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
        Assert.That(attempt.Success, Is.True);

        var userGroupCreateAttempt = await UserGroupService.CreateAsync(attempt.Result, Constants.Security.SuperUserKey);
        var userGroup = userGroupCreateAttempt.Result;

        Assert.Multiple(() =>
        {
            Assert.That(userGroupCreateAttempt.Success, Is.True);
            Assert.That(userGroup, Is.Not.Null);
        });

        Assert.That(userGroup.GranularPermissions, Has.Count.EqualTo(1));
        var documentTypeGranularPermissions = userGroup.GranularPermissions.OfType<DocumentPropertyValueGranularPermission>().ToArray();
        Assert.That(documentTypeGranularPermissions, Has.Length.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(documentTypeGranularPermissions.All(x => x.Key == contentType.Key), Is.True);
            Assert.That(documentTypeGranularPermissions.First().Permission, Is.EqualTo($"{propertyType.Key}|"));
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
        Assert.That(userGroupCreateAttempt.Success, Is.True);
        Assert.That(userGroupCreateAttempt.Result!.GranularPermissions, Has.Count.EqualTo(4));

        var deleteResult = await GetRequiredService<IContentTypeService>().DeleteAsync(contentType1.Key, Constants.Security.SuperUserKey);
        Assert.That(deleteResult, Is.EqualTo(ContentTypeOperationStatus.Success));

        var userGroup = await UserGroupService.GetAsync(userGroupCreateAttempt.Result!.Key);
        Assert.That(userGroup, Is.Not.Null);

        Assert.That(userGroup.GranularPermissions, Has.Count.EqualTo(2));
    }

    private async Task<Guid> CreateContent()
    {
        // NOTE Maybe not the best way to create/save test data as we are using the services, which are being tested.
        var template = TemplateBuilder.CreateTextPageTemplate("defaultTemplate");
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);
        // Create and Save ContentType "umbTextpage" -> 1051 (template), 1052 (content type)
        var contentTypeCreateModel = ContentTypeEditingBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateKey: template.Key);
        var contentTypeAttempt = await ContentTypeEditingService.CreateAsync(contentTypeCreateModel, Constants.Security.SuperUserKey);
        Assert.That(contentTypeAttempt.Success, Is.True);

        var contentTypeResult = contentTypeAttempt.Result;
        var contentTypeUpdateModel = ContentTypeUpdateHelper.CreateContentTypeUpdateModel(contentTypeResult);
        contentTypeUpdateModel.AllowedContentTypes = new[]
        {
            new ContentTypeSort(contentTypeResult.Key, 0, contentTypeCreateModel.Alias),
        };
        var updatedContentTypeResult = await ContentTypeEditingService.UpdateAsync(contentTypeResult, contentTypeUpdateModel, Constants.Security.SuperUserKey);
        Assert.That(updatedContentTypeResult.Success, Is.True);

        // Create and Save Content "Homepage" based on "umbTextpage" -> 1053
        var textPage = ContentEditingBuilder.CreateSimpleContent(updatedContentTypeResult.Result.Key);
        var createContentResultTextPage = await ContentEditingService.CreateAsync(textPage, Constants.Security.SuperUserKey);
        Assert.That(createContentResultTextPage.Success, Is.True);

        return createContentResultTextPage.Result.Content.Key;
    }
}
