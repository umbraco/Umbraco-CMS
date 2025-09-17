using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Infrastructure.PublishedCache.DataSource;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PublishedCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class ContentCacheTests : UmbracoIntegrationTestWithContent
{
    private ContentStore GetContentStore()
    {
        var path = Path.Combine(GetRequiredService<IHostingEnvironment>().LocalTempPath, "NuCache");
        Directory.CreateDirectory(path);

        var localContentDbPath = Path.Combine(path, "NuCache.Content.db");
        var localContentDbExists = File.Exists(localContentDbPath);
        var contentDataSerializer = new ContentDataSerializer(new DictionaryOfPropertyDataSerializer());
        var localContentDb = BTree.GetTree(localContentDbPath, localContentDbExists, new NuCacheSettings(), contentDataSerializer);

        return new ContentStore(
            GetRequiredService<IPublishedSnapshotAccessor>(),
            GetRequiredService<IVariationContextAccessor>(),
            LoggerFactory.CreateLogger<ContentCacheTests>(),
            LoggerFactory,
            GetRequiredService<IPublishedModelFactory>(), // new NoopPublishedModelFactory
            localContentDb);
    }

    private ContentNodeKit CreateContentNodeKit()
    {
        var contentData = new ContentDataBuilder()
            .WithName("Content 1")
            .WithProperties(new PropertyDataBuilder()
                .WithPropertyData("welcomeText", "Welcome")
                .WithPropertyData("welcomeText", "Welcome", "en-US")
                .WithPropertyData("welcomeText", "Willkommen", "de")
                .WithPropertyData("welcomeText", "Welkom", "nl")
                .WithPropertyData("welcomeText2", "Welcome")
                .WithPropertyData("welcomeText2", "Welcome", "en-US")
                .WithPropertyData("noprop", "xxx")
                .Build())
            .Build();

        return ContentNodeKitBuilder.CreateWithContent(
            ContentType.Id,
            1,
            "-1,1",
            draftData: contentData,
            publishedData: contentData);
    }

    [Test]
    public async Task SetLocked()
    {
        var contentStore = GetContentStore();

        using (contentStore.GetScopedWriteLock(ScopeProvider))
        {
            var contentNodeKit = CreateContentNodeKit();

            contentStore.SetLocked(contentNodeKit);

            // Try running the same operation again in an async task
            await Task.Run(() => contentStore.SetLocked(contentNodeKit));
        }
    }
}
