// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine.Lucene;
using Examine.Lucene.Providers;
using Lucene.Net.Store;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Infrastructure.Examine.Extensions;
using Umbraco.Extensions;
using Umbraco.Search.Diagnostics;
using Directory = Lucene.Net.Store.Directory;

namespace Umbraco.Cms.Infrastructure.Examine;

public class LuceneIndexDiagnostics : IIndexDiagnostics
{
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly LuceneDirectoryIndexOptions? _indexOptions;

    public LuceneIndexDiagnostics(
        LuceneIndex luceneIndex,
        ILogger<LuceneIndexDiagnostics> logger,
        IHostingEnvironment hostingEnvironment,
        IOptionsMonitor<LuceneDirectoryIndexOptions>? indexOptions)
    {
        _hostingEnvironment = hostingEnvironment;
        if (indexOptions != null)
        {
            _indexOptions = indexOptions.Get(luceneIndex.Name);
        }

        LuceneIndex = luceneIndex;
        Logger = logger;
    }

    public LuceneIndex LuceneIndex { get; }
    public ILogger<LuceneIndexDiagnostics> Logger { get; }


    public Attempt<string?> IsHealthy()
    {
        var isHealthy = LuceneIndex.IsHealthy(out Exception? indexError);
        return isHealthy ? Attempt<string?>.Succeed() : Attempt.Fail(indexError?.Message);
    }

    public long GetDocumentCount() => LuceneIndex.GetDocumentCount();

    public IEnumerable<string> GetFieldNames() => LuceneIndex.GetFieldNames();

    public virtual IReadOnlyDictionary<string, object?> Metadata
    {
        get
        {
            Directory luceneDir = LuceneIndex.GetLuceneDirectory();
            var d = new Dictionary<string, object?>
            {
                [nameof(UmbracoExamineLuceneIndex.CommitCount)] = LuceneIndex.CommitCount,
                [nameof(UmbracoExamineLuceneIndex.DefaultAnalyzer)] = LuceneIndex.DefaultAnalyzer.GetType().Name,
                ["LuceneDirectory"] = luceneDir.GetType().Name
            };

            if (luceneDir is FSDirectory fsDir)
            {
                var rootDir = _hostingEnvironment.ApplicationPhysicalPath;
                d["LuceneIndexFolder"] = fsDir.Directory.ToString().ToLowerInvariant()
                    .TrimStart(rootDir.ToLowerInvariant()).Replace("\\", " /").EnsureStartsWith('/');
            }

            if (_indexOptions != null)
            {
                if (_indexOptions.DirectoryFactory != null)
                {
                    d[nameof(LuceneDirectoryIndexOptions.DirectoryFactory)] = _indexOptions.DirectoryFactory.GetType();
                }

                if (_indexOptions.IndexDeletionPolicy != null)
                {
                    d[nameof(LuceneDirectoryIndexOptions.IndexDeletionPolicy)] =
                        _indexOptions.IndexDeletionPolicy.GetType();
                }
            }

            return d;
        }
    }
}
