// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     Tests covering the EntityService (isolated tests, new DB schema per test)
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class EntityServiceTestsIsolated : UmbracoIntegrationTest
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IEntityService EntityService => GetRequiredService<IEntityService>();

    public IContentTypeEditingService ContentTypeEditingService => GetRequiredService<IContentTypeEditingService>();

    [TestCase(false)]
    [TestCase(true)]
    public void EntityService_Can_Count_Trashed_Content_Children_At_Root(bool useTrashed)
    {
        var contentType = ContentTypeBuilder.CreateSimpleContentType();
        ContentTypeService.Save(contentType);

        var root = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(root);
        for (var i = 0; i < 10; i++)
        {
            var content = ContentBuilder.CreateSimpleContent(contentType, Guid.NewGuid().ToString(), root);
            ContentService.Save(content);

            if (i % 2 == 0)
            {
                ContentService.MoveToRecycleBin(content);
            }
        }

        // get paged entities at recycle bin root
        long total;
        var entities = useTrashed
            ? EntityService
                .GetPagedTrashedChildren(Constants.System.RecycleBinContent, UmbracoObjectTypes.Document, 0, 0, out total)
                .ToArray()
            : EntityService
                .GetPagedChildren(Constants.System.RecycleBinContentKey, [UmbracoObjectTypes.Document], [UmbracoObjectTypes.Document], 0, 0, true, out total);

        Assert.AreEqual(5, total);
        Assert.IsEmpty(entities);
    }
}
