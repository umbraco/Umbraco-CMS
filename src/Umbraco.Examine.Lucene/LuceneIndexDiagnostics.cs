// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Store;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Examine;
using Umbraco.Extensions;

namespace Umbraco.Cms.Examine
{
    public class LuceneIndexDiagnostics : IIndexDiagnostics
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public LuceneIndexDiagnostics(LuceneIndex index, ILogger<LuceneIndexDiagnostics> logger, IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            Index = index;
            Logger = logger;
        }

        public LuceneIndex Index { get; }
        public ILogger<LuceneIndexDiagnostics> Logger { get; }

        public int DocumentCount
        {
            get
            {
                try
                {
                    return Index.GetIndexDocumentCount();
                }
                catch (AlreadyClosedException)
                {
                    Logger.LogWarning("Cannot get GetIndexDocumentCount, the writer is already closed");
                    return 0;
                }
            }
        }

        public int FieldCount
        {
            get
            {
                try
                {
                    return Index.GetIndexFieldCount();
                }
                catch (AlreadyClosedException)
                {
                    Logger.LogWarning("Cannot get GetIndexFieldCount, the writer is already closed");
                    return 0;
                }
            }
        }

        public Attempt<string> IsHealthy()
        {
            var isHealthy = Index.IsHealthy(out var indexError);
            return isHealthy ? Attempt<string>.Succeed() : Attempt.Fail(indexError.Message);
        }

        public virtual IReadOnlyDictionary<string, object> Metadata
        {
            get
            {
                var luceneDir = Index.GetLuceneDirectory();
                var d = new Dictionary<string, object>
                {
                    [nameof(UmbracoExamineIndex.CommitCount)] = Index.CommitCount,
                    [nameof(UmbracoExamineIndex.DefaultAnalyzer)] = Index.DefaultAnalyzer.GetType().Name,
                    ["LuceneDirectory"] = luceneDir.GetType().Name
                };

                if (luceneDir is FSDirectory fsDir)
                {

                    var rootDir = _hostingEnvironment.ApplicationPhysicalPath;
                    d[nameof(UmbracoExamineIndex.LuceneIndexFolder)] = fsDir.Directory.ToString().ToLowerInvariant().TrimStart(rootDir.ToLowerInvariant()).Replace("\\", "/").EnsureStartsWith('/');
                }

                return d;
            }
        }


    }
}
