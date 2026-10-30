using Examine;
using Examine.Lucene;
using Examine.Lucene.Directories;
using Examine.Lucene.Providers;
using Lucene.Net.Analysis.Standard;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Infrastructure.Models;
using Umbraco.Cms.Tests.Common.Testing;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Examine.Lucene.UmbracoExamine;

/// <summary>
///     Tests how a file system backed index behaves when its files are removed from disk by something outside
///     the application while the index is in use, for example an operating system clean-up of the temp folder.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.None)]
internal sealed class ExternallyRemovedIndexFilesTests : ExamineBaseTest
{
    private const string IndexName = Constants.UmbracoIndexes.InternalIndexName;

    private DirectoryInfo _baseDirectory;

    [SetUp]
    public void SetUpBaseDirectory()
        => _baseDirectory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "ExamineIndexes", Guid.NewGuid().ToString("N")));

    [TearDown]
    public void TearDownBaseDirectory()
    {
        if (_baseDirectory.Exists)
        {
            _baseDirectory.Delete(true);
        }
    }

    [Test]
    public void Search_Succeeds_When_Committed_Index_Files_Are_Removed_Before_First_Search()
    {
        using UmbracoContentIndex index = CreateFileSystemIndex();
        ValueSet[] valueSets = GetContentValueSets();
        index.IndexItems(valueSets);
        index.IndexWriter.IndexWriter.Commit();

        RemoveIndexFolder();

        Assert.DoesNotThrow(() => index.Searcher.CreateQuery().All().Execute());
    }

    [Test]
    public void RepopulationRequired_Is_Raised_When_Committed_Index_Files_Are_Removed_Before_First_Search()
    {
        using UmbracoContentIndex index = CreateFileSystemIndex();
        index.IndexItems(GetContentValueSets());
        index.IndexWriter.IndexWriter.Commit();
        var raised = 0;
        index.RepopulationRequired += (_, _) => raised++;

        RemoveIndexFolder();
        _ = index.Searcher;
        _ = index.Searcher;

        Assert.AreEqual(1, raised);
    }

    [Test]
    public void Index_Rebuild_Is_Requested_When_Index_Requires_Repopulation()
    {
        using UmbracoContentIndex index = CreateFileSystemIndex();
        index.IndexItems(GetContentValueSets());
        index.IndexWriter.IndexWriter.Commit();

        var indexRebuilder = new Mock<IIndexRebuilder>();
        indexRebuilder
            .Setup(x => x.RebuildIndexAsync(It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>()))
            .ReturnsAsync(Attempt.Succeed(IndexRebuildResult.Success));
        var handler = new RebuildOnRepopulationRequiredHandler(
            Mock.Of<IExamineManager>(x => x.Indexes == new IIndex[] { index }),
            indexRebuilder.Object,
            NullLogger<RebuildOnRepopulationRequiredHandler>.Instance);
        handler.Handle(new UmbracoApplicationStartedNotification(false));

        RemoveIndexFolder();
        _ = index.Searcher;

        indexRebuilder.Verify(x => x.RebuildIndexAsync(IndexName, It.IsAny<TimeSpan?>(), It.IsAny<bool>()), Times.Once);
    }

    [Test]
    public void Rebuild_Restores_Search_When_Committed_Index_Files_Are_Removed_Before_First_Search()
    {
        using UmbracoContentIndex index = CreateFileSystemIndex();
        ValueSet[] valueSets = GetContentValueSets();
        index.IndexItems(valueSets);
        index.IndexWriter.IndexWriter.Commit();

        RemoveIndexFolder();

        Assert.DoesNotThrow(() => index.CreateIndex());
        index.IndexItems(valueSets);

        ISearchResults results = index.Searcher.CreateQuery().All().Execute();
        Assert.AreEqual(valueSets.Length, results.TotalItemCount);
    }

    private UmbracoContentIndex CreateFileSystemIndex()
    {
        var options = new LuceneDirectoryIndexOptions
        {
            Analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion),
            Validator = new ContentValueSetValidator(false),
            FieldDefinitions = new UmbracoFieldDefinitionCollection(),
            UnlockIndex = true,
        };
        var optionsMonitor = IndexInitializer.GetOptions(IndexName, options);
        options.DirectoryFactory = new FileSystemDirectoryFactory(_baseDirectory, new UmbracoLockFactory(), optionsMonitor);

        var index = new UmbracoContentIndex(
            GetRequiredService<ILoggerFactory>(),
            IndexName,
            optionsMonitor,
            HostingEnvironment,
            RunningRuntimeState,
            IndexInitializer.GetMockLocalizationService());

        index.WithThreadingMode(IndexThreadingMode.Synchronous);
        return index;
    }

    private void RemoveIndexFolder()
    {
        var indexFolder = new DirectoryInfo(Path.Combine(_baseDirectory.FullName, IndexName));
        Assert.IsTrue(indexFolder.Exists, "The index should have been written to disk before it is removed.");

        indexFolder.Delete(true);
    }

    private static ValueSet[] GetContentValueSets()
        => new ExamineDemoDataContentService()
            .GetPublishedContentByXPath("//*[string-length(@id)>0 and number(@id)>0]")
            .Root
            .Elements()
            .Select(x => x.ConvertToValueSet(IndexTypes.Content))
            .ToArray();
}
