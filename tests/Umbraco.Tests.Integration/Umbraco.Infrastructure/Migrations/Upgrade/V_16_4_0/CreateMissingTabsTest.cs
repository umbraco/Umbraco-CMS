// Copyright (c) Umbraco.
// See LICENSE for more details.

using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_16_4_0;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations.Upgrade.V16_4_0;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class CreateMissingTabsTest : UmbracoIntegrationTest
{
    private IScopeProvider ScopeProvider => GetRequiredService<IScopeProvider>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    /// <summary>
    /// A verification integration test for the solution to https://github.com/umbraco/Umbraco-CMS/issues/20058
    /// provided in https://github.com/umbraco/Umbraco-CMS/pull/20303.
    [Test]
    public async Task Can_Create_Missing_Tabs()
    {
        // Prepare document types as per reproduction steps described here: https://github.com/umbraco/Umbraco-CMS/issues/20058#issuecomment-3332742559
        // - Create a new composition with a tab "Content" and inside add a group "Header" with a "Text 1" property inside.
        // - Save the composition.
        // - Create a new document type and inherit the composition created in step 2.
        // - Save the document type.
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

        // Get ID of "Header" group.
        var headerGroupId = baseContentType.PropertyGroups.First(x => x.Alias == "content/header").Id;

        // Create composed content type.
        var composedContentType = new ContentTypeBuilder()
            .WithAlias("composedType")
            .WithName("Composed Type")
            .Build();
        composedContentType.ContentTypeComposition = [baseContentType];
        await ContentTypeService.CreateAsync(composedContentType, Constants.Security.SuperUserKey);
        composedContentType = await ContentTypeService.GetAsync(composedContentType.Key);

        // Add further groups and properties to composed content type.
        var propertyType1 = new PropertyTypeBuilder()
            .WithAlias("text2")
            .WithName("Text 2")
            .WithPropertyGroupId(headerGroupId)
            .Build();
        composedContentType.AddPropertyType(propertyType1);

        var propertyType2 = new PropertyTypeBuilder()
            .WithAlias("text3")
            .WithName("Text 3")
            .WithPropertyGroupId(headerGroupId)
            .Build();
        composedContentType.AddPropertyType(propertyType2, "content/homeContent", "Home Content");

        await ContentTypeService.UpdateAsync(composedContentType, Constants.Security.SuperUserKey);

        // TODO: Assert the groups and types created in the database.

        using IScope scope = ScopeProvider.CreateScope();
        Sql<ISqlContext> groupsSql = scope.Database.SqlContext.Sql()
            .Select<PropertyTypeGroupDto>()
            .From<PropertyTypeGroupDto>()
            .WhereIn<PropertyTypeGroupDto>(x => x.ContentTypeNodeId, new[] { baseContentType.Id, composedContentType.Id });
        var groups = await scope.Database.FetchAsync<PropertyTypeGroupDto>(groupsSql); // <-- this doesn't seem correct, we have 3 groups, but from the issue description would expect 5

        Sql<ISqlContext> typesSql = scope.Database.SqlContext.Sql()
            .Select<PropertyTypeDto>()
            .From<PropertyTypeDto>()
            .WhereIn<PropertyTypeDto>(x => x.ContentTypeId, new[] { baseContentType.Id, composedContentType.Id });
        var types = await scope.Database.FetchAsync<PropertyTypeDto>(typesSql);
        scope.Complete();

        // TODO: Delete one of the group records so we get to the 13 state.

        await ExecuteMigration();

        // TODO: Re-retrieve the content types and assert that the groups and types are as expected.
        // TODO: Verify in the database that the migration has re-added the record we removed in the setup.
    }

    private async Task ExecuteMigration()
    {
        using IScope scope = ScopeProvider.CreateScope();
        await CreateMissingTabs.ExecuteMigration(scope.Database);
        scope.Complete();
    }
}
