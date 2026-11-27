// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine;
using Examine.Lucene;
using Examine.Lucene.Providers;
using Lucene.Net.Documents;
using Lucene.Net.Store;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using Directory = Lucene.Net.Store.Directory;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     An abstract provider containing the basic functionality to be able to query against Umbraco data.
/// </summary>
public abstract class UmbracoExamineIndex : LuceneIndex, IUmbracoIndex, IIndexDiagnostics
{
    private readonly UmbracoExamineIndexDiagnostics _diagnostics;
    private readonly ILogger<UmbracoExamineIndex> _logger;
    private readonly IRuntimeState _runtimeState;
    private readonly System.Threading.Lock _recoveryLock = new();
    private bool _hasLoggedInitLog;

    protected UmbracoExamineIndex(
        ILoggerFactory loggerFactory,
        string name,
        IOptionsMonitor<LuceneDirectoryIndexOptions> indexOptions,
        IHostingEnvironment hostingEnvironment,
        IRuntimeState runtimeState)
        : base(loggerFactory, name, indexOptions)
    {
        _runtimeState = runtimeState;
        _diagnostics = new UmbracoExamineIndexDiagnostics(this, loggerFactory.CreateLogger<UmbracoExamineIndexDiagnostics>(), hostingEnvironment, indexOptions);
        _logger = loggerFactory.CreateLogger<UmbracoExamineIndex>();
    }

    public Attempt<string?> IsHealthy() => _diagnostics.IsHealthy();

    public virtual IReadOnlyDictionary<string, object?> Metadata => _diagnostics.Metadata;

    /// <summary>
    ///     Performs special __Path and __Icon value transformations to all deriving indexes when set to true.
    /// </summary>
    protected virtual bool ApplySpecialValueTransformations => true;

    /// <summary>
    ///     When set to true Umbraco will keep the index in sync with Umbraco data automatically
    /// </summary>
    public bool EnableDefaultEventHandler { get; set; } = true;

    public bool PublishedValuesOnly { get; protected set; } = false;

    public bool SupportProtectedContent { get; protected set; } = true;

    /// <inheritdoc />
    public virtual string UniqueKeyFieldName => UmbracoExamineFieldNames.NodeKeyFieldName;

    // TODO: Remove RepopulationRequired and the Searcher/CreateIndex recovery overrides once Examine recovers from
    // index files removed from disk itself: https://github.com/Shazwazza/Examine/issues/629

    /// <summary>
    ///     Raised when the index has been cleared to recover from an unreadable state, and needs to be repopulated.
    /// </summary>
    internal event EventHandler? RepopulationRequired;

    /// <inheritdoc />
    /// <remarks>
    ///     If files of the index have been removed from disk while the index is in use, the index is cleared so that it
    ///     can be searched again, and is then repopulated.
    /// </remarks>
    public override ISearcher Searcher
    {
        get
        {
            try
            {
                return base.Searcher;
            }
            catch (Exception exception) when (IsMissingIndexFile(exception))
            {
                return RecoverSearcher(exception);
            }
        }
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Recreates the index folder on disk if it has been removed while the index is in use, so that clearing the
    ///     index can commit and a subsequent rebuild restores it.
    /// </remarks>
    public override void CreateIndex()
    {
        EnsureIndexFolderExists();
        base.CreateIndex();
    }

    /// <summary>
    ///     override to check if we can actually initialize.
    /// </summary>
    /// <remarks>
    ///     This check is required since the base examine lib will try to rebuild on startup
    /// </remarks>
    protected override void PerformDeleteFromIndex(IEnumerable<string> itemIds, Action<IndexOperationEventArgs>? onComplete)
    {
        if (CanInitialize())
        {
            base.PerformDeleteFromIndex(itemIds, onComplete);
        }
    }

    protected override void PerformIndexItems(IEnumerable<ValueSet> values, Action<IndexOperationEventArgs> onComplete)
    {
        if (CanInitialize())
        {
            base.PerformIndexItems(values, onComplete);
        }
    }

    /// <summary>
    ///     Returns true if the Umbraco application is in a state that we can initialize the examine indexes
    /// </summary>
    /// <returns></returns>
    protected bool CanInitialize()
    {
        var canInit = _runtimeState.Level == RuntimeLevel.Run;

        if (!canInit && !_hasLoggedInitLog)
        {
            _hasLoggedInitLog = true;
            _logger.LogWarning("Runtime state is not " + RuntimeLevel.Run + ", no indexing will occur");
        }

        return canInit;
    }

    private static bool IsMissingIndexFile(Exception exception)
        => exception is FileNotFoundException or DirectoryNotFoundException;

    private ISearcher RecoverSearcher(Exception missingFileException)
    {
        lock (_recoveryLock)
        {
            try
            {
                // Another caller may have recovered the index while this one waited for the lock.
                return base.Searcher;
            }
            catch (Exception exception) when (IsMissingIndexFile(exception))
            {
            }

            _logger.LogWarning(
                missingFileException,
                "Files of the index {IndexName} are missing from disk. The index will be cleared and repopulated.",
                Name);

            CreateIndex();
            ISearcher searcher = base.Searcher;
            RepopulationRequired?.Invoke(this, EventArgs.Empty);
            return searcher;
        }
    }

    private void EnsureIndexFolderExists()
    {
        // Lucene only recreates a missing folder based on an existence check it has cached, so a folder removed
        // from disk after the directory was opened is never recreated and every commit fails.
        if (GetFileSystemDirectory(GetLuceneDirectory()) is { } fileSystemDirectory)
        {
            System.IO.Directory.CreateDirectory(fileSystemDirectory.Directory.FullName);
        }
    }

    private static FSDirectory? GetFileSystemDirectory(Directory directory)
        => directory switch
        {
            FSDirectory fileSystemDirectory => fileSystemDirectory,
            NRTCachingDirectory nrtCachingDirectory => GetFileSystemDirectory(nrtCachingDirectory.Delegate),
            FilterDirectory filterDirectory => GetFileSystemDirectory(filterDirectory.Delegate),
            _ => null,
        };

    /// <summary>
    ///     This ensures that the special __Raw_ fields are indexed correctly
    /// </summary>
    /// <param name="docArgs"></param>
    protected override void OnDocumentWriting(DocumentWritingEventArgs docArgs)
    {
        Document? d = docArgs.Document;

        foreach (KeyValuePair<string, IReadOnlyList<object>> f in docArgs.ValueSet.Values
                     .Where(x => x.Key.StartsWith(UmbracoExamineFieldNames.RawFieldPrefix)).ToArray())
        {
            if (f.Value.Count > 0)
            {
                //remove the original value so we can store it the correct way
                d.RemoveField(f.Key);

                d.Add(new StoredField(f.Key, f.Value[0].ToString()));
            }
        }

        base.OnDocumentWriting(docArgs);
    }

    protected override void OnTransformingIndexValues(IndexingItemEventArgs e)
    {
        base.OnTransformingIndexValues(e);

        if (ApplySpecialValueTransformations)
        {
            ApplySpecialIndexValueTransformations(e);
        }
    }

    /// <summary>
    ///     Updates the index ValueSet with a special __Path and __Icon fields.
    /// </summary>
    private static void ApplySpecialIndexValueTransformations(IndexingItemEventArgs e)
    {
        var updatedValues = e.ValueSet.Values.ToDictionary(x => x.Key, x => (IEnumerable<object>)x.Value);

        // Ensure special __Path field
        var path = e.ValueSet.GetValue("path");
        if (path != null)
        {
            updatedValues[UmbracoExamineFieldNames.IndexPathFieldName] = path.Yield();
        }

        // Ensure special __Icon field
        if (e.ValueSet.Values.TryGetValue("icon", out IReadOnlyList<object>? icon) &&
            e.ValueSet.Values.ContainsKey(UmbracoExamineFieldNames.IconFieldName) == false)
        {
            updatedValues[UmbracoExamineFieldNames.IconFieldName] = icon;
        }

        e.SetValues(updatedValues);
    }
}
