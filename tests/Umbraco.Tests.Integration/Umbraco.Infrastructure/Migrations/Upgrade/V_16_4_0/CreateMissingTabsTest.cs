// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging.Abstractions;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_16_4_0;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.TestServerTest;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations.Upgrade.V16_4_0;

[TestFixture]
internal sealed class CreateMissingTabsTest : UmbracoTestServerTestBase
{
    private IScopeProvider ScopeProvider => GetRequiredService<IScopeProvider>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IUmbracoMapper UmbracoMapper => GetRequiredService<IUmbracoMapper>();

    /// <summary>
    /// A verification integration test for the solution to https://github.com/umbraco/Umbraco-CMS/issues/20058
    /// provided in https://github.com/umbraco/Umbraco-CMS/pull/20303.
    /// </summary>
    [Test]
    public async Task Can_Create_Missing_Tabs()
    {
        // Prepare a base and composed content type.
        (IContentType baseContentType, IContentType composedContentType) = await PrepareTestData();

        // Assert the groups and properties are created in the database and that the content type model is as expected.
        await AssertValidDbGroupsAndProperties(baseContentType.Id, composedContentType.Id);
        await AssertValidContentTypeModel(composedContentType.Key);

        // Prepare the database state as it would have been in Umbraco 13.
        await PreparePropertyGroupPersistedStateForUmbraco13(composedContentType);

        // Assert that the content type groups are now without a parent tab.
        await AssertInvalidContentTypeModel(composedContentType.Key);

        // Run the migration to add the missing tab back.
        await ExecuteMigration();

        // Re-retrieve the content types and assert that the groups and types are as expected.
        await AssertValidContentTypeModel(composedContentType.Key);

        // Verify in the database that the migration has re-added only the record we removed in the setup.
        await AssertValidDbGroupsAndProperties(baseContentType.Id, composedContentType.Id);
    }

    private async Task<(IContentType BaseContentType, IContentType ComposedContentType)> PrepareTestData()
    {
        // Prepare document types as per reproduction steps described here: https://github.com/umbraco/Umbraco-CMS/issues/20058#issuecomment-3332742559
        // - Create a new composition with a tab "Content" and inside add a group "Header" with a "Text 1" property inside.
        // - Save the composition.
        // - Create a new document type and inherit the composition created in step 2.
        // - Add a new property "Text 2" to the Content > Header group.
        // - Create a new group "Home Content", inside the "Content" tab, and add a property "Text 3".
        // - Save the document type.

        // Create base content type.
        var baseContentType = new ContentTypeBuilder()
            .WithAlias("baseType")
            .WithName("Base Type")
            .AddPropertyGroup()
                .WithAlias("content")
                .WithName("Content")
                .WithType(PropertyGroupType.Tab)
                .Done()
            .AddPropertyGroup()
                .WithAlias("content/header")
                .WithName("Header")
                .WithType(PropertyGroupType.Group)
                .AddPropertyType()
                    .WithAlias("text1")
                    .WithName("Text 1")
                    .Done()
                .Done()
            .Build();
        await ContentTypeService.CreateAsync(baseContentType, Constants.Security.SuperUserKey);
        baseContentType = await ContentTypeService.GetAsync(baseContentType.Key);

        // Create composed content type.
        var composedContentType = new ContentTypeBuilder()
            .WithAlias("composedType")
            .WithName("Composed Type")
            .AddPropertyGroup()
                .WithAlias("content")
                .WithName("Content")
                .WithType(PropertyGroupType.Tab)
                .Done()
            .AddPropertyGroup()
                .WithAlias("content/header")
                .WithName("Header")
                .WithType(PropertyGroupType.Group)
                .AddPropertyType()
                    .WithAlias("text2")
                    .WithName("Text 2")
                    .Done()
                .Done()
            .AddPropertyGroup()
                .WithAlias("content/homeContent")
                .WithName("Home Content")
                .WithType(PropertyGroupType.Group)
                .AddPropertyType()
                    .WithAlias("text3")
                    .WithName("Text 3")
                    .Done()
                .Done()
            .Build();
        composedContentType.ContentTypeComposition = [baseContentType];
        await ContentTypeService.CreateAsync(composedContentType, Constants.Security.SuperUserKey);
        composedContentType = await ContentTypeService.GetAsync(composedContentType.Key);
        return (baseContentType, composedContentType);
    }

    private async Task AssertValidDbGroupsAndProperties(int baseContentTypeId, int composedContentTypeId)
    {
        using IScope scope = ScopeProvider.CreateScope();
        Sql<ISqlContext> groupsSql = scope.Database.SqlContext.Sql()
            .Select<PropertyTypeGroupDto>()
            .From<PropertyTypeGroupDto>()
            .WhereIn<PropertyTypeGroupDto>(x => x.ContentTypeNodeId, new[] { baseContentTypeId, composedContentTypeId });
        var groups = await scope.Database.FetchAsync<PropertyTypeGroupDto>(groupsSql);
        Assert.AreEqual(5, groups.Count);

        Assert.AreEqual(1, groups.Count(x => x.ContentTypeNodeId == baseContentTypeId && x.Type == (int)PropertyGroupType.Tab));
        Assert.AreEqual(1, groups.Count(x => x.ContentTypeNodeId == baseContentTypeId && x.Type == (int)PropertyGroupType.Group));

        Assert.AreEqual(1, groups.Count(x => x.ContentTypeNodeId == composedContentTypeId && x.Type == (int)PropertyGroupType.Tab));
        Assert.AreEqual(2, groups.Count(x => x.ContentTypeNodeId == composedContentTypeId && x.Type == (int)PropertyGroupType.Group));

        Sql<ISqlContext> propertiesSql = scope.Database.SqlContext.Sql()
            .Select<PropertyTypeDto>()
            .From<PropertyTypeDto>()
            .WhereIn<PropertyTypeDto>(x => x.ContentTypeId, new[] { baseContentTypeId, composedContentTypeId });
        var types = await scope.Database.FetchAsync<PropertyTypeDto>(propertiesSql);
        Assert.AreEqual(3, types.Count);
        scope.Complete();
    }

    private async Task AssertValidContentTypeModel(Guid contentTypeKey)
    {
        var contentType = await ContentTypeService.GetAsync(contentTypeKey);
        DocumentTypeResponseModel model = UmbracoMapper.Map<DocumentTypeResponseModel>(contentType)!;
        Assert.AreEqual(3, model.Containers.Count());

        var contentTab = model.Containers.FirstOrDefault(c => c.Name == "Content" && c.Type == nameof(PropertyGroupType.Tab));
        Assert.IsNotNull(contentTab);

        var headerGroup = model.Containers.FirstOrDefault(c => c.Name == "Header" && c.Type == nameof(PropertyGroupType.Group));
        Assert.IsNotNull(headerGroup);
        Assert.IsNotNull(headerGroup.Parent);
        Assert.AreEqual(contentTab.Id, headerGroup.Parent.Id);

        var homeContentGroup = model.Containers.FirstOrDefault(c => c.Name == "Home Content" && c.Type == nameof(PropertyGroupType.Group));
        Assert.IsNotNull(homeContentGroup);
        Assert.IsNotNull(homeContentGroup.Parent);
        Assert.AreEqual(contentTab.Id, homeContentGroup.Parent.Id);
    }

    private async Task PreparePropertyGroupPersistedStateForUmbraco13(IContentType composedContentType)
    {
        // Delete one of the tab records so we get to the 13 state.
        using IScope scope = ScopeProvider.CreateScope();
        Sql<ISqlContext> deleteTabSql = scope.Database.SqlContext.Sql()
            .Delete<PropertyTypeGroupDto>()
            .Where<PropertyTypeGroupDto>(x => x.Type == (int)PropertyGroupType.Tab && x.ContentTypeNodeId == composedContentType.Id);
        var deletedCount = await scope.Database.ExecuteAsync(deleteTabSql);
        scope.Complete();
        Assert.AreEqual(1, deletedCount);
    }

    private async Task AssertInvalidContentTypeModel(Guid contentTypeKey)
    {
        var contentType = await ContentTypeService.GetAsync(contentTypeKey);
        DocumentTypeResponseModel model = UmbracoMapper.Map<DocumentTypeResponseModel>(contentType)!;
        Assert.AreEqual(2, model.Containers.Count());

        var contentTab = model.Containers.FirstOrDefault(c => c.Name == "Content" && c.Type == nameof(PropertyGroupType.Tab));
        Assert.IsNull(contentTab);

        var headerGroup = model.Containers.FirstOrDefault(c => c.Name == "Header" && c.Type == nameof(PropertyGroupType.Group));
        Assert.IsNotNull(headerGroup);
        Assert.IsNull(headerGroup.Parent);

        var homeContentGroup = model.Containers.FirstOrDefault(c => c.Name == "Home Content" && c.Type == nameof(PropertyGroupType.Group));
        Assert.IsNotNull(homeContentGroup);
        Assert.IsNull(homeContentGroup.Parent);
    }

    private async Task ExecuteMigration()
    {
        using IScope scope = ScopeProvider.CreateScope();
        await CreateMissingTabs.ExecuteMigration(scope.Database, new NullLogger<CreateMissingTabs>());
        scope.Complete();
    }
}
