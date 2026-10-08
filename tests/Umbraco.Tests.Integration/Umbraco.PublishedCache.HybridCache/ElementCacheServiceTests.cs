// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Scoping;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, PublishedRepositoryEvents = true)]
internal sealed class ElementCacheServiceTests : UmbracoIntegrationTest
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddUmbracoHybridCache();
        builder.Services.AddUnique<IServerMessenger, ScopedRepositoryTests.LocalServerMessenger>();
        builder.Services.PostConfigure<NuCacheSettings>(options => options.NuCacheSerializerType = NuCacheSerializerType.JSON);
    }

    private IElementService ElementService => GetRequiredService<IElementService>();

    private IElementCacheService ElementCacheService => GetRequiredService<IElementCacheService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private ISqlContext SqlContext => GetRequiredService<ISqlContext>();

    [Test]
    public async Task Can_Get_Draft_Element_By_Key()
    {
        // Arrange
        var (elementType, element) = await CreateAndSaveElement();

        // Act
        IPublishedElement? result = await ElementCacheService.GetByKeyAsync(element.Key, preview: true);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Key, Is.EqualTo(element.Key));
    }

    [Test]
    public async Task Can_Get_Published_Element_By_Key()
    {
        // Arrange
        var (elementType, element) = await CreateSaveAndPublishElement();

        // Act
        IPublishedElement? result = await ElementCacheService.GetByKeyAsync(element.Key, preview: false);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Key, Is.EqualTo(element.Key));
    }

    [Test]
    public async Task Cannot_Get_Unpublished_Element_Without_Preview()
    {
        // Arrange — save only, don't publish
        var (elementType, element) = await CreateAndSaveElement();

        // Act
        IPublishedElement? result = await ElementCacheService.GetByKeyAsync(element.Key, preview: false);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task Can_Get_Element_Property()
    {
        // Arrange
        var (elementType, element) = await CreateSaveAndPublishElement();

        // Act
        IPublishedElement? result = await ElementCacheService.GetByKeyAsync(element.Key, preview: false);

        // Assert
        Assert.That(result, Is.Not.Null);
        IPublishedProperty? titleProperty = result!.GetProperty("title");
        Assert.That(titleProperty, Is.Not.Null);
    }

    [Test]
    public async Task Rebuild_Creates_Draft_And_Published_Cache_Records()
    {
        // Arrange
        var (elementType, element) = await CreateSaveAndPublishElement();

        // Clear existing cache records
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            var deleteSql = SqlContext.Sql()
                .Delete<ContentNuDto>()
                .Where<ContentNuDto>(x => x.NodeId == element.Id);
            ScopeAccessor.AmbientScope!.Database.Execute(deleteSql);
        }

        // Act
        ElementCacheService.Rebuild([elementType.Id]);

        // Assert
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            var selectSql = SqlContext.Sql()
                .Select<ContentNuDto>()
                .From<ContentNuDto>()
                .Where<ContentNuDto>(x => x.NodeId == element.Id);

            var dtos = ScopeAccessor.AmbientScope!.Database.Fetch<ContentNuDto>(selectSql);

            Assert.That(dtos, Has.Count.EqualTo(2), "Should have both draft and published cache records");
            Assert.That(dtos.Any(x => x.Published == false), Is.True, "Should have a draft cache record");
            Assert.That(dtos.Any(x => x.Published == true), Is.True, "Should have a published cache record");
        }
    }

    [Test]
    public async Task Rebuild_Creates_Draft_Cache_Record_With_Property_Data()
    {
        // Arrange
        var (elementType, element) = await CreateSaveAndPublishElement();

        // Clear and rebuild
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            var deleteSql = SqlContext.Sql()
                .Delete<ContentNuDto>()
                .Where<ContentNuDto>(x => x.NodeId == element.Id);
            ScopeAccessor.AmbientScope!.Database.Execute(deleteSql);
        }

        ElementCacheService.Rebuild([elementType.Id]);

        // Assert
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            var selectSql = SqlContext.Sql()
                .Select<ContentNuDto>()
                .From<ContentNuDto>()
                .Where<ContentNuDto>(x => x.NodeId == element.Id && !x.Published);

            var dto = ScopeAccessor.AmbientScope!.Database.Fetch<ContentNuDto>(selectSql).FirstOrDefault();

            Assert.That(dto, Is.Not.Null);
            Assert.That(dto!.Data, Is.Not.Null.And.Not.Empty, "Draft cache data should not be empty");
            Assert.That(dto.Data, Does.Contain("\"title\""), "Draft cache data should contain the title property");
        }
    }

    [Test]
    public async Task Delete_Removes_Cache_Records()
    {
        // Arrange
        var (elementType, element) = await CreateSaveAndPublishElement();

        // Act
        await ElementCacheService.DeleteItemAsync(element);

        // Assert
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            var selectSql = SqlContext.Sql()
                .Select<ContentNuDto>()
                .From<ContentNuDto>()
                .Where<ContentNuDto>(x => x.NodeId == element.Id);

            var dtos = ScopeAccessor.AmbientScope!.Database.Fetch<ContentNuDto>(selectSql);

            Assert.That(dtos, Has.Count.EqualTo(0), "Cache records should be removed after delete");
        }
    }

    [Test]
    public async Task RefreshElementAsync_Writes_Draft_Cache_Record()
    {
        // Arrange
        var (elementType, element) = await CreateAndSaveElement();

        // Act — RefreshElementAsync is called automatically by the notification handler
        // Verify the draft cmsContentNu record exists
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            var selectSql = SqlContext.Sql()
                .Select<ContentNuDto>()
                .From<ContentNuDto>()
                .Where<ContentNuDto>(x => x.NodeId == element.Id && !x.Published);

            var dto = ScopeAccessor.AmbientScope!.Database.Fetch<ContentNuDto>(selectSql).FirstOrDefault();

            Assert.That(dto, Is.Not.Null, "Draft cache record should exist after save");
        }
    }

    [Test]
    public async Task RefreshElementAsync_Writes_Published_Cache_Record_On_Publish()
    {
        // Arrange
        var (elementType, element) = await CreateSaveAndPublishElement();

        // Assert — both draft and published records should exist
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            var selectSql = SqlContext.Sql()
                .Select<ContentNuDto>()
                .From<ContentNuDto>()
                .Where<ContentNuDto>(x => x.NodeId == element.Id);

            var dtos = ScopeAccessor.AmbientScope!.Database.Fetch<ContentNuDto>(selectSql);

            Assert.That(dtos.Any(x => x.Published == false), Is.True, "Draft cache record should exist");
            Assert.That(dtos.Any(x => x.Published == true), Is.True, "Published cache record should exist after publish");
        }
    }

    private async Task<(IContentType ElementType, IElement Element)> CreateAndSaveElement()
    {
        var elementType = ContentTypeBuilder.CreateSimpleElementType();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        var element = new Element("Test Element", elementType);
        element.SetValue("title", "Element Title");

        ElementService.Save(element);

        return (elementType, element);
    }

    private async Task<(IContentType ElementType, IElement Element)> CreateSaveAndPublishElement()
    {
        var elementType = ContentTypeBuilder.CreateSimpleElementType();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        var element = new Element("Test Element", elementType);
        element.SetValue("title", "Element Title");

        ElementService.Save(element);
        ElementService.Publish(element, ["*"]);

        return (elementType, element);
    }
}
