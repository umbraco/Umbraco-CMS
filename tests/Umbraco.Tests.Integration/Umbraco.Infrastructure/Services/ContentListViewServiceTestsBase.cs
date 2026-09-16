using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.HostedServices;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.ServerEvents;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Search.BackOffice.DependencyInjection;
using Umbraco.Cms.Search.Core.Configuration;
using Umbraco.Cms.Search.Core.DependencyInjection;
using Umbraco.Cms.Search.Core.Persistence;
using Umbraco.Cms.Search.Core.Services;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Testing.Search;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public abstract class ContentListViewServiceTestsBase : UmbracoIntegrationTest
{
    // ContentListViewService now resolves list-view items through IContentSearchService (Umbraco.Cms.Search.BackOffice),
    // which searches the Umb_Content index. Compose the back-office search stack over an in-memory indexer/searcher so
    // the service resolves and index-backed filtering returns correct results, mirroring Umbraco.Tests.Search.Integration.
    private readonly TestIndexerAndSearcher _indexerAndSearcher = new();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        builder.AddSearchCore();
        builder.AddBackOfficeSearch();

        builder.Services.AddUnique<IBackgroundTaskQueue, ImmediateBackgroundTaskQueue>();
        builder.Services.AddUnique<IServerMessenger, LocalServerMessenger>();
        builder.Services.AddUnique<IServerEventRouter, NoOpServerEventRouter>();
        builder.Services.AddUnique<IIndexDocumentRepository, NoopIndexDocumentRepository>();

        builder.Services.AddTransient<IIndexer>(_ => _indexerAndSearcher);
        builder.Services.AddTransient<ISearcher>(_ => _indexerAndSearcher);

        builder.Services.Configure<IndexOptions>(options =>
        {
            options.RegisterContentIndex<IIndexer, ISearcher, IPublishedContentChangeStrategy>(Constants.IndexAliases.PublishedContent, UmbracoObjectTypes.Document);
            options.RegisterContentIndex<IIndexer, ISearcher, IDraftContentChangeStrategy>(Constants.IndexAliases.DraftContent, UmbracoObjectTypes.Document);
            options.RegisterContentIndex<IIndexer, ISearcher, IDraftContentChangeStrategy>(Constants.IndexAliases.DraftMedia, UmbracoObjectTypes.Media);
            options.RegisterContentIndex<IIndexer, ISearcher, IDraftContentChangeStrategy>(Constants.IndexAliases.DraftMembers, UmbracoObjectTypes.Member);
        });

        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<MediaTreeChangeNotification, MediaTreeChangeDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<MemberSavedNotification, MemberSavedDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<MemberDeletedNotification, MemberDeletedDistributedCacheNotificationHandler>();
    }

    protected IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    protected IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    protected IUserService UserService => GetRequiredService<IUserService>();

    protected async Task<IUser> GetSuperUser()
        => await UserService.GetAsync(Constants.Security.SuperUserKey);

    protected async Task AssertListViewConfiguration(ListViewConfiguration actualConfiguration, Guid expectedListViewDataTypeKey)
    {
        var actualCollectionPropertyAliases = actualConfiguration
            .IncludeProperties
            .Select(p => p.Alias)
            .WhereNotNull()
            .ToArray();

        // The configured list view
        var expectedContentListViewConfig = await GetListViewConfigurationFromListViewDataType(expectedListViewDataTypeKey);
        var expectedCollectionPropertyAliases = expectedContentListViewConfig
            .IncludeProperties
            .Select(p => p.Alias)
            .WhereNotNull()
            .ToArray();

        Assert.AreEqual(expectedCollectionPropertyAliases.Length, actualCollectionPropertyAliases.Length);
        Assert.IsTrue(expectedCollectionPropertyAliases.SequenceEqual(actualCollectionPropertyAliases));
    }

    private async Task<ListViewConfiguration> GetListViewConfigurationFromListViewDataType(Guid dataTypeKey)
    {
        IDataType? dataType = await DataTypeService.GetAsync(dataTypeKey);
        var listViewConfiguration = dataType.ConfigurationObject;
        Assert.IsTrue(listViewConfiguration is ListViewConfiguration);

        return listViewConfiguration as ListViewConfiguration;
    }
}
