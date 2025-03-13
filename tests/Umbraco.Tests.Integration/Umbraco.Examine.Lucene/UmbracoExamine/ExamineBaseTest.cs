using System.Data;
using Examine;
using Examine.Lucene.Providers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Examine.Lucene.UmbracoExamine;

[TestFixture]
public abstract class ExamineBaseTest : UmbracoIntegrationTest
{
    protected IndexInitializer IndexInitializer => Services.GetRequiredService<IndexInitializer>();

    protected IHostingEnvironment HostingEnvironment => Services.GetRequiredService<IHostingEnvironment>();

    protected IRuntimeState RunningRuntimeState { get; } = Mock.Of<IRuntimeState>(x => x.Level == RuntimeLevel.Run);

    protected IExamineManager ExamineManager => GetRequiredService<IExamineManager>();

    protected override void ConfigureTestServices(IServiceCollection services)
        => services.AddSingleton<IndexInitializer>();

    /// <summary>
    ///     Used to create and manage a testable index
    /// </summary>
    /// <param name="publishedValuesOnly"></param>
    /// <param name="index"></param>
    /// <param name="contentRebuilder"></param>
    /// <param name="contentValueSetBuilder"></param>
    /// <param name="parentId"></param>
    /// <param name="contentService"></param>
    /// <returns></returns>
    protected IDisposable GetSynchronousContentIndex(
        bool publishedValuesOnly,
        out UmbracoContentIndex index,
        out ContentIndexPopulator contentRebuilder,
        out ContentValueSetBuilder contentValueSetBuilder,
        int? parentId = null,
        IContentService contentService = null)
    {
        contentValueSetBuilder = IndexInitializer.GetContentValueSetBuilder(publishedValuesOnly);

        var sqlContext = Mock.Of<ISqlContext>(x => x.Query<IContent>() == Mock.Of<IQuery<IContent>>());
        var dbFactory = Mock.Of<IUmbracoDatabaseFactory>(x => x.SqlContext == sqlContext);

        if (contentService == null)
        {
            contentService = IndexInitializer.GetMockContentService();
        }

        contentRebuilder = IndexInitializer.GetContentIndexRebuilder(contentService, publishedValuesOnly, dbFactory);

        var luceneDir = new RandomIdRAMDirectory();

        ContentValueSetValidator validator;

        // if only published values then we'll change the validator for tests to
        // ensure we don't support protected nodes and that we
        // mock the public access service for the special protected node.
        if (publishedValuesOnly)
        {
            var publicAccessServiceMock = new Mock<IPublicAccessService>();
            publicAccessServiceMock.Setup(x => x.IsProtected(It.IsAny<string>()))
                .Returns((string path) =>
                {
                    if (path.EndsWith("," + ExamineDemoDataContentService.ProtectedNode))
                    {
                        return Attempt<PublicAccessEntry>.Succeed();
                    }

                    return Attempt<PublicAccessEntry>.Fail();
                });

            var scopeProviderMock = new Mock<IScopeProvider>();
            scopeProviderMock.Setup(x => x.CreateScope(
                    It.IsAny<IsolationLevel>(),
                    It.IsAny<RepositoryCacheMode>(),
                    It.IsAny<IEventDispatcher>(),
                    It.IsAny<IScopedNotificationPublisher>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(Mock.Of<IScope>);

            validator = new ContentValueSetValidator(
                publishedValuesOnly,
                false,
                publicAccessServiceMock.Object,
                scopeProviderMock.Object,
                parentId);
        }
        else
        {
            validator = new ContentValueSetValidator(publishedValuesOnly, parentId);
        }

        index = IndexInitializer.GetUmbracoIndexer(
            HostingEnvironment,
            RunningRuntimeState,
            luceneDir,
            validator: validator);

        var syncMode = index.WithThreadingMode(IndexThreadingMode.Synchronous);

        return new DisposableWrapper(syncMode, index, luceneDir);
    }

    private AutoResetEvent indexingHandle = new(false);

    protected async Task ExecuteAndWaitForIndexing(Action indexUpdatingAction, string indexName) =>
        await ExecuteAndWaitForIndexing<int?>(
            () =>
            {
                indexUpdatingAction();
                return null;
            }, indexName);

    /// <summary>
    /// Performs and action and waits for the specified index to be done indexing.
    /// </summary>
    /// <param name="indexUpdatingAction">The action that causes the index to be updated.</param>
    /// <param name="indexName">The name of the index to wait for rebuild.</param>
    /// <typeparam name="T">The type returned from the action.</typeparam>
    /// <returns>The result of the action.</returns>
    protected async Task<T> ExecuteAndWaitForIndexing<T> (Func<T> indexUpdatingAction, string indexName)
    {
        // Set up an action to release the handle when the index is populated.
        if (ExamineManager.TryGetIndex(indexName, out IIndex index) is false)
        {
            throw new InvalidOperationException($"Could not find index: {indexName}");
        }

        index.IndexOperationComplete += (_, _) =>
        {
            indexingHandle.Set();
        };

        // Perform the action, and wait for the handle to be freed, meaning the index is done populating.
        var result = indexUpdatingAction();
        await indexingHandle.WaitOneAsync();

        return result;
    }

    private class DisposableWrapper : IDisposable
    {
        private readonly IDisposable[] _disposables;

        public DisposableWrapper(params IDisposable[] disposables) => _disposables = disposables;

        public void Dispose()
        {
            foreach (var d in _disposables)
            {
                d.Dispose();
            }
        }
    }

    protected string GetIndexPath(string indexName)
    {
        var root = TestContext.CurrentContext.TestDirectory.Split("Umbraco.Tests.Integration")[0];
        return Path.Combine(root, "Umbraco.Tests.Integration", "umbraco", "Data", "TEMP", "ExamineIndexes", indexName);
    }
}
