using System.Diagnostics;
using Umbraco.Cms.Tests.Integration.Testing;
using System.Reflection;
using Examine;
using Examine.Lucene.Providers;
using NUnit.Framework;
using Umbraco.Cms.Core.HostedServices;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.ServerEvents;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Search.Core.Cache.Language;
using Umbraco.Cms.Search.Core.DependencyInjection;
using Umbraco.Cms.Search.Core.NotificationHandlers;
using Umbraco.Cms.Search.Provider.Examine.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Tests.Search.Examine.Integration.Attributes;
using Umbraco.Tests.Search.Examine.Integration.Extensions;
using Umbraco.Tests.Search.Examine.Integration.Tests.ContentTests.IndexService;
using Umbraco.Tests.Search.Integration;

namespace Umbraco.Tests.Search.Examine.Integration.Tests;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public abstract class TestBase : UmbracoIntegrationTest
{
    // these tests all run against the Examine search provider, which does not care about the origin
    // of content changes, so the origin value does not matter.
    internal const string DefaultOrigin = "does-not-matter";

    protected static readonly Guid RootKey = Guid.Parse("D9EBF985-C65C-4341-955F-FFADA160F6D9");
    protected static readonly Guid ChildKey = Guid.Parse("C84E91B2-3351-4BA9-9906-09C2260D77EC");
    protected static readonly Guid GrandchildKey = Guid.Parse("201858C2-5AC2-4505-AC2E-E4BF38F39AC4");
    private bool _indexingComplete;

    protected DateTime CurrentDateTime { get; set; }

    protected DateTimeOffset CurrentDateTimeOffset { get; } = DateTimeOffset.Now;

    // Maximum value for default step in Umbraco 18+,  min=0.0 and step=0.000001
    protected decimal DecimalValue { get; } = 12.431167m;

    protected IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    protected IContentService ContentService => GetRequiredService<IContentService>();

    protected IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    protected ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    protected IRuntimeState RuntimeState => GetRequiredService<IRuntimeState>();


    protected void SaveAndPublish(IContent content)
    {
        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);
    }

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        builder.AddExamineSearchProviderForTest<TestIndex, TestInMemoryDirectoryFactory>();

        builder.AddSearchCore();

        builder.Services.AddUnique<IBackgroundTaskQueue, ImmediateBackgroundTaskQueue>();
        builder.Services.AddUnique<IServerMessenger, LocalServerMessenger>();
        builder.Services.AddUnique<IServerEventRouter, NoOpServerEventRouter>();
        builder.AddNotificationHandler<LanguageCacheRefresherNotification, RebuildIndexesNotificationHandler>();

        // the core ConfigureBuilderAttribute won't execute from other assemblies at the moment, so this is a workaround
        var testType = Type.GetType(TestContext.CurrentContext.Test.ClassName!);
        if (testType is not null)
        {
            MethodInfo? methodInfo = testType.GetMethod(TestContext.CurrentContext.Test.Name);
            if (methodInfo is not null)
            {
                foreach (ConfigureUmbracoBuilderAttribute attribute in methodInfo.GetCustomAttributes(typeof(ConfigureUmbracoBuilderAttribute), true).OfType<ConfigureUmbracoBuilderAttribute>())
                {
                    attribute.Execute(builder, testType);
                }
            }
        }
    }

    protected async Task WaitForIndexing(string indexAlias, Func<Task> indexUpdatingAction)
    {
        var activeIndexManager = GetRequiredService<IActiveIndexManager>();
        var physicalName = activeIndexManager.IsRebuilding(indexAlias)
            ? activeIndexManager.ResolveShadowIndexName(indexAlias)
            : activeIndexManager.ResolveActiveIndexName(indexAlias);
        var index = (LuceneIndex)GetRequiredService<IExamineManager>().GetIndex(physicalName);
        index.IndexCommitted += IndexCommited;

        await indexUpdatingAction();

        var stopWatch = Stopwatch.StartNew();

        while (_indexingComplete is false)
        {
            if (stopWatch.ElapsedMilliseconds > TimeSpan.FromSeconds(30).TotalMilliseconds)
            {
                throw new TimeoutException("Indexing timed out");
            }

            await Task.Delay(250);
        }

        _indexingComplete = false;
        index.IndexCommitted -= IndexCommited;
    }

    private void IndexCommited(object? sender, EventArgs e)
    {
        _indexingComplete = true;
    }

    protected string GetIndexAlias(bool publish) => publish ? Cms.Core.Constants.IndexAliases.PublishedContent : Cms.Core.Constants.IndexAliases.DraftContent;
}
