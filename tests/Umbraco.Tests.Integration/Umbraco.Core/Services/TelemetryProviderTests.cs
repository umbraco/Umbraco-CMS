// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Telemetry.Providers;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

/// <summary>
///     Tests covering the SectionService
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class TelemetryProviderTests : UmbracoIntegrationTest
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IFileService FileService => GetRequiredService<IFileService>();

    private IDomainService DomainService => GetRequiredService<IDomainService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private DomainTelemetryProvider DetailedTelemetryProviders => GetRequiredService<DomainTelemetryProvider>();

    private ContentTelemetryProvider ContentTelemetryProvider => GetRequiredService<ContentTelemetryProvider>();

    private LanguagesTelemetryProvider LanguagesTelemetryProvider => GetRequiredService<LanguagesTelemetryProvider>();

    private UserTelemetryProvider UserTelemetryProvider => GetRequiredService<UserTelemetryProvider>();

    private MediaTelemetryProvider MediaTelemetryProvider => GetRequiredService<MediaTelemetryProvider>();

    private PropertyEditorTelemetryProvider PropertyEditorTelemetryProvider =>
        GetRequiredService<PropertyEditorTelemetryProvider>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private IUserService UserService => GetRequiredService<IUserService>();

    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private readonly LanguageBuilder _languageBuilder = new();

    private readonly UserBuilder _userBuilder = new();

    private readonly UserGroupBuilder _userGroupBuilder = new();

    private readonly ContentTypeBuilder _contentTypeBuilder = new();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<DomainTelemetryProvider>();
        builder.Services.AddTransient<ContentTelemetryProvider>();
        builder.Services.AddTransient<LanguagesTelemetryProvider>();
        builder.Services.AddTransient<UserTelemetryProvider>();
        builder.Services.AddTransient<MediaTelemetryProvider>();
        builder.Services.AddTransient<PropertyEditorTelemetryProvider>();
        base.CustomTestSetup(builder);
    }

    [Test]
    public async Task Domain_Telemetry_Provider_Can_Get_Domains()
    {
        // Arrange
        var contentType = ContentTypeBuilder.CreateBasicContentType();
        ContentTypeService.Save(contentType);

        var content = ContentBuilder.CreateBasicContent(contentType);
        ContentService.Save(content);

        await DomainService.UpdateDomainsAsync(
            content.Key,
            new DomainsUpdateModel
            {
                Domains = new[] { new DomainModel { DomainName = "english", IsoCode = "en-US" } }
            });

        // Act
        IEnumerable<UsageInformation> result = DetailedTelemetryProviders.GetInformation();

        // Assert
        Assert.AreEqual(1, result.First().Data);
        Assert.AreEqual("DomainCount", result.First().Name);
    }

    [Test]
    public void SectionService_Can_Get_Allowed_Sections_For_User()
    {
        // Arrange
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType);

        var blueprint = ContentBuilder.CreateTextpageContent(contentType, "hello", Constants.System.Root);
        blueprint.SetValue("title", "blueprint 1");
        blueprint.SetValue("bodyText", "blueprint 2");
        blueprint.SetValue("keywords", "blueprint 3");
        blueprint.SetValue("description", "blueprint 4");

        ContentService.SaveBlueprint(blueprint);

        var fromBlueprint = ContentService.CreateContentFromBlueprint(blueprint, "My test content");
        ContentService.Save(fromBlueprint);

        IEnumerable<UsageInformation> result = null;
        // Act
        result = ContentTelemetryProvider.GetInformation();


        // Assert
        // TODO : Test multiple roots, with children + grandchildren
        Assert.AreEqual(1, result.First().Data);
    }

    [Test]
    public async Task Language_Telemetry_Can_Get_Languages()
    {
        // Arrange
        var langTwo = _languageBuilder.WithCultureInfo("da-DK").Build();
        var langThree = _languageBuilder.WithCultureInfo("sv-SE").Build();

        var langTwoResult = await LanguageService.CreateAsync(langTwo, Constants.Security.SuperUserKey);
        Assert.IsTrue(langTwoResult.Success);
        var langThreeResult = await LanguageService.CreateAsync(langThree, Constants.Security.SuperUserKey);
        Assert.IsTrue(langThreeResult.Success);

        IEnumerable<UsageInformation> result = null;

        // Act
        result = LanguagesTelemetryProvider.GetInformation();

        // Assert
        Assert.AreEqual(3, result.First().Data);
    }

    [Test]
    public void MediaTelemetry_Can_Get_Media_In_Folders()
    {
        var folderType = MediaTypeService.Get(1031);
        var imageMediaType = MediaTypeService.Get(1032);

        var root = MediaBuilder.CreateMediaFolder(folderType, -1);
        MediaService.Save(root);
        var createdMediaCount = 10;
        for (var i = 0; i < createdMediaCount; i++)
        {
            var c1 = MediaBuilder.CreateMediaImage(imageMediaType, root.Id);
            MediaService.Save(c1);
        }

        var result = MediaTelemetryProvider.GetInformation()
            .FirstOrDefault(x => x.Name == Constants.Telemetry.MediaCount);
        Assert.AreEqual(createdMediaCount, result.Data);
    }

    [Test]
    public void MediaTelemetry_Can_Get_Media_In_Root()
    {
        var imageMediaType = MediaTypeService.Get(1032);
        var createdMediaCount = 10;
        for (var i = 0; i < createdMediaCount; i++)
        {
            var c1 = MediaBuilder.CreateMediaImage(imageMediaType, -1);
            MediaService.Save(c1);
        }

        var result = MediaTelemetryProvider.GetInformation()
            .FirstOrDefault(x => x.Name == Constants.Telemetry.MediaCount);
        Assert.AreEqual(createdMediaCount, result.Data);
    }

    [Test]
    public void PropertyEditorTelemetry_Counts_Same_Editor_As_One()
    {
        var ct2 = ContentTypeBuilder.CreateBasicContentType("ct2", "CT2");
        AddPropType("title", -88, ct2);
        var ct3 = ContentTypeBuilder.CreateBasicContentType("ct3", "CT3");
        AddPropType("title", -88, ct3);
        var ct5 = ContentTypeBuilder.CreateBasicContentType("ct5", "CT5");
        AddPropType("blah", -88, ct5);

        ContentTypeService.Save(ct2);
        ContentTypeService.Save(ct3);
        ContentTypeService.Save(ct5);

        var properties = PropertyEditorTelemetryProvider.GetInformation()
            .FirstOrDefault(x => x.Name == Constants.Telemetry.Properties);
        var result = properties.Data as IEnumerable<string>;
        Assert.AreEqual(1, result?.Count());
    }

    [Test]
    public void PropertyEditorTelemetry_Can_Get_All_PropertyTypes()
    {
        var ct2 = ContentTypeBuilder.CreateBasicContentType("ct2", "CT2");
        AddPropType("title", -88, ct2);
        AddPropType("title", -99, ct2);
        var ct5 = ContentTypeBuilder.CreateBasicContentType("ct5", "CT5");
        AddPropType("blah", -88, ct5);

        ContentTypeService.Save(ct2);
        ContentTypeService.Save(ct5);

        var properties = PropertyEditorTelemetryProvider.GetInformation()
            .FirstOrDefault(x => x.Name == Constants.Telemetry.Properties);
        var result = properties.Data as IEnumerable<string>;
        Assert.AreEqual(2, result?.Count());
    }

    [Test]
    public void UserTelemetry_Can_Get_Default_User()
    {
        var result = UserTelemetryProvider.GetInformation()
            .FirstOrDefault(x => x.Name == Constants.Telemetry.UserCount);

        Assert.AreEqual(1, result.Data);
    }

    [Test]
    public void UserTelemetry_Can_Get_With_Saved_User()
    {
        var user = BuildUser(0);

        UserService.Save(user);

        var result = UserTelemetryProvider.GetInformation()
            .FirstOrDefault(x => x.Name == Constants.Telemetry.UserCount);

        Assert.AreEqual(2, result.Data);
    }

    [Test]
    public void UserTelemetry_Can_Get_More_Users()
    {
        var totalUsers = 99;
        var users = BuildUsers(totalUsers);
        UserService.Save(users);

        var result = UserTelemetryProvider.GetInformation()
            .FirstOrDefault(x => x.Name == Constants.Telemetry.UserCount);

        Assert.AreEqual(totalUsers + 1, result.Data);
    }

    [Test]
    public void UserTelemetry_Can_Get_Default_UserGroups()
    {
        var result = UserTelemetryProvider.GetInformation()
            .FirstOrDefault(x => x.Name == Constants.Telemetry.UserGroupCount);
        Assert.AreEqual(5, result.Data);
    }

    [Test]
    public async Task UserTelemetry_Can_Get_With_Saved_UserGroups()
    {
        var userGroup = BuildUserGroup("testGroup");

        await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);
        var result = UserTelemetryProvider.GetInformation()
            .FirstOrDefault(x => x.Name == Constants.Telemetry.UserGroupCount);

        Assert.AreEqual(6, result.Data);
    }

    [Test]
    public async Task UserTelemetry_Can_Get_More_UserGroups()
    {
        var userGroups = BuildUserGroups(100);

        foreach (var userGroup in userGroups)
        {
            await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);
        }

        var result = UserTelemetryProvider.GetInformation()
            .FirstOrDefault(x => x.Name == Constants.Telemetry.UserGroupCount);

        Assert.AreEqual(105, result.Data);
    }

    private User BuildUser(int count) =>
        _userBuilder
            .WithLogin($"username{count}", "test pass")
            .WithName("Test" + count)
            .WithEmail($"test{count}@test.com")
            .Build();

    private IEnumerable<User> BuildUsers(int count)
    {
        for (var i = 0; i < count; i++)
        {
            yield return BuildUser(count);
        }
    }

    private IUserGroup BuildUserGroup(string alias) =>
        _userGroupBuilder
            .WithAlias(alias)
            .WithName(alias)
            .WithAllowedSections(new List<string> { "A", "B" })
            .Build();

    private IEnumerable<IUserGroup> BuildUserGroups(int count)
    {
        for (var i = 0; i < count; i++)
        {
            yield return BuildUserGroup(i.ToString());
        }
    }

    private void AddPropType(string alias, int dataTypeId, IContentType ct)
    {
        var contentCollection = new PropertyTypeCollection(true)
        {
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext)
            {
                Alias = alias,
                Name = "Title",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = dataTypeId
            }
        };
        var pg = new PropertyGroup(contentCollection) { Alias = "test", Name = "test", SortOrder = 1 };
        ct.PropertyGroups.Add(pg);
    }
}
