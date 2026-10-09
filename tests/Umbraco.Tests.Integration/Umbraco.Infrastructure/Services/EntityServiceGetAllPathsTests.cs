// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[Category("Slow")]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class EntityServiceGetAllPathsTests : UmbracoIntegrationTest
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IEntityRepository EntityRepository => GetRequiredService<IEntityRepository>();

    private ICoreScopeProvider CoreScopeProvider => GetRequiredService<ICoreScopeProvider>();

    [Test]
    [Explicit("Slow test that requires LocalDb to reproduce the SQL Server 2100 parameter limit. Run manually to verify the batching fix.")]
    public async Task GetAllPaths_By_Ids_Returns_All_Paths_In_Batches()
    {
        // Create enough content items to exceed SQL Server's hard limit of 2100 parameters,
        // which is above Constants.Sql.MaxParameterCount (2000). Without batching, the
        // WhereIn clause will fail with "The incoming request has too many parameters".
        //
        // NOTE: SQLite does not enforce a parameter limit, so this test verifies functional
        // correctness on SQLite and will catch the SQL Server regression when run with LocalDb.
        const int sqlServerParameterLimit = 2100;
        var itemCount = sqlServerParameterLimit + 10;

        var contentType = await CreateContentType();

        var root = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(root);

        var ids = new List<int>();
        for (var i = 0; i < itemCount; i++)
        {
            var child = ContentBuilder.CreateSimpleContent(contentType, $"Item {i}", root);
            ContentService.Save(child);
            ids.Add(child.Id);
        }

        // Use a manually-managed scope so the SQL error surfaces directly
        // rather than being masked by autoComplete scope disposal.
        using ICoreScope scope = CoreScopeProvider.CreateCoreScope();

        var objectTypeGuid = Constants.ObjectTypes.Document;
        TreeEntityPath[]? paths = null;

        // This should not throw SqlException "too many parameters".
        Assert.DoesNotThrow(() =>
            paths = EntityRepository.GetAllPaths(objectTypeGuid, ids.ToArray()).ToArray());

        Assert.IsNotNull(paths);
        Assert.AreEqual(ids.Count, paths!.Length, "Should return a path for every requested ID.");
        foreach (var id in ids)
        {
            Assert.IsTrue(paths.Any(p => p.Id == id), $"Missing path for ID {id}");
        }

        scope.Complete();
    }

    [Test]
    [Explicit("Slow test that requires LocalDb to reproduce the SQL Server 2100 parameter limit. Run manually to verify the batching fix.")]
    public async Task GetAllPaths_By_Keys_Returns_All_Paths_In_Batches()
    {
        // Same verification as the ID-based test but for the Guid key overload.
        const int sqlServerParameterLimit = 2100;
        var itemCount = sqlServerParameterLimit + 10;

        var contentType = await CreateContentType();

        var root = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(root);

        var keys = new List<Guid>();
        for (var i = 0; i < itemCount; i++)
        {
            var child = ContentBuilder.CreateSimpleContent(contentType, $"Item {i}", root);
            ContentService.Save(child);
            keys.Add(child.Key);
        }

        using ICoreScope scope = CoreScopeProvider.CreateCoreScope();

        var objectTypeGuid = Constants.ObjectTypes.Document;
        TreeEntityPath[]? paths = null;

        // This should not throw SqlException "too many parameters".
        Assert.DoesNotThrow(() =>
            paths = EntityRepository.GetAllPaths(objectTypeGuid, keys.ToArray()).ToArray());

        Assert.IsNotNull(paths);
        Assert.AreEqual(keys.Count, paths!.Length, "Should return a path for every requested key.");
        foreach (var key in keys)
        {
            Assert.IsTrue(paths.Any(p => p.Key == key), $"Missing path for key {key}");
        }

        scope.Complete();
    }

    private async Task<ContentType> CreateContentType()
    {
        var template = TemplateBuilder.CreateTextPageTemplate("defaultTemplate");
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateId: template.Id);
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        return contentType;
    }
}
