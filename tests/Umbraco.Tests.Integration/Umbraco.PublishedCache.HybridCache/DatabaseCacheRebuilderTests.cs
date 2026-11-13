using System.Collections.Concurrent;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[Timeout(180000)]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class DatabaseCacheRebuilderTests : UmbracoIntegrationTest
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IDatabaseCacheRebuilder DatabaseCacheRebuilder => GetRequiredService<IDatabaseCacheRebuilder>();

    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IElementPublishingService ElementPublishingService => GetRequiredService<IElementPublishingService>();

    private IIdKeyMap IdKeyMap => GetRequiredService<IIdKeyMap>();

    [Test]
    public async Task Rebuilding_During_Publishing_Keeps_Every_Published_Document_Cached()
    {
        ContentType contentType = ContentTypeBuilder.CreateBasicContentType("rebuildPage", "Rebuild Page");
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        void PublishNew(string name)
        {
            Content content = ContentBuilder.CreateBasicContent(contentType);
            content.Name = name;
            ContentService.Save(content);
            ContentService.Publish(content, ["*"]);
        }

        for (var i = 0; i < 20; i++)
        {
            PublishNew($"Existing {i}");
        }

        var errors = new ConcurrentQueue<Exception>();
        var rebuilding = new Thread(() =>
        {
            try
            {
                for (var i = 0; i < 3; i++)
                {
                    Attempt<DatabaseCacheRebuildResult> result = DatabaseCacheRebuilder.RebuildAsync(false).GetAwaiter().GetResult();
                    if (result.Success is false)
                    {
                        errors.Enqueue(new InvalidOperationException($"The rebuild didn't run: {result.Result}."));
                    }
                }
            }
            catch (Exception e)
            {
                errors.Enqueue(e);
            }
        });
        var publishing = new Thread(() =>
        {
            try
            {
                for (var i = 0; i < 20; i++)
                {
                    PublishNew($"New {i}");
                }
            }
            catch (Exception e)
            {
                errors.Enqueue(e);
            }
        });

        StartAndJoin(rebuilding, publishing);

        int[] published = ContentService.GetPagedChildren(Constants.System.Root, 0, 1000, out _, null, null, null)
            .Where(x => x.Published)
            .Select(x => x.Id)
            .ToArray();
        int[] cached = GetNodeIdsWithPublishedCacheRow();

        Assert.Multiple(() =>
        {
            Assert.That(rebuilding.IsAlive || publishing.IsAlive, Is.False, "The rebuild or the publishing didn't finish.");
            Assert.That(errors, Is.Empty);
            Assert.That(published, Has.Length.EqualTo(40));
            Assert.That(cached, Is.SupersetOf(published), "A published document has no published cache row.");
        });
    }

    [Test]
    public async Task Rebuilding_During_Publishing_Keeps_Every_Published_Element_Cached()
    {
        IContentType elementType = ContentTypeBuilder.CreateSimpleElementType();
        elementType.AllowedInLibrary = true;
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        var publishedKeys = new ConcurrentQueue<Guid>();

        void PublishNew(string name)
        {
            Attempt<ElementCreateResult, ContentEditingOperationStatus> created = ElementEditingService.CreateAsync(
                    new ElementCreateModel { ContentTypeKey = elementType.Key, Variants = [new() { Name = name }] },
                    Constants.Security.SuperUserKey)
                .GetAwaiter()
                .GetResult();
            if (created.Success is false)
            {
                throw new InvalidOperationException($"The element wasn't created: {created.Status}.");
            }

            Guid key = created.Result.Content!.Key;
            Attempt<ContentPublishingResult, ContentPublishingOperationStatus> publishResult = ElementPublishingService.PublishAsync(
                    key,
                    [new CulturePublishScheduleModel { Culture = "*", Schedule = null }],
                    Constants.Security.SuperUserKey)
                .GetAwaiter()
                .GetResult();
            if (publishResult.Success is false)
            {
                throw new InvalidOperationException($"The element wasn't published: {publishResult.Status}.");
            }

            publishedKeys.Enqueue(key);
        }

        for (var i = 0; i < 20; i++)
        {
            PublishNew($"Existing {i}");
        }

        var errors = new ConcurrentQueue<Exception>();
        var rebuilding = new Thread(() =>
        {
            try
            {
                for (var i = 0; i < 3; i++)
                {
                    Attempt<DatabaseCacheRebuildResult> result = DatabaseCacheRebuilder.RebuildAsync(false).GetAwaiter().GetResult();
                    if (result.Success is false)
                    {
                        errors.Enqueue(new InvalidOperationException($"The rebuild didn't run: {result.Result}."));
                    }
                }
            }
            catch (Exception e)
            {
                errors.Enqueue(e);
            }
        });
        var publishing = new Thread(() =>
        {
            try
            {
                for (var i = 0; i < 20; i++)
                {
                    PublishNew($"New {i}");
                }
            }
            catch (Exception e)
            {
                errors.Enqueue(e);
            }
        });

        StartAndJoin(rebuilding, publishing);

        int[] published = publishedKeys
            .Select(key => IdKeyMap.GetIdForKey(key, UmbracoObjectTypes.Element).Result)
            .ToArray();
        int[] cached = GetNodeIdsWithPublishedCacheRow();

        Assert.Multiple(() =>
        {
            Assert.That(rebuilding.IsAlive || publishing.IsAlive, Is.False, "The rebuild or the publishing didn't finish.");
            Assert.That(errors, Is.Empty);
            Assert.That(published, Has.Length.EqualTo(40));
            Assert.That(cached, Is.SupersetOf(published), "A published element has no published cache row.");
        });
    }

    private int[] GetNodeIdsWithPublishedCacheRow()
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IUmbracoDatabase database = ScopeAccessor.AmbientScope!.Database;
            return database.Fetch<ContentNuDto>(database.SqlContext.Sql()
                    .SelectAll()
                    .From<ContentNuDto>()
                    .Where<ContentNuDto>(x => x.Published))
                .Select(x => x.NodeId)
                .ToArray();
        }
    }

    private static void StartAndJoin(params Thread[] threads)
    {
        // Keep the test's ambient scope out of the worker threads.
        using (ExecutionContext.SuppressFlow())
        {
            foreach (Thread thread in threads)
            {
                thread.Start();
            }
        }

        foreach (Thread thread in threads)
        {
            thread.Join(TimeSpan.FromSeconds(90));
        }
    }
}
